﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AsyncFriendlyStackTrace;
using Common.Log;
using Lykke.Service.BitcoinCash.API.Core.Address;
using Lykke.Service.BitcoinCash.API.Core.BlockChainReaders;
using NBitcoin;
using NBitcoin.RPC;

namespace Lykke.Service.BitcoinCash.API.Services.BlockChainProviders.InsightApi
{
    public class RpcBlockchainProvider:IBlockChainProvider
    {
        private readonly RPCClient _client;
        private readonly IAddressValidator _addressValidator;
        private readonly ILog _log;

        public RpcBlockchainProvider(RPCClient client, IAddressValidator addressValidator, ILog log)
        {
            _client = client;
            _addressValidator = addressValidator;
            _log = log;
        }

        public Task BroadCastTransaction(Transaction tx)
        {
            return _client.SendRawTransactionAsync(tx);
        }

        public async Task<int> GetTxConfirmationCount(string txHash)
        {
            try
            {
                var tx = await _client.GetRawTransactionInfoAsync(uint256.Parse(txHash));

                return (int)tx.Confirmations;
            }
            catch (RPCException e) when(e.RPCCode == RPCErrorCode.RPC_INVALID_ADDRESS_OR_KEY)
            {
                await _log.WriteInfoAsync(nameof(RpcBlockchainProvider), nameof(GetTxConfirmationCount), $"Tx not found {e.ToAsyncString()}");

                return 0;
            }
        }

        public Task ImportWatchOnlyAddress(string address)
        {
            var btcAddr = _addressValidator.GetBitcoinAddress(address);

            if (btcAddr == null)
            {
                throw new ArgumentException("Unable to recognize address", nameof(address));
            }

            return _client.ImportAddressAsync(btcAddr);
        }

        public async Task<IList<Coin>> GetUnspentOutputs(string address, int minConfirmationCount)
        {
            var btcAddr = _addressValidator.GetBitcoinAddress(address);

            if (btcAddr == null)
            {
                throw new ArgumentException("Unable to recognize address", nameof(address));
            }

            var rpcResponce = await _client.ListUnspentAsync(minConfirmationCount, int.MaxValue, btcAddr);

            return rpcResponce.Select(p => new Coin(p.OutPoint, new TxOut(p.Amount, p.ScriptPubKey))).ToList();
        }

        public async Task<long> GetBalanceSatoshiFromUnspentOutputs(string address, int minConfirmationCount)
        {
            var unspentOutputs = await GetUnspentOutputs(address, minConfirmationCount);

            return unspentOutputs.Sum(p => p.Amount.Satoshi);
        }

        public Task<int> GetLastBlockHeight()
        {
            return _client.GetBlockCountAsync();
        }

        public Task<IEnumerable<string>> GetTransactionsForAddress(string address)
        {
            throw new NotImplementedException();
        }

        public Task<BchTransaction> GetTransaction(string txHash)
        {
            throw new NotImplementedException();
        }
    }
}
