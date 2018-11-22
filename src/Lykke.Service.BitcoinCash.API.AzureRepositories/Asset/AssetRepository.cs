using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.BitcoinCash.API.Core.Asset;
using Lykke.Service.BitcoinCash.API.Core.Constants;
using Lykke.Service.BitcoinCash.API.Core.Pagination;

namespace Lykke.Service.BitcoinCash.API.AzureRepositories.Asset
{
    public class Asset : IAsset
    {
        public string AssetId { get; set; }
        public string Address { get; set; }
        public string Name { get; set; }
        public int Accuracy { get; set; }
    }

    public class AssetRepository:IAssetRepository
    {
        private readonly IEnumerable<IAsset> _mockList;

        public AssetRepository(string defaultAssetId)
        {
            _mockList = new List<IAsset>
            {

                new Asset
                {
                    Address = Constants.Assets.BitcoinCash.Address,
                    AssetId = defaultAssetId,
                    Accuracy = Constants.Assets.BitcoinCash.Accuracy,
                    Name = Constants.Assets.BitcoinCash.Name
                }
            };
        }



        public Task<IAsset> GetDefaultAsset()
        {
            return Task.FromResult(_mockList.Single());
        }

        public Task<IPaginationResult<IAsset>> GetPaged(int take, string continuation)
        {
            return Task.FromResult(PaginationResult<IAsset>.Create(_mockList, null));
        }

        public Task<IAsset> GetById(string assetId)
        {
            return Task.FromResult(_mockList.SingleOrDefault(p=>p.AssetId == assetId));
        }
    }
}
