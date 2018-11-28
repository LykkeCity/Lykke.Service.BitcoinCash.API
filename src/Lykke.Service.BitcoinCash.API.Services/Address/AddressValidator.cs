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


        public BitcoinAddress GetBitcoinAddress(string address, Network network)
        {
            try
            {
                return BitcoinAddress.Create(address, network);
            }
            catch (Exception)
            {
                try
                {
                    return new BitcoinColoredAddress(address, network).Address;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }


        public BitcoinAddress GetBitcoinAddress(string address)
        {
            var legacyAddress = GetBitcoinAddress(address, _network);
            if (legacyAddress != null)
                return legacyAddress;

            return GetBitcoinAddress(address, _bcashNetwork);
        }
    }
}
