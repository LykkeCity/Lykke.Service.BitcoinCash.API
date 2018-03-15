using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.BitcoinCash.API.Core.BlockChainReaders;
using Lykke.Service.BitcoinCash.API.Core.Broadcast;
using Lykke.Service.BitcoinCash.API.Core.Domain.Health.Exceptions;
using Lykke.Service.BitcoinCash.API.Core.ObservableOperation;
using Lykke.Service.BitcoinCash.API.Core.Operation;
using Lykke.Service.BitcoinCash.API.Core.Transactions;
using NBitcoin;

namespace Lykke.Service.BitcoinCash.API.Services.Broadcast
{
    public class BroadcastService : IBroadcastService
    {
        private readonly IBlockChainProvider _blockChainProvider;
        private readonly ILog _log;
        private readonly IUnconfirmedTransactionRepository _unconfirmedTransactionRepository;
        private readonly IOperationMetaRepository _operationMetaRepository;
        private readonly IOperationEventRepository _operationEventRepository;
        private readonly IObservableOperationRepository _observableOperationRepository;
        private readonly ITransactionBlobStorage _transactionBlobStorage;

        public BroadcastService(IBlockChainProvider blockChainProvider,
            ILog log,
            IUnconfirmedTransactionRepository unconfirmedTransactionRepository,
            IOperationMetaRepository operationMetaRepository,
            IOperationEventRepository operationEventRepository,
            IObservableOperationRepository observableOperationRepository,
            ITransactionBlobStorage transactionBlobStorage)
        {
            _blockChainProvider = blockChainProvider;
            _log = log;
            _unconfirmedTransactionRepository = unconfirmedTransactionRepository;
            _operationMetaRepository = operationMetaRepository;
            _operationEventRepository = operationEventRepository;
            _observableOperationRepository = observableOperationRepository;
            _transactionBlobStorage = transactionBlobStorage;
        }

        public async Task BroadCastTransaction(Guid operationId, Transaction tx)
        {
            var operation = await _operationMetaRepository.Get(operationId);
            if (operation == null)
            {
                throw new BusinessException("Operation not found", ErrorCode.BadInputParameter);
            }

            if (await _operationEventRepository.Exist(operationId, OperationEventType.Broadcasted))
            {
                throw new BusinessException("Transaction already brodcasted", ErrorCode.TransactionAlreadyBroadcasted);
            }
            var hash = tx.GetHash().ToString();
            await _transactionBlobStorage.AddOrReplaceTransaction(operationId, hash, TransactionBlobType.BeforeBroadcast, tx.ToHex());

            var lastBlockHeight = await _blockChainProvider.GetLastBlockHeight();

            await _blockChainProvider.BroadCastTransaction(tx);

            await _observableOperationRepository.InsertOrReplace(ObervableOperation.Create(operation, BroadcastStatus.InProgress, hash, lastBlockHeight));

            await _unconfirmedTransactionRepository.InsertOrReplace(UnconfirmedTransaction.Create(operationId, hash));

            await _operationEventRepository.InsertIfNotExist(OperationEvent.Create(operationId, OperationEventType.Broadcasted));
        }

        public async Task BroadCastTransaction(Guid operationId, string txHex)
        {
            Transaction tx;

            try
            {
                tx = Transaction.Parse(txHex);
            }
            catch (Exception e)
            {
                await _log.WriteErrorAsync(nameof(BroadcastService), nameof(BroadCastTransaction),
                    txHex, e);
                throw new BusinessException("Invalid transaction transactionContext", ErrorCode.BadInputParameter);
            }

            await BroadCastTransaction(operationId, tx);
        }
    }
}
