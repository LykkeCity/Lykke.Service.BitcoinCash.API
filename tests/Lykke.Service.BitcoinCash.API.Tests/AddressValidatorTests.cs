using Lykke.Service.BitcoinCash.API.Services.Address;
using NBitcoin;
using NBitcoin.Altcoins;
using Xunit;

namespace Lykke.Service.BitcoinCash.API.Tests
{
    public class AddressValidatorTests
    {
        [Fact]
        public void CanPassValidTestnetAddress()
        {
            var addresses = new[]
            {
                "muLn6NV9aB9VLM7rJvh5i1wtUEXgDGNxW2",
                "bchreg:qq79zqy00c02p6jxx4vavx9gpl78uq8f0qmadakpzx"
            };
            var addressValidator = new AddressValidator(Network.TestNet, BCash.Instance.Regtest);

            foreach (var address in addresses)
            {
                Assert.True(addressValidator.IsValid(address));

            }
        }

        [Fact]
        public void CanPassValidMainetAddress()
        {
            var addresses = new[]
            {
                "19xM6HywehvSYfPvf3C8JVZPfE7zh1ziCD",
                "bitcoincash:qpykm9qwke5zj8je0sqqwu2tde6ej7lsvu2z4swqqj"
            };
            var addressValidator = new AddressValidator(Network.Main, BCash.Instance.Mainnet);

            foreach (var address in addresses)
            {
                Assert.True(addressValidator.IsValid(address));

            }
        }

        [Fact]
        public void CanDetectInvalidAddress()
        {

            var invalidAddress = "invalid";
            var addressValidator = new AddressValidator(Network.Main, BCash.Instance.Mainnet);

            Assert.False(addressValidator.IsValid(invalidAddress));
        }

        [Fact]
        public void CheckCashAddress()
        {
            var address = "17hFbeAibPyJ36PeH4hFDsjr35YC7wWmhv";
            var addressValidator = new AddressValidator(Network.Main, BCash.Instance.Mainnet);
            var parsedAddress = addressValidator.GetBitcoinAddress(address);
            var result = parsedAddress.ScriptPubKey.GetDestinationAddress(BCash.Instance.Mainnet).ToString();

            Assert.Equal("bitcoincash:qpykm9qwke5zj8je0sqqwu2tde6ej7lsvu2z4swqqj", result);
        }
    }
}
