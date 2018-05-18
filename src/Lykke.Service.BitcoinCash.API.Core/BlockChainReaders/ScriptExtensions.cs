using System;
using System.Collections.Generic;
using System.Text;
using NBitcoin;
using NBitcoin.DataEncoders;

namespace Lykke.Service.BitcoinCash.API.Core.BlockChainReaders
{
    public static class ScriptExtensions
    {
        public static Script ToScript(this string hex)
        {
            return Script.FromBytesUnsafe(Encoders.Hex.DecodeData(hex));
        }
    }
}
