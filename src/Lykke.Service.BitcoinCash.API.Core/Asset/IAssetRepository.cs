using System.Threading.Tasks;
using Lykke.Service.BitcoinCash.API.Core.Pagination;

namespace Lykke.Service.BitcoinCash.API.Core.Asset
{
    public interface IAsset
    {
        string AssetId { get; }
        string Address { get; }
        string Name { get; }
        int Accuracy { get; }
    }

    public interface IAssetRepository
    {
        Task<IAsset> GetDefaultAsset();
        Task<IPaginationResult<IAsset>> GetPaged(int take, string continuation);
        Task<IAsset> GetById(string assetId);
    }
}
