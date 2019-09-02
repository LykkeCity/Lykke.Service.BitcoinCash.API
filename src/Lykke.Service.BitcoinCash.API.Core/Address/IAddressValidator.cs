using NBitcoin;

namespace Lykke.Service.BitcoinCash.API.Core.Address
{
    public interface IAddressValidator
    {
        bool IsValid(string address);
        BitcoinAddress GetBitcoinAddress(string address, Network forNetwork);
        BitcoinAddress GetBitcoinAddress(string address);
        Network GetObsoleteNetwork();
        Network GetBcashNetwork();
    }
}
