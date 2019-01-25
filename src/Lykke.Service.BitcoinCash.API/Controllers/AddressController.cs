using System.Net;
using Autofac.Features.AttributeFilters;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.BitcoinCash.API.Core.Address;
using Lykke.Service.BitcoinCash.API.Core.Constants;
using Lykke.Service.BlockchainApi.Contract.Addresses;
using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.BitcoinCash.API.Controllers
{
    public class AddressController:Controller
    {
        private readonly IAddressValidator _addressValidator;


        public AddressController(IAddressValidator addressValidator)
        {
            _addressValidator = addressValidator;
        }

        [SwaggerOperation(nameof(Validate))]
        [ProducesResponseType(typeof(AddressValidationResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [HttpGet("api/addresses/{address}/validity")]
        public AddressValidationResponse Validate(string address)
        {
            return new AddressValidationResponse
            {
                IsValid = _addressValidator.IsValid(address)
            };
        }

        [SwaggerOperation(nameof(Formats))]
        [ProducesResponseType(typeof(AddressValidationResponse), (int)HttpStatusCode.OK)]
        [HttpGet("api/addresses/{address}/formats")]
        public ActionResult Formats(string address)
        {
            var addr = _addressValidator.GetBitcoinAddress(address);
            return Ok(new 
            {
                isValid = addr!=null,
                bCashFormat = addr!= null ? addr.ScriptPubKey.GetDestinationAddress(_addressValidator.GetBcashNetwork()).ToString() : null,
                obsoleteFormat = addr != null ? addr.ScriptPubKey.GetDestinationAddress(_addressValidator.GetObsoleteNetwork()).ToString() : null
            });
        }
    }
}
