using System;
using System.Threading.Tasks;
using Lykke.Service.BitcoinCash.API.Core.Transactions;
using NBitcoin;

namespace Lykke.Service.BitcoinCash.API.Core.Operation
{
    public interface IOperationService
    {
        Task<BuildedTransactionInfo> GetOrBuildTransferTransaction(Guid operationId, 
            BitcoinAddress fromAddress,
            BitcoinAddress toAddress,
            string assetId,
            Money amountToSend, 
            bool includeFee);
    }
}
