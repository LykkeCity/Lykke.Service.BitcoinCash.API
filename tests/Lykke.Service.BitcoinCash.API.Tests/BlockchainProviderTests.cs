using System;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.BitcoinCash.API.Core.Domain.Health.Exceptions;
using Lykke.Service.BitcoinCash.API.Services.Address;
using Lykke.Service.BitcoinCash.API.Services.BlockChainProviders.InsightApi;
using NBitcoin;
using NBitcoin.RPC;
using Xunit;

namespace Lykke.Service.BitcoinCash.API.Tests
{
    public class BlockchainProviderTests
    {
        private readonly RpcBlockchainProvider _provider;

        public BlockchainProviderTests()
        {
            string username = "";
            string password = "";
            Uri host = new Uri("");

            var rpcClient = new RPCClient(new NetworkCredential(username, password), host, Network.Main);
            var validator = new AddressValidator(Network.Main, Network.Main);
            _provider = new RpcBlockchainProvider(rpcClient, validator, new EmptyLog());
        }

        [Fact(Skip = "Manual testing")]
        public async Task HandleRpcException()
        {
            var tx = Transaction.Parse("", Network.Main);

            var exception = await Assert.ThrowsAsync<BusinessException>(
                async () => await _provider.BroadCastTransaction(tx));

            Assert.Equal(ErrorCode.TransactionAlreadyBroadcasted, exception.Code);
        }
    }
}
