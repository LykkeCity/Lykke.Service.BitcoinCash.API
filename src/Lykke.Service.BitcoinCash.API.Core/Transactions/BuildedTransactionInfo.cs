﻿using System;
using System.Collections.Generic;
using System.Text;
using NBitcoin;
using NBitcoin.JsonConverters;
using Newtonsoft.Json;

namespace Lykke.Service.BitcoinCash.API.Core.Transactions
{
    public class BuildedTransactionInfo
    {
        public string TransactionHex { get; set; }
                
        public IEnumerable<Coin> UsedCoins { get; set; }

        public string ToJson(Network network)
        {
            return Serializer.ToString(this, network);
        }
    }
}
