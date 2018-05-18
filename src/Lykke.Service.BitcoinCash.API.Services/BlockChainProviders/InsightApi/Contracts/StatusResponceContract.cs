using Newtonsoft.Json;

namespace Lykke.Service.BitcoinCash.API.Services.BlockChainProviders.InsightApi.Contracts
{
    internal class StatusResponceContract
    {
        [JsonProperty("info")]
        public InsightApiStatusInfoContract Info { get; set; }
    }

    internal class InsightApiStatusInfoContract
    {
        [JsonProperty("blocks")]
        public int LastBlockHeight { get; set; }
    }
}
