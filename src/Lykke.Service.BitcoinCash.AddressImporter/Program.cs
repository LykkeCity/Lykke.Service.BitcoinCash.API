﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.Logs;
using Lykke.Logs.Loggers.LykkeConsole;
using Lykke.Service.BitcoinCash.API.AzureRepositories.Wallet;
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

        static void Main(string[] args)
        {
            var application = new CommandLineApplication
            {
                Description = "Imports observable wallets in bitcoin cash node"
            };

            var arguments = new Dictionary<string, CommandArgument>
            {
                { SettingsUrl, application.Argument(SettingsUrl, "Url of a Bitcoin cash service settings.") },
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
                            arguments[SettingsUrl].Value

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

        private static async Task Import(string settingsUrl)
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

            


            Console.WriteLine("Retrieving wallets");

            var allWallets = await observableWalletRepository.GetAll();
            var total = allWallets.Count();

            var counter = 0;
            foreach (var observableWallet in allWallets)
            {
                await bcProvider.ImportWatchOnlyAddress(observableWallet.Address);

                counter++;
                Console.WriteLine($"{counter} of {total} imported -- {observableWallet.Address}");
            }

            Console.WriteLine("Import completed");
        }
    }
}
