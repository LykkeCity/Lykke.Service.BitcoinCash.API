using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.Logs;
using Lykke.Logs.Loggers.LykkeConsole;
using Lykke.Service.BitcoinCash.API.AzureRepositories.Wallet;
using Lykke.Service.BitcoinCash.API.Core.BlockChainReaders;
using Lykke.Service.BitcoinCash.API.Core.Settings;
using Lykke.Service.BitcoinCash.API.Services.Address;
using Lykke.Service.BitcoinCash.API.Services.BlockChainProviders.InsightApi;
using Lykke.SettingsReader;
using Microsoft.Extensions.CommandLineUtils;
using NBitcoin;
using NBitcoin.Altcoins;
using NBitcoin.RPC;

namespace Lykke.Service.BitcoinCash.AddressImporter
{
    class Program
    {
        private const string SettingsUrl = "settingsUrl";
        private const string HotWallet = "hotWallet";

        static void Main(string[] args)
        {
            var application = new CommandLineApplication
            {
                Description = "Imports observable wallets in bitcoin cash node"
            };

            var arguments = new Dictionary<string, CommandArgument>
            {
                { SettingsUrl, application.Argument(SettingsUrl, "Url of a Bitcoin cash service settings.") },
                { HotWallet, application.Argument(HotWallet, "Hot wallet") },
            };

            application.HelpOption("-? | -h | --help");
            application.OnExecute(async () =>
            {
                try
                {
                    if (arguments.Any(x => string.IsNullOrEmpty(x.Value.Value)))
                    {
                        application.ShowHelp();
                    }
                    else
                    {
                        await Import
                        (
                            arguments[SettingsUrl].Value,

                            arguments[HotWallet].Value

                        );
                    }

                    return 0;
                }
                catch (Exception e)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e);

                    return 1;
                }
            });

            application.Execute(args);
        }

        private static async Task Import(string settingsUrl, string hotwallet)
        {
            if (!Uri.TryCreate(settingsUrl, UriKind.Absolute, out _))
            {
                Console.WriteLine($"{SettingsUrl}: {settingsUrl} should be a valid uri");

                return;
            }

            var logFactory = LogFactory.Create().AddConsole();

            var settings = new SettingsServiceReloadingManager<AppSettings>(settingsUrl).Nested(x => x.BitcoinCashApi);
            
            BCash.Instance.EnsureRegistered();
            var network = Network.GetNetwork(settings.CurrentValue.Network);
            var bcashNetwork = network == Network.Main ? BCash.Instance.Mainnet : BCash.Instance.Regtest;

            var rpcClient = new RPCClient(
                new NetworkCredential(settings.CurrentValue.Rpc.UserName, settings.CurrentValue.Rpc.Password),
                new Uri(settings.CurrentValue.Rpc.Host),
                bcashNetwork);

            var bcProvider = new RpcBlockchainProvider(rpcClient, new AddressValidator(network, bcashNetwork),
                logFactory.CreateLog("RpcBlockchainProvider"));

            var observableWalletRepository = new ObservableWalletRepository(AzureTableStorage<ObservableWalletEntity>.Create(
                settings.Nested(p => p.Db.DataConnString),
                "ObservableWallets", logFactory));
            
            var counter = 0;



            await bcProvider.ImportWatchOnlyAddress(hotwallet);

            Console.WriteLine("Hot wallet imported");

            string continuation = null;
            do
            {
                var batchResult = await observableWalletRepository.GetAll(100, continuation);

                continuation = batchResult.ContinuationToken;

                foreach (var addr in batchResult.items)
                {
                    await bcProvider.ImportWatchOnlyAddress(addr.Address);

                    counter++;
                    Console.WriteLine($"{counter} imported -- {addr.Address}");
                }
            } while (!string.IsNullOrEmpty(continuation));

            Console.WriteLine("Import completed");
        }
    }
}
