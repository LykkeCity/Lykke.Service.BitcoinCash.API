using Newtonsoft.Json;

namespace Lykke.Service.BitcoinCash.API.Services.BlockChainProviders.InsightApi.Contracts
{
    public class RawTxResponce
    {
        [JsonProperty("rawtx")]
        public string RawTx { get; set; }
    }
}
