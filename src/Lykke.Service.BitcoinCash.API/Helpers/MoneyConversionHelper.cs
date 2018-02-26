using Lykke.Service.BitcoinCash.API.Core.Constants;
using Lykke.Service.BlockchainApi.Contract;
using NBitcoin;

namespace Lykke.Service.BitcoinCash.API.Helpers
{
    public class MoneyConversionHelper
    {
        public static string SatoshiToContract(long satoshi)
        {
            return Conversions.CoinsToContract(new Money(satoshi).ToUnit(MoneyUnit.BTC), Constants.Assets.BitcoinCash.Accuracy);
        }

        public static long SatoshiFromContract(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return 0;
            }

            var btc = Conversions.CoinsFromContract(input, Constants.Assets.BitcoinCash.Accuracy);

            return new Money(btc, MoneyUnit.BTC).Satoshi;
        }
    }
}
