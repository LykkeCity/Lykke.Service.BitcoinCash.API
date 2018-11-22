using System;
using Autofac.Features.AttributeFilters;
using Lykke.Service.BitcoinCash.API.Core.Address;
using Lykke.Service.BitcoinCash.API.Core.Constants;
using NBitcoin;

namespace Lykke.Service.BitcoinCash.API.Services.Address
{
    public class AddressValidator : IAddressValidator
    {
        private readonly Network _network;
        private readonly Network _bcashNetwork;

        public AddressValidator(Network network, [KeyFilter(Constants.BCashFilter)] Network bcashNetwork)
        {
            _network = network;
            _bcashNetwork = bcashNetwork;
        }


        public bool IsValid(string address)
        {
            var addr = GetBitcoinAddress(address);

            return addr != null;
        }


        public BitcoinAddress GetBitcoinAddress(string base58Date, Network network)
        {
            try
            {
                return BitcoinAddress.Create(base58Date, network);
            }
            catch (Exception)
            {
                try
                {
                    return new BitcoinColoredAddress(base58Date, network).Address;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }


        public BitcoinAddress GetBitcoinAddress(string base58Data)
        {
            var legacyAddress = GetBitcoinAddress(base58Data, _network);
            if (legacyAddress != null)
                return legacyAddress;

            return GetBitcoinAddress(base58Data, _bcashNetwork);
        }
    }
}
