using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AzureStorage.Tables;
using Lykke.Logs;
using Lykke.Logs.Loggers.LykkeConsole;
using Lykke.Service.BitcoinCash.API.AzureRepositories.Wallet;
using Lykke.Service.BitcoinCash.API.Core.Helpers;
using Lykke.Service.BitcoinCash.API.Core.Settings;
using Lykke.Service.BitcoinCash.API.Core.Wallet;
using Lykke.Service.BitcoinCash.API.Services.Address;
using Lykke.Service.BitcoinCash.API.Services.BlockChainProviders.InsightApi;
using Lykke.Service.BitcoinCash.API.Services.Operations;
using Lykke.Service.BitcoinCash.API.Services.Wallet;
using Lykke.SettingsReader;
using Microsoft.Extensions.CommandLineUtils;
using NBitcoin;
using NBitcoin.Altcoins;
using NBitcoin.RPC;

namespace Lykke.Tool.BitcoinCashAddressTransformer
{
    class Program
    {
        private const string SettingsUrl = "settingsUrl";

        static void Main(string[] args)
        {
            var application = new CommandLineApplication
            {
                Description = "Transfers observable wallets to actual bitcoin cash address format"
            };

            var arguments = new Dictionary<string, CommandArgument>
            {
                { SettingsUrl, application.Argument(SettingsUrl, "Url of a Bitcoin cash service settings.") }
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
                        await Execute
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

        private static async Task Execute(string settingsUrl)
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
            
            var addressValidator = new AddressValidator(network, bcashNetwork);

            var observableWalletRepository = new ObservableWalletRepository(AzureTableStorage<ObservableWalletEntity>.Create(
                settings.Nested(p => p.Db.DataConnString),
                "ObservableWallets", logFactory));

            var observableWalletV2Repository = new ObservableWalletRepository(AzureTableStorage<ObservableWalletEntity>.Create(
                settings.Nested(p => p.Db.DataConnString),
                "ObservableWalletsV2", logFactory));

            var walletBalanceV2Repo = new WalletBalanceRepository(
                AzureTableStorage<WalletBalanceEntity>.Create(settings.Nested(p => p.Db.DataConnString),
                    "WalletBalancesV2", logFactory));

            var bcProvider = new RpcBlockchainProvider(new RPCClient(
                new NetworkCredential(settings.CurrentValue.Rpc.UserName, settings.CurrentValue.Rpc.Password),
                new Uri(settings.CurrentValue.Rpc.Host),
                bcashNetwork), addressValidator, logFactory.CreateLog("temp"));

            var walletBalanceService = new WalletBalanceService(walletBalanceV2Repo,
                observableWalletV2Repository,
                bcProvider,
                new OperationsConfirmationsSettings
                {
                    MinConfirmationsToDetectOperation = settings.CurrentValue.MinConfirmationsToDetectOperation
                });

            Console.WriteLine("Retrieving observable wallets");

            var observableWallets = (await observableWalletRepository.GetAll()).ToList();

            var obserwabletWalletsTransformation = observableWallets.ToDictionary(observableWallet=>observableWallet.Address, observableWallet =>
            {
                var addr = addressValidator.GetBitcoinAddress(observableWallet.Address);

                if (addr == null)
                {
                    throw new ArgumentException($"Unable to recognize address {observableWallet.Address}", nameof(observableWallet.Address));
                }

                var oldAdrr = addr.ScriptPubKey.GetDestinationAddress(network).ToString();
                var newAddr = addr.ScriptPubKey.GetDestinationAddress(bcashNetwork).ToString();

                if (!string.Equals(observableWallet.Address, oldAdrr, StringComparison.InvariantCultureIgnoreCase))
                {
                    var prevColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{observableWallet.Address} not in obsolete format. Old format: {oldAdrr} | New Format {newAddr}");
                    Console.ForegroundColor = prevColor;
                }

                return newAddr;
            });

            var refillingProgress = 0;
            Console.WriteLine("Filling table ObservableWalletsV2");
            await observableWallets.ForEachAsyncSemaphore(8, async observableWallet =>
            {
                var progress = Interlocked.Increment(ref refillingProgress);
                var newAddress = obserwabletWalletsTransformation[observableWallet.Address];

                Console.WriteLine($"Inserting obserwablewallet record {observableWallet.Address} => {newAddress} " +
                                  $"-- {progress} of {observableWallets.Count}");

                await observableWalletV2Repository.Insert(ObservableWallet.Create(newAddress));
            });

            var updatingBalanceProgress = 0;
            Console.WriteLine("Filling table WalletBalancesV2");
            await observableWallets.ForEachAsyncSemaphore(8, async observableWallet =>
            {
                var progress = Interlocked.Increment(ref updatingBalanceProgress);

                var newAddress = obserwabletWalletsTransformation[observableWallet.Address];

                Console.WriteLine($"Updating balance record {observableWallet.Address} => {newAddress} " +
                                  $"-- {progress} of {observableWallets.Count}");

                await walletBalanceService.UpdateBalance(newAddress);
            });

            Console.WriteLine("All done!");
        }
    }
}
