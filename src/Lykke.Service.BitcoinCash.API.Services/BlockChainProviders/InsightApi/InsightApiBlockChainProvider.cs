﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Flurl;
using Flurl.Http;
using Lykke.Service.BitcoinCash.API.Core.Address;
using Lykke.Service.BitcoinCash.API.Core.BlockChainReaders;
using Lykke.Service.BitcoinCash.API.Core.Domain.Health.Exceptions;
using Lykke.Service.BitcoinCash.API.Services.BlockChainProviders.InsightApi.Contracts;
using NBitcoin;
using NBitcoin.Altcoins;

namespace Lykke.Service.BitcoinCash.API.Services.BlockChainProviders.InsightApi
{
    internal class InsightApiBlockChainProvider : IBlockChainProvider
    {
        private readonly InsightApiSettings _insightApiSettings;
        private readonly IAddressValidator _addressValidator;
        private readonly Network _network;
        private readonly ILog _log;

        public InsightApiBlockChainProvider(InsightApiSettings insightApiSettings, IAddressValidator addressValidator, Network network, ILog log)
        {
            _insightApiSettings = insightApiSettings;
            _addressValidator = addressValidator;
            _network = network;
            _log = log;
        }


        public async Task<int> GetLastBlockHeight()
        {
            var url = _insightApiSettings.Url.AppendPathSegment("api/status");
            var resp = await GetJson<StatusResponceContract>(url);
            return resp.Info.LastBlockHeight;
        }


        public async Task BroadCastTransaction(Transaction tx)
        {
            await _insightApiSettings.Url.AppendPathSegment("api/tx/send")
                .PostJsonAsync(new BroadcastTransactionRequestContract
                {
                    RawTx = tx.ToHex()
                }).ReceiveString();
        }

        public async Task<int> GetTxConfirmationCount(string txHash)
        {
            return (await GetTx(txHash))?.Confirmation ?? 0;
        }

        public async Task<IList<Coin>> GetUnspentOutputs(string address, int minConfirmationCount)
        {
            var resp = await GetUnspentOutputsResponce(address);

            return resp.Where(p => p.Confirmation >= minConfirmationCount).Select(MapUnspentCoun).ToList();
        }

        public async Task<long> GetBalanceSatoshiFromUnspentOutputs(string address, int minConfirmationCount)
        {
            var unspentOutputs = (await GetUnspentOutputsResponce(address)).Where(p => p.Confirmation >= minConfirmationCount);

            return unspentOutputs.Sum(p => p.Satoshi);
        }

        private async Task<IEnumerable<AddressUnspentOutputsResponce>> GetUnspentOutputsResponce(string address)
        {
            if (_network == Network.Main)
            {
                var parsedAddress = _addressValidator.GetBitcoinAddress(address);
                address = parsedAddress.ScriptPubKey.GetDestinationAddress(BCash.Instance.Mainnet).ToString();
            }
            var url = _insightApiSettings.Url.AppendPathSegment($"api/addr/{address}/utxo");

            return await GetJson<AddressUnspentOutputsResponce[]>(url);
        }

        private Coin MapUnspentCoun(AddressUnspentOutputsResponce source)
        {
            return new Coin(new OutPoint(uint256.Parse(source.TxHash), source.N), new TxOut(new Money(source.Satoshi, MoneyUnit.Satoshi), source.ScriptPubKey.ToScript()));
        }

        private async Task<TxResponceContract> GetTx(string txHash)
        {
            try
            {
                var url = _insightApiSettings.Url
                    .AppendPathSegment($"api/tx/{txHash}");

                var resp = await GetJson<TxResponceContract>(url);

                return resp;
            }
            catch (FlurlHttpException e) when (e.Call.Response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        private async Task<T> GetJson<T>(Url url, int tryCount = 3)
        {
            bool NeedToRetryException(Exception ex)
            {
                if (!(ex is FlurlHttpException flurlException))
                    return false;

                if (flurlException is FlurlHttpTimeoutException)
                    return true;

                if (flurlException.Call.HttpStatus == HttpStatusCode.ServiceUnavailable ||
                    flurlException.Call.HttpStatus == HttpStatusCode.InternalServerError)
                    return true;
                return false;
            }

            try
            {

                return await Retry.Try(() => url.GetJsonAsync<T>(), NeedToRetryException, tryCount, _log);
            }
            catch (FlurlHttpException e)
            {
                throw new BusinessException("Error while proceeding operation within Blockchain Insight Api", ErrorCode.BlockChainApiError, e);
            }
        }
    }
}