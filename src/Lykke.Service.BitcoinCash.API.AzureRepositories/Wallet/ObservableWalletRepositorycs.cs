using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.BitcoinCash.API.Core.Domain.Health.Exceptions;
using Lykke.Service.BitcoinCash.API.Core.Wallet;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.BitcoinCash.API.AzureRepositories.Wallet
{
    public class ObservableWalletEntity : TableEntity, IObservableWallet
    {
        public string Address { get; set; }

        public static string GeneratePartitionKey()
        {
            return "ByAddress";
        }

        public static string GenerateRowKey(string address)
        {
            return address;
        }

        public static ObservableWalletEntity Create(IObservableWallet source)
        {
            return new ObservableWalletEntity
            {
                Address = source.Address,
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(source.Address)
            };
        }
    }
    public class ObservableWalletRepository: IObservableWalletRepository
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
            try
            {
                await _storage.InsertAsync(ObservableWalletEntity.Create(wallet));
            }
            catch (StorageException e) when(e.RequestInformation.HttpStatusCode == EntityExistsHttpStatusCode)
            {
                throw new BusinessException($"Wallet {wallet.Address} already exist", ErrorCode.EntityAlreadyExist);
            }
        }

        public async Task<IEnumerable<IObservableWallet>> GetAll()
        {
            return await _storage.GetDataAsync(ObservableWalletEntity.GeneratePartitionKey());
        }

        public async Task Delete(string address)
        {
            try
            {
                await _storage.DeleteAsync(ObservableWalletEntity.GeneratePartitionKey(),
                    ObservableWalletEntity.GenerateRowKey(address));
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == EntityNotExistsHttpStatusCode)
            {
                throw new BusinessException($"Wallet {address} not exist", ErrorCode.EntityNotExist);
            }
        }

        public async Task<IObservableWallet> Get(string address)
        {
            return await _storage.GetDataAsync(ObservableWalletEntity.GeneratePartitionKey(), ObservableWalletEntity.GenerateRowKey(address));
        }
    }
}
