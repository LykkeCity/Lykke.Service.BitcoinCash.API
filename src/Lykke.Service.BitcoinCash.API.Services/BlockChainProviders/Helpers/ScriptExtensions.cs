using NBitcoin;
using NBitcoin.DataEncoders;

namespace Lykke.Service.BitcoinCash.API.Services.BlockChainProviders.Helpers
{
    public static class ScriptExtensions
    {
        public static Script ToScript(this string hex)
        {
            return Script.FromBytesUnsafe(Encoders.Hex.DecodeData(hex));
        }
    }
}
