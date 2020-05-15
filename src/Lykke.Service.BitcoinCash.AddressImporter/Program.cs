using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AzureStorage.Tables;
using Lykke.Logs;
using Lykke.Logs.Loggers.LykkeConsole;
using Lykke.Service.BitcoinCash.API.AzureRepositories.Wallet;
using Lykke.Service.BitcoinCash.API.Core.Settings;
using Lykke.Service.BitcoinCash.API.Services.Address;
using Lykke.SettingsReader;
using Microsoft.Extensions.CommandLineUtils;
using MoreLinq;
using NBitcoin;
using NBitcoin.Altcoins;
using NBitcoin.RPC;
using Newtonsoft.Json;
using Polly;

namespace Lykke.Service.BitcoinCash.AddressImporter
{
    class Program
    {
        private const string SettingsUrl = "settingsUrl";
        private const string HotWallet = "hotWallet";
        private const string Timestamp = "timestamp";
        private const string BatchSize = "batchSize";

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
                { BatchSize, application.Argument(BatchSize, "Size of the addresses batch") }
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

                            arguments[Timestamp].Value,

                            arguments[BatchSize].Value
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

        private static async Task Import(string settingsUrl, string hotwallet, string timestamp, string batchSize)
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

            if (!int.TryParse(batchSize, out var batchSizeTyped))
            {
                Console.WriteLine($"{BatchSize}: {batchSize} should be a valid integer ");

                return;
            }

            var logFactory = LogFactory.Create().AddConsole();

            var settings = new SettingsServiceReloadingManager<AppSettings>(settingsUrl).Nested(x => x.BitcoinCashApi);
            
            var network = Network.GetNetwork(settings.CurrentValue.Network);
            var bcashNetwork = network == Network.Main ? BCash.Instance.Mainnet : BCash.Instance.Regtest;
            var addressValidator = new AddressValidator(network, bcashNetwork);
            var hotwalletTyped = addressValidator.GetBitcoinAddress(hotwallet);

            if (hotwalletTyped == null)
            {
                Console.WriteLine($"{HotWallet}: {hotwallet} should be a valid bitcoin address");

                return;
            }

            var authTokenBytes = Encoding.ASCII.GetBytes($"{settings.CurrentValue.Rpc.UserName}:{settings.CurrentValue.Rpc.Password}");
            var authToken = Convert.ToBase64String(authTokenBytes);
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(settings.CurrentValue.Rpc.Host),
                Timeout = TimeSpan.FromMinutes(30),
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue("Basic", authToken)
                }
            };
            
            var observableWalletRepository = new ObservableWalletRepository(AzureTableStorage<ObservableWalletEntity>.Create(
                settings.Nested(p => p.Db.DataConnString),
                "ObservableWalletsV2", logFactory));
            
            Console.WriteLine("Retrieving observable wallets");

            var observableWallets = await observableWalletRepository.GetAll();

            var walletsToImport = observableWallets
                .Select(p => addressValidator.GetBitcoinAddress(p.Address))
                .Concat(new[] {hotwalletTyped})
                .ToList();

            Console.WriteLine($"Importing {walletsToImport.Count} addresses in node started at {DateTime.UtcNow}. Timestamp {timeStampTyped}. Batch size {batchSize}");

            var batchNum = 1;

            foreach (var batch in walletsToImport.Batch(batchSizeTyped))
            {
                Console.WriteLine($"{DateTime.UtcNow} Importing batch {batchNum++}...");
                
                var payload = new
                {
                    jsonrpc = "1.0",
                    id = Guid.NewGuid().ToString(),
                    method = "importmulti",
                    @params = new object[]
                    {
                        batch.Select(address => new ImportMultiAddress
                        {
                            ScriptPubKey = new ImportMultiAddress.ScriptPubKeyObject(address),
                            Timestamp = new DateTimeOffset(timeStampTyped),
                        }),
                        true
                    }
                };

                await Policy
                    .Handle<Exception>()
                    .RetryAsync(5, (e, i) =>
                    {
                        Console.WriteLine($"Retrying ({i})...");
                    })
                    .ExecuteAsync(async () =>
                    {
                        using (var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"))
                        {
                            using (var response = await httpClient.PostAsync("", content))
                            {
                                if (response.StatusCode != HttpStatusCode.OK)
                                {
                                    Console.WriteLine($"Failed to execute RPC call. Response: {response.StatusCode}");
                                    throw new InvalidOperationException($"Failed to execute RPC call. Response: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                                }
                            }
                        }
                    });

                Console.WriteLine($"{DateTime.UtcNow} Batch imported");
            }

            Console.WriteLine($"Import completed at {DateTime.UtcNow}");
        }
    }
}
