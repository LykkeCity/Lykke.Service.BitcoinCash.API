using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.BitcoinCash.API.Core.Settings.ServiceSettings
{
    public class BitcoinCashApiSettings
    {
        public DbSettings Db { get; set; }
        public string Network { get; set; }

        [HttpCheck("status")]
        public string InsightApiUrl { get; set; }

        public string RpcHost { get; set; }

        public string RpcUserName { get; set; }

        public string RpcPassword { get; set; }

        [Optional]
        public int FeePerByte { get; set; } = 1;

        [Optional]
        public long MinFeeValue { get; set; } = 100;
        [Optional]
        public long MaxFeeValue { get; set; } = 10000000;
        
        [Optional]
        public int MinConfirmationsToDetectOperation { get; set; } = 3;

        [Optional]
        public double SpentOutputsExpirationDays { get; set; } = 7;
    }
}
