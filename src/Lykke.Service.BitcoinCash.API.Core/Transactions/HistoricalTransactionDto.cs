using System;

namespace Lykke.Service.BitcoinCash.API.Core.Transactions
{
    public class HistoricalTransactionDto
    {
        public DateTime TimeStamp { get; set; }

        public string FromAddress { get; set; }
        public string ToAddress { get; set; }

        public string AssetId { get; set; }

        public long AmountSatoshi { get; set; }

        public string TxHash { get; set; }

        public bool IsSending { get; set; }
    }
}