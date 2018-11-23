using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AsyncFriendlyStackTrace;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.BitcoinCash.API.Core.Address;
using Lykke.Service.BitcoinCash.API.Core.BlockChainReaders;
using Lykke.Service.BitcoinCash.API.Core.Domain.Health.Exceptions;
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

        public async Task BroadCastTransaction(Transaction tx)
        {

            try
            {
                await _client.SendRawTransactionAsync(tx);
            }
            catch (RPCException e) when(NeedToRebuildTransaction(e))
            {
                await _log.WriteWarningAsync(nameof(RpcBlockchainProvider), nameof(BroadCastTransaction), tx.ToHex(),
                    "Retrying tx build", e);

                throw new BusinessException($"Retrying tx build code: {e.RPCResult.Error.Code} message {e.RPCResult.Error.Message}",
                    ErrorCode.BuildingShouldBeRepeated,
                    e);
            }

            catch (RPCException e)
            {
                await _log.WriteWarningAsync(nameof(RpcBlockchainProvider), nameof(BroadCastTransaction), tx.ToHex(),
                    "Broadcast error", e);

                throw new BusinessException($"Error while tx broadcast: {e.RPCResult.Error.Code} message {e.RPCResult.Error.Message}",
                    ErrorCode.BroadcastError,
                    e);
            }
        }

        private bool NeedToRebuildTransaction(RPCException ex)
        {
            if (ex.RPCResult.Error.Code == RPCErrorCode.RPC_TRANSACTION_ERROR &&
                ex.RPCResult.Error.Message == "Missing inputs")
            {
                return true;
            }

            if (ex.RPCResult.Error.Message == "txn-mempool-conflict")
            {
                return true;
            }

            return false;
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
