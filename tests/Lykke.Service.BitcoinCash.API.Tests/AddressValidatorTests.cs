using Lykke.Service.BitcoinCash.API.Services.Address;
using NBitcoin.Altcoins;
using Xunit;

namespace Lykke.Service.BitcoinCash.API.Tests
{
    public class AddressValidatorTests
    {
        [Fact]
        public void CanPassValidAddress()
        {         


            var addresses = new[]
            {
                "muLn6NV9aB9VLM7rJvh5i1wtUEXgDGNxW2",                
            };
            var addressValidator = new AddressValidator();

            foreach (var address in addresses)
            {
                Assert.True(addressValidator.IsValid(address));

            }


        }

        [Fact]
        public void CanDetectInvalidAddress()
        {            

            var invalidAddress = "invalid";
            var addressValidator = new AddressValidator();

            Assert.False(addressValidator.IsValid(invalidAddress));
        }

        [Fact]
        public void CheckCashAddress()
        {
            var address = "17hFbeAibPyJ36PeH4hFDsjr35YC7wWmhv";
            var addressValidator = new AddressValidator();
            var parsedAddress = addressValidator.GetBitcoinAddress(address);
            var result = parsedAddress.ScriptPubKey.GetDestinationAddress(BCash.Instance.Mainnet).ToString();

            Assert.Equal("bitcoincash:qpykm9qwke5zj8je0sqqwu2tde6ej7lsvu2z4swqqj", result);
        }
    }

}
