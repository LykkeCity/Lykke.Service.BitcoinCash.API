using Lykke.Service.BitcoinCash.API.Services.Address;
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
    }

}
