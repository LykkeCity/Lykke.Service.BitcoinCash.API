using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.BitcoinCash.API.Core.Address;
using Lykke.Service.BitcoinCash.API.Core.BlockChainReaders;
using Lykke.Service.BitcoinCash.API.Core.Domain.Health.Exceptions;
using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace Lykke.Service.BitcoinCash.API.Services.BlockChainProviders
{
    internal class NinjaApiBlockChainProvider : IBlockChainProvider
    {
        private readonly QBitNinjaClient _ninjaClient;
        private readonly IAddressValidator _addressValidator;

        public NinjaApiBlockChainProvider(QBitNinjaClient ninjaClient, IAddressValidator addressValidator)
        {
            _ninjaClient = ninjaClient;
            _addressValidator = addressValidator;
        }

        public async Task BroadCastTransaction(Transaction tx)
        {
            var response = await _ninjaClient.Broadcast(tx);
            if (!response.Success || response.Error?.ErrorCode == NBitcoin.Protocol.RejectCode.INVALID)
                throw new BusinessException(response.Error?.Reason, ErrorCode.BroadcastError);
        }

        public async Task<int> GetTxConfirmationCount(string txHash)
        {
            var tx = await _ninjaClient.GetTransaction(uint256.Parse(txHash));
            return tx?.Block?.Confirmations ?? 0;
        }

        public async Task<IList<Coin>> GetUnspentOutputs(string address, int minConfirmationCount)
        {
            var response = await _ninjaClient.GetBalance(_addressValidator.GetBitcoinAddress(address), true);
            return response.Operations.Where(o => o.Confirmations >= minConfirmationCount).SelectMany(o => o.ReceivedCoins).OfType<Coin>().ToList();
        }


        public async Task<long> GetBalanceSatoshiFromUnspentOutputs(string address, int minConfirmationCount)
        {
            var unspentOutputs = await GetUnspentOutputs(address, minConfirmationCount);
            return unspentOutputs.Select(o => o.Amount).DefaultIfEmpty().Sum(p => p?.Satoshi ?? 0);
        }

        public async Task<int> GetLastBlockHeight()
        {
            var block = await _ninjaClient.GetBlock(BlockFeature.Parse("tip"), true);
            return block.AdditionalInformation.Height;
        }
    }
}
