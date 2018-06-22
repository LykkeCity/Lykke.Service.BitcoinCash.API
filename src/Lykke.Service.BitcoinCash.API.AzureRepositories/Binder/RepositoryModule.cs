﻿using Autofac;
using AzureStorage.Blob;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.BitcoinCash.API.AzureRepositories.Asset;
using Lykke.Service.BitcoinCash.API.AzureRepositories.Operations;
using Lykke.Service.BitcoinCash.API.AzureRepositories.SpentOutputs;
using Lykke.Service.BitcoinCash.API.AzureRepositories.Transactions;
using Lykke.Service.BitcoinCash.API.AzureRepositories.Wallet;
using Lykke.Service.BitcoinCash.API.Core.Asset;
using Lykke.Service.BitcoinCash.API.Core.ObservableOperation;
using Lykke.Service.BitcoinCash.API.Core.Operation;
using Lykke.Service.BitcoinCash.API.Core.Settings.ServiceSettings;
using Lykke.Service.BitcoinCash.API.Core.TransactionOutputs;
using Lykke.Service.BitcoinCash.API.Core.Transactions;
using Lykke.Service.BitcoinCash.API.Core.Wallet;
using Lykke.SettingsReader;

namespace Lykke.Service.BitcoinCash.API.AzureRepositories.Binder
{
    public class RepositoryModule : Module
    {
        private readonly ILog _log;
        private readonly IReloadingManager<BitcoinCashApiSettings> _settings;
        public RepositoryModule(IReloadingManager<BitcoinCashApiSettings> settings, ILog log)
        {
            _log = log;
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            RegisterRepo(builder);
            RegisterBlob(builder);
        }

        private void RegisterRepo(ContainerBuilder builder)
        {
            builder.RegisterInstance(new AssetRepository())
                .As<IAssetRepository>()
                .SingleInstance();

            builder.RegisterInstance(new OperationMetaRepository(
                AzureTableStorage<OperationMetaEntity>.Create(_settings.Nested(p => p.Db.DataConnString),
                    "OperationMeta", _log)))
                .As<IOperationMetaRepository>()
                .SingleInstance();

            builder.RegisterInstance(new OperationEventRepository(
                    AzureTableStorage<OperationEventTableEntity>.Create(_settings.Nested(p => p.Db.DataConnString),
                        "OperationEvents", _log)))
                .As<IOperationEventRepository>()
                .SingleInstance();


            builder.RegisterInstance(new UnconfirmedTransactionRepository(
                AzureTableStorage<UnconfirmedTransactionEntity>.Create(_settings.Nested(p => p.Db.DataConnString),
                    "UnconfirmedTransactions", _log)))
                .As<IUnconfirmedTransactionRepository>()
                .SingleInstance();

            builder.RegisterInstance(new ObservableOperationRepository(
                AzureTableStorage<ObservableOperationEntity>.Create(_settings.Nested(p => p.Db.DataConnString),
                    "ObservableOperations", _log)))
                .As<IObservableOperationRepository>()
                .SingleInstance();

            builder.RegisterInstance(new ObservableWalletRepository(
                AzureTableStorage<ObservableWalletEntity>.Create(_settings.Nested(p => p.Db.DataConnString),
                    "ObservableWallets", _log)))
                .As<IObservableWalletRepository>()
                .SingleInstance();

            builder.RegisterInstance(new WalletBalanceRepository(
                    AzureTableStorage<WalletBalanceEntity>.Create(_settings.Nested(p => p.Db.DataConnString),
                        "WalletBalances", _log)))
                .As<IWalletBalanceRepository>()
                .SingleInstance();

            builder.RegisterInstance(new SpentOutputRepository(
                    AzureTableStorage<SpentOutputEntity>.Create(_settings.Nested(p => p.Db.DataConnString),
                        "SpentOutputs", _log)))
                .As<ISpentOutputRepository>()
                .SingleInstance();
        }

        private void RegisterBlob(ContainerBuilder builder)
        {
            builder.RegisterInstance(
                new TransactionBlobStorage(AzureBlobStorage.Create(_settings.Nested(p => p.Db.DataConnString))))
                .As<ITransactionBlobStorage>();
        }
    }
}
