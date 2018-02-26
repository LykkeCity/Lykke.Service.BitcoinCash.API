using System;
using System.Threading.Tasks;

namespace Lykke.Service.BitcoinCash.API.Core.Transactions
{
    public enum TransactionBlobType
    {
        Initial = 0,
        BeforeBroadcast = 1
    }

    public interface ITransactionBlobStorage
    {
        Task<string> GetTransaction(Guid operationId, TransactionBlobType type);

        Task AddOrReplaceTransaction(Guid operationId, TransactionBlobType type, string transactionHex);
    }
}
