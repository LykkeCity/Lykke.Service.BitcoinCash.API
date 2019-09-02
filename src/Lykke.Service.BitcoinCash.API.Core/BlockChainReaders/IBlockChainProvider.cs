using System.Collections.Generic;
using System.Threading.Tasks;
using NBitcoin;

namespace Lykke.Service.BitcoinCash.API.Core.BlockChainReaders
{
    public interface IBlockChainProvider
    {
        Network Network { get; }

        Task BroadCastTransaction(Transaction tx);
        Task<int> GetTxConfirmationCount(string txHash);
        Task ImportWatchOnlyAddress(string address);
        Task<IList<Coin>> GetUnspentOutputs(string address, int minConfirmationCount);        
        Task<long> GetBalanceSatoshiFromUnspentOutputs(string address, int minConfirmationCount);
        Task<int> GetLastBlockHeight();
        Task<IEnumerable<string>> GetTransactionsForAddress(string address);
        Task<BchTransaction> GetTransaction(string txHash);
    }
}
