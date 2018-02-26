using System.Threading.Tasks;

namespace Lykke.Service.BitcoinCash.API.Core.Fee
{

    public interface IFeeRateFacade
    {
        Task<int> GetFeePerByte();
    }
}
