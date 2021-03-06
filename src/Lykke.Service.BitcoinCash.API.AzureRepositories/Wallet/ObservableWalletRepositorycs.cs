﻿using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Lykke.Service.BitcoinCash.API.Core.Domain.Health.Exceptions;
using Lykke.Service.BitcoinCash.API.Core.Wallet;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.BitcoinCash.API.AzureRepositories.Wallet
{
    public class ObservableWalletEntity : TableEntity, IObservableWallet
    {
        public string Address { get; set; }

        public static string GeneratePartitionKey(string address)
        {
            return address;
        }

        public static string GenerateRowKey()
        {
            return "_";
        }

        public static ObservableWalletEntity Create(IObservableWallet source)
        {
            return new ObservableWalletEntity
            {
                Address = source.Address,
                PartitionKey = GeneratePartitionKey(source.Address),
                RowKey = GenerateRowKey()
            };
        }
    }
    public class ObservableWalletRepository : IObservableWalletRepository
    {
        private readonly INoSQLTableStorage<ObservableWalletEntity> _storage;
        private const int EntityExistsHttpStatusCode = 409;
        private const int EntityNotExistsHttpStatusCode = 404;

        public ObservableWalletRepository(INoSQLTableStorage<ObservableWalletEntity> storage)
        {
            _storage = storage;
        }

        public async Task Insert(IObservableWallet wallet)
        {
            if (!await _storage.TryInsertAsync(ObservableWalletEntity.Create(wallet)))
                throw new BusinessException($"Wallet {wallet.Address} already exist", ErrorCode.EntityAlreadyExist);
        }

        public async Task<(IEnumerable<IObservableWallet> items, string ContinuationToken)> GetAll(int take, string continuationToken)
        {
            return await _storage.GetDataWithContinuationTokenAsync(take, continuationToken);
        }

        public async Task<IEnumerable<IObservableWallet>> GetAll()
        {
            return await _storage.GetDataAsync();
        }


        public async Task Delete(string address)
        {
            if (!await _storage.DeleteIfExistAsync(ObservableWalletEntity.GeneratePartitionKey(address), ObservableWalletEntity.GenerateRowKey()))
                throw new BusinessException($"Wallet {address} not exist", ErrorCode.EntityNotExist);
        }

        public async Task<IObservableWallet> Get(string address)
        {
            return await _storage.GetDataAsync(ObservableWalletEntity.GeneratePartitionKey(address), ObservableWalletEntity.GenerateRowKey());
        }
    }
}
