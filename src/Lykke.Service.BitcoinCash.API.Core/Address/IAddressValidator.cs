using NBitcoin;

namespace Lykke.Service.BitcoinCash.API.Core.Address
{
    public interface IAddressValidator
    {
        bool IsValid(string address);
        BitcoinAddress GetBitcoinAddress(string address);
        BitcoinAddress GetBitcoinAddress(string base58Date, Network network);
    }
}
