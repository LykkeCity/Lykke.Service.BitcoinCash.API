﻿using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.BitcoinCash.API.Core.Address;
using Lykke.Service.BitcoinCash.API.Core.Domain.Health.Exceptions;
using Lykke.Service.BitcoinCash.API.Core.Transactions;
using Lykke.Service.BitcoinCash.API.Helpers;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.BitcoinCash.API.Controllers
{
    [Route("api/transactions/history")]
    public class HistoryController : Controller
    {
        private readonly IHistoryService _historyService;
        private readonly IAddressValidator _addressValidator;

        public HistoryController(IHistoryService historyService, IAddressValidator addressValidator)
        {
            _historyService = historyService;
            _addressValidator = addressValidator;
        }

        [HttpPost("from/{address}/observation")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public IActionResult ObserveFrom(
            [FromRoute] string address)
        {
            ValidateAddress(address);
            return Ok();
        }

        [HttpPost("to/{address}/observation")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public IActionResult ObserveTo(
            [FromRoute] string address)
        {
            ValidateAddress(address);
            return Ok();
        }

        [HttpDelete("from/{address}/observation")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public IActionResult DeleteObservationFrom(
            [FromRoute] string address)
        {
            ValidateAddress(address);
            return Ok();
        }

        [HttpDelete("to/{address}/observation")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public IActionResult DeleteObservationTo(
            [FromRoute] string address)
        {
            ValidateAddress(address);
            return Ok();
        }

        [HttpGet("from/{address}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(HistoricalTransactionContract[]))]
        public async Task<IActionResult> GetHistoryFrom(
            [FromRoute] string address,
            [FromQuery] string afterHash,
            [FromQuery] int take)
        {
            if (take <= 0)
            {
                return BadRequest(new ErrorResponse() { ErrorMessage = $"{nameof(take)} must be greater than zero" });
            }

            ValidateAddress(address);


            return Ok(Enumerable.Empty<HistoricalTransactionContract>());

            //var addr = _addressValidator.GetBitcoinAddress(address);
            //var result = await _historyService.GetHistoryFrom(addr, afterHash, take);

            //return Ok(result.Select(ToHistoricalTransaction));
        }

        [HttpGet("to/{address}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(HistoricalTransactionContract[]))]
        public async Task<IActionResult> GetHistoryTo(
            [FromRoute] string address,
            [FromQuery] string afterHash,
            [FromQuery] int take)
        {
            if (take <= 0)
            {
                return BadRequest(new ErrorResponse() { ErrorMessage = $"{nameof(take)} must be greater than zero" });
            }

            ValidateAddress(address);

            return Ok(Enumerable.Empty<HistoricalTransactionContract>());

            //var btcAddress = _addressValidator.GetBitcoinAddress(address);
            //var result = await _historyService.GetHistoryTo(btcAddress, afterHash, take);

            //return Ok(result.Select(ToHistoricalTransaction));
        }

        private void ValidateAddress(string address)
        {
            if (!_addressValidator.IsValid(address))
            {
                throw new BusinessException($"Invalid BCH address ${address}", ErrorCode.BadInputParameter);
            }
        }

        private HistoricalTransactionContract ToHistoricalTransaction(HistoricalTransactionDto source)
        {
            return new HistoricalTransactionContract
            {
                ToAddress = source.ToAddress,
                FromAddress = source.FromAddress,
                AssetId = source.AssetId,
                Amount = MoneyConversionHelper.SatoshiToContract(source.AmountSatoshi),
                Hash = source.TxHash,
                Timestamp = source.TimeStamp,
                TransactionType = source.IsSending ? TransactionType.Send : TransactionType.Receive
            };
        }
    }
}
