using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage.Tables;
using Lykke.Logs;
using Lykke.Logs.Loggers.LykkeConsole;
using Lykke.Service.BitcoinCash.API.AzureRepositories.Wallet;
using Lykke.Service.BitcoinCash.API.Core.Settings;
using Lykke.Service.BitcoinCash.API.Core.Wallet;
using Lykke.Service.BitcoinCash.API.Services.Address;
using Lykke.SettingsReader;
using Microsoft.Extensions.CommandLineUtils;
using NBitcoin;
using NBitcoin.Altcoins;

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

            var walletBalanceRepo = new WalletBalanceRepository(
                AzureTableStorage<WalletBalanceEntity>.Create(settings.Nested(p => p.Db.DataConnString),
                    "WalletBalances", logFactory));

            var backupTableName = "ObservableWalletsBackup" + DateTime.UtcNow.UnixTimestamp();
            var observableWalletBackupRepository = new ObservableWalletRepository(AzureTableStorage<ObservableWalletEntity>.Create(
                settings.Nested(p => p.Db.DataConnString),
                backupTableName, logFactory));

            Console.WriteLine("Retrieving observable wallets");

            var observableWallets = (await observableWalletRepository.GetAll()).ToList();

            var obserwabletWalletsTransformation = observableWallets.ToDictionary(observableWallet=>observableWallet.Address, observableWallet =>
            {
                var newAddress = addressValidator.GetBitcoinAddress(observableWallet.Address);

                if (newAddress == null)
                {
                    throw new ArgumentException($"Unable to recognize address {observableWallet.Address}", nameof(observableWallet.Address));
                }

                return newAddress.ScriptPubKey.GetDestinationAddress(bcashNetwork).ToString();
            });

            var backupProgressCounter = 0;
            Console.WriteLine($"Starting {observableWallets.Count} observable wallets backup to {backupTableName} table");

            foreach (var observableWallet in observableWallets)
            {
                backupProgressCounter++;
                Console.WriteLine($"Backup {observableWallet.Address} -- {backupProgressCounter} of {observableWallets.Count}");

                await observableWalletBackupRepository.Insert(observableWallet);
            }

            var flushingProgress = 0;
            Console.WriteLine("Flushing balance table");

            foreach (var observableWallet in observableWallets)
            {
                flushingProgress++;
                Console.WriteLine($"Deleting wallet balance record -- {flushingProgress} of {observableWallets.Count}");

                await walletBalanceRepo.DeleteIfExist(observableWallet.Address);
            }


            var removingProgress = 0;
            Console.WriteLine("Clearing table ObservableWallets");

            foreach (var observableWallet in observableWallets)
            {
                removingProgress++;
                Console.WriteLine($"Deleting observable wallet record -- {removingProgress} of {observableWallets.Count}");

                await observableWalletBackupRepository.Delete(observableWallet.Address);
            }

            var refillingProgress = 0;
            Console.WriteLine("Refilling table ObservableWallets");

            foreach (var observableWallet in observableWallets)
            {
                refillingProgress++;
                var newAddress = obserwabletWalletsTransformation[observableWallet.Address];

                Console.WriteLine($"Inserting obserwablewallet record {observableWallet.Address} => {newAddress} " +
                                  $"-- {refillingProgress} of {observableWallets.Count}");

                await observableWalletBackupRepository.Insert(ObservableWallet.Create(newAddress));
            }


            Console.WriteLine("All done!");
        }
    }
}
