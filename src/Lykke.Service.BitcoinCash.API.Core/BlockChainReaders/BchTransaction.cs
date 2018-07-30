using System;
using System.Collections.Generic;
using System.Text;
using NBitcoin;

namespace Lykke.Service.BitcoinCash.API.Core.BlockChainReaders
{

    public class BchTransaction
    {
        public string Hash { get; set; }

        public DateTime Timestamp { get; set; }

        public IList<BchInput> Inputs { get; set; }

        public IList<BchOutput> Outputs { get; set; }
    }

    public class BchInput
    {
        public string Address { get; set; }

        public Money Value { get; set; }
    }

    public class BchOutput
    {
        public string Address { get; set; }

        public Money Value { get; set; }
    }
}
