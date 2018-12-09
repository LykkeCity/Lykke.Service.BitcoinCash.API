using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AzureStorage.Tables;
using Lykke.Logs;
using Lykke.Logs.Loggers.LykkeConsole;
using Lykke.Service.BitcoinCash.API.AzureRepositories.Wallet;
using Lykke.Service.BitcoinCash.API.Core.Settings;
using Lykke.Service.BitcoinCash.API.Services.Address;
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
        private const string Timestamp = "timestamp";

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
                { Timestamp, application.Argument(Timestamp, "Date time (ISO format) of oldest address creation. " +
                                                             "The timestamp of the oldest key will determine how far back blockchain rescans need " +
                                                             "to begin for missing wallet transactions.") },
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

                            arguments[HotWallet].Value,

                            arguments[Timestamp].Value

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

        private static async Task Import(string settingsUrl, string hotwallet, string timestamp)
        {
            if (!Uri.TryCreate(settingsUrl, UriKind.Absolute, out _))
            {
                Console.WriteLine($"{SettingsUrl}: {settingsUrl} should be a valid uri");

                return;
            }

            DateTime timeStampTyped;
            if (!DateTime.TryParse(timestamp, out timeStampTyped))
            {
                Console.WriteLine($"{Timestamp}: {timestamp} should be Date time ");

                return;
            }

            if (!DateTime.TryParse(timestamp, out timeStampTyped))
            {
                Console.WriteLine($"{Timestamp}: {timestamp} should be a valid Date time ");

                return;
            }

            var logFactory = LogFactory.Create().AddConsole();

            var settings = new SettingsServiceReloadingManager<AppSettings>(settingsUrl).Nested(x => x.BitcoinCashApi);
            
            BCash.Instance.EnsureRegistered();
            var network = Network.GetNetwork(settings.CurrentValue.Network);
            var bcashNetwork = network == Network.Main ? BCash.Instance.Mainnet : BCash.Instance.Regtest;
            
            var addressValidator = new AddressValidator(network, bcashNetwork);

            var hotwalletTyped = addressValidator.GetBitcoinAddress(hotwallet);

            if (hotwalletTyped == null)
            {
                Console.WriteLine($"{HotWallet}: {hotwallet} should be a valid bitcoin address");

                return;
            }

            var rpcClient = new RPCClient(
                new NetworkCredential(settings.CurrentValue.Rpc.UserName, settings.CurrentValue.Rpc.Password),
                new Uri(settings.CurrentValue.Rpc.Host),
                bcashNetwork);

            
            var observableWalletRepository = new ObservableWalletRepository(AzureTableStorage<ObservableWalletEntity>.Create(
                settings.Nested(p => p.Db.DataConnString),
                "ObservableWallets", logFactory));
            
            Console.WriteLine("Retrieving observable wallets");

            var observableWallets = await observableWalletRepository.GetAll();

            var walletsToImport = observableWallets
                .Select(p => addressValidator.GetBitcoinAddress(p.Address))
                .Concat(new[] {hotwalletTyped})
                .ToList();

            Console.WriteLine($"Importing {walletsToImport.Count} addresses in node started at {DateTime.UtcNow}. Timestamp {timeStampTyped}");

            await rpcClient.ImportMultiAsync(walletsToImport.Select(addr => new ImportMultiAddress
            {
                ScriptPubKey = new ImportMultiAddress.ScriptPubKeyObject(addr),
                Timestamp = new DateTimeOffset(timeStampTyped)
            }).ToArray(), rescan: true);

            Console.WriteLine($"Import completed at {DateTime.UtcNow}");
        }
    }
}
