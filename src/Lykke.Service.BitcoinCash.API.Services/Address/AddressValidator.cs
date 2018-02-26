using System;
using Lykke.Service.BitcoinCash.API.Core.Address;
using NBitcoin;

namespace Lykke.Service.BitcoinCash.API.Services.Address
{
    public class AddressValidator : IAddressValidator
    {
        public bool IsValid(string address)
        {
            var addr = GetBitcoinAddress(address);

            return addr != null;
        }

        public BitcoinAddress GetBitcoinAddress(string base58data)
        {            
            try
            {
                return BitcoinAddress.Create(base58data);
            }            
            catch (Exception)
            {
                try
                {
                    return new BitcoinColoredAddress(base58data)?.Address;
                }
                catch (Exception)
                {
                    return null;
                }
            }                        
        }
    }
}
