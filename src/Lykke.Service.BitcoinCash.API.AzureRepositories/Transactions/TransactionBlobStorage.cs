using System;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.BitcoinCash.API.Core.Transactions;

namespace Lykke.Service.BitcoinCash.API.AzureRepositories.Transactions
{
    public class TransactionBlobStorage : ITransactionBlobStorage
    {
        private const string BlobContainer = "transactions";

        private readonly IBlobStorage _blobStorage;

        public TransactionBlobStorage(IBlobStorage blobStorage)
        {
            _blobStorage = blobStorage;
        }

        public async Task<string> GetTransaction(Guid operationId, TransactionBlobType type)
        {
            var key = GenerateKey(operationId, type);
            if (await _blobStorage.HasBlobAsync(BlobContainer, key))
                return await _blobStorage.GetAsTextAsync(BlobContainer, key);
            return null;
        }

        public async Task AddOrReplaceTransaction(Guid operationId, TransactionBlobType type, string transactionHex)
        {
            var key = GenerateKey(operationId, type);
            if (await _blobStorage.HasBlobAsync(BlobContainer, key))
                await _blobStorage.DelBlobAsync(BlobContainer, key);
            await _blobStorage.SaveBlobAsync(BlobContainer, key, Encoding.UTF8.GetBytes(transactionHex));
        }

        private string GenerateKey(Guid operationId, TransactionBlobType type)
        {
            return $"{operationId}.{type}.txt";
        }
    }
}
