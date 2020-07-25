using System;
using Autofac.Features.AttributeFilters;
using Lykke.Service.BitcoinCash.API.Core.Address;
using Lykke.Service.BitcoinCash.API.Core.Constants;
using NBitcoin;
using NBitcoin.Altcoins;

namespace Lykke.Service.BitcoinCash.API.Services.Address
{
    public class AddressValidator : IAddressValidator
    {
        private readonly Network _network;
        private readonly Network _bcashNetwork;
        private readonly string _bc1 = "bc1";

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


        private static BitcoinAddress ParseAddress(string address, Network network)
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

        public BitcoinAddress GetBitcoinAddress(string address, Network forNetwork)
        {
            return GetBitcoinAddress(address)?.ScriptPubKey.GetDestinationAddress(forNetwork);
        }

        public BitcoinAddress GetBitcoinAddress(string address)
        {
            if (address.StartsWith(_bc1, StringComparison.InvariantCultureIgnoreCase))
                return null;
            //eg moc231tgxApbRSwLNrc9ZbSVDktTRo3acK
            var legacyAddress = ParseAddress(address, _network);

            if (legacyAddress != null)
                return legacyAddress;

            //eg: bitcoincash:qpm2qsznhks23z7629mms6s4cwef74vcwvy22gdx6a
            var canonicalAddress = ParseAddress(address, _bcashNetwork);

            if (canonicalAddress != null)
                return canonicalAddress;

            //eg qpm2qsznhks23z7629mms6s4cwef74vcwvy22gdx6a
            var addressWithoutPrefix =
                ParseAddress($"{GetAddressPrefix(_bcashNetwork)}:{address?.Trim()}", _bcashNetwork);

            return addressWithoutPrefix;
        }

        public Network GetObsoleteNetwork()
        {
            return _network;
        }

        public Network GetBcashNetwork()
        {
            return _bcashNetwork;
        }

        private static string GetAddressPrefix(Network bcashNetwork)
        {
            if (bcashNetwork == BCash.Instance.Mainnet)
            {
                return "bitcoincash";
            }
            if (bcashNetwork == BCash.Instance.Regtest)
            {
                return "bchreg";
            }
            if (bcashNetwork == BCash.Instance.Testnet)
            {
                return "bchtest";
            }

            throw new ArgumentException("Unknown bcash forNetwork", nameof(bcashNetwork));
        }
    }
}
