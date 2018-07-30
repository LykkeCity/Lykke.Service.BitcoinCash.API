using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;

namespace Lykke.Service.BitcoinCash.API.Core.Transactions
{
    public interface IHistoryService
    {
        Task<IEnumerable<HistoricalTransactionDto>> GetHistoryFrom(BitcoinAddress address, string afterHash, int take);
        Task<IEnumerable<HistoricalTransactionDto>> GetHistoryTo(BitcoinAddress address, string afterHash, int take);
    }
}
