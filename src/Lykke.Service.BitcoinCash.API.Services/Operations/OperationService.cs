using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Lykke.Service.BitcoinCash.API.Core.Operation;
using Lykke.Service.BitcoinCash.API.Core.Transactions;
using NBitcoin;
using NBitcoin.JsonConverters;
using Newtonsoft.Json;

namespace Lykke.Service.BitcoinCash.API.Services.Operations
{
    public class OperationService : IOperationService
    {
        private readonly ITransactionBuilderService _transactionBuilder;
        private readonly IOperationMetaRepository _operationMetaRepository;
        private readonly ITransactionBlobStorage _transactionBlobStorage;
        private readonly Network _network;

        public OperationService(ITransactionBuilderService transactionBuilder,
            IOperationMetaRepository operationMetaRepository,
            ITransactionBlobStorage transactionBlobStorage, Network network)
        {
            _transactionBuilder = transactionBuilder;
            _operationMetaRepository = operationMetaRepository;
            _transactionBlobStorage = transactionBlobStorage;
            _network = network;
        }

        public async Task<BuildedTransactionInfo> GetOrBuildTransferTransaction(Guid operationId,
            BitcoinAddress fromAddress,
            BitcoinAddress toAddress,
            string assetId,
            Money amountToSend,
            bool includeFee)
        {
            if (await _operationMetaRepository.Exist(operationId))
            {
                var alreadyBuildedTransaction = await _transactionBlobStorage.GetTransaction(operationId, TransactionBlobType.Initial);

                return Serializer.ToObject<BuildedTransactionInfo>(alreadyBuildedTransaction);
            }

            var buildedTransaction = await _transactionBuilder.GetTransferTransaction(fromAddress, toAddress, amountToSend, includeFee);

            var buildedTransactionInfo = new BuildedTransactionInfo
            {
                TransactionHex = buildedTransaction.TransactionData.ToHex(),
                UsedCoins = buildedTransaction.UsedCoins
            };

            await _transactionBlobStorage.AddOrReplaceTransaction(operationId, TransactionBlobType.Initial,                
               buildedTransactionInfo.ToJson(_network));

            var operation = OperationMeta.Create(operationId, fromAddress.ToString(), toAddress.ToString(), assetId,
                buildedTransaction.Amount.Satoshi, buildedTransaction.Fee.Satoshi, includeFee);
            await _operationMetaRepository.Insert(operation);



            return buildedTransactionInfo;
        }
    }
}
