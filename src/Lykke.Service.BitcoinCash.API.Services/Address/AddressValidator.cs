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


        private BitcoinAddress GetBitcoinAddress(string base58Date, Network network)
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
            //eg moc231tgxApbRSwLNrc9ZbSVDktTRo3acK
            var legacyAddress = GetBitcoinAddress(base58Data, _network);
            if (legacyAddress != null)
                return legacyAddress;

            //eg: bitcoincash:qpm2qsznhks23z7629mms6s4cwef74vcwvy22gdx6a
            var canonicalAddress = GetBitcoinAddress(base58Data, _bcashNetwork);

            if (canonicalAddress != null)
                return canonicalAddress;

            //eg qpm2qsznhks23z7629mms6s4cwef74vcwvy22gdx6a
            var addressWithoutPrefix =
                GetBitcoinAddress($"{GetAddressPrefix(_bcashNetwork)}:{base58Data?.Trim()}", _bcashNetwork);

            return addressWithoutPrefix;
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

            throw new ArgumentException("Unknown bcash network", nameof(bcashNetwork));
        }
    }
}
