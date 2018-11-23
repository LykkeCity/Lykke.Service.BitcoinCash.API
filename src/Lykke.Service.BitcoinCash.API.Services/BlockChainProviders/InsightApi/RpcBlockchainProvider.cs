﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public RpcBlockchainProvider(RPCClient client, IAddressValidator addressValidator)
        {
            _client = client;
            _addressValidator = addressValidator;
        }

        public Task BroadCastTransaction(Transaction tx)
        {
            return _client.SendRawTransactionAsync(tx);
        }

        public async Task<int> GetTxConfirmationCount(string txHash)
        {
            var tx = await _client.GetRawTransactionInfoAsync(uint256.Parse(txHash));
            return (int) tx.Confirmations;
        }

        public Task ImportWatchOnlyAddress(string address)
        {
            return _client.ImportAddressAsync(_addressValidator.GetBitcoinAddress(address, _client.Network));
        }

        public async Task<IList<Coin>> GetUnspentOutputs(string address, int minConfirmationCount)
        {
            var rpcResponce = await _client.ListUnspentAsync(minConfirmationCount, int.MaxValue, _addressValidator.GetBitcoinAddress(address, _client.Network));

            return rpcResponce.Select(p => new Coin(p.OutPoint, new TxOut(p.Amount, p.ScriptPubKey))).ToList();
        }

        public async Task<long> GetBalanceSatoshiFromUnspentOutputs(string address, int minConfirmationCount)
        {
            var unspentOutputs = await GetUnspentOutputs(address, minConfirmationCount);

            return unspentOutputs.Sum(p => p.Amount.Satoshi);
        }

        public async Task<int> GetLastBlockHeight()
        {
            return (int) (await _client.GetBlockchainInfoAsync()).Blocks;
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