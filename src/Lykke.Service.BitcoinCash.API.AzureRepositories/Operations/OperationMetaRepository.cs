using System;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.BitcoinCash.API.Core.Operation;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.BitcoinCash.API.AzureRepositories.Operations
{
    public class OperationMetaEntity : TableEntity, IOperationMeta
    {
        public Guid OperationId { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string AssetId { get; set; }
        public long AmountSatoshi { get; set; }
        public long FeeSatoshi { get; set; }
        public bool IncludeFee { get; set; }
        public DateTime Inserted { get; set; }

        public static OperationMetaEntity Map(string partitionKey, string rowKey, IOperationMeta source)
        {
            return new OperationMetaEntity
            {
                PartitionKey = partitionKey,
                RowKey = rowKey,
                ToAddress = source.ToAddress,
                FromAddress = source.FromAddress,
                AssetId = source.AssetId,
                OperationId = source.OperationId,
                IncludeFee = source.IncludeFee,
                AmountSatoshi = source.AmountSatoshi,
                Inserted = source.Inserted,
                FeeSatoshi = source.FeeSatoshi
            };
        }

        public static class ByOperationId
        {
            public static string GeneratePartitionKey()
            {
                return "ByOperationId";
            }

            public static string GenerateRowKey(Guid operationId)
            {
                return operationId.ToString();
            }

            public static OperationMetaEntity Create(IOperationMeta source)
            {
                return Map(GeneratePartitionKey(), GenerateRowKey(source.OperationId), source);
            }
        }
    }

    public class OperationMetaRepository:IOperationMetaRepository
    {
        private readonly INoSQLTableStorage<OperationMetaEntity> _storage;

        public OperationMetaRepository(INoSQLTableStorage<OperationMetaEntity> storage)
        {
            _storage = storage;
        }

        public Task Insert(IOperationMeta meta)
        {
            return _storage.InsertAsync(OperationMetaEntity.ByOperationId.Create(meta));
        }

        public async Task<IOperationMeta> Get(Guid id)
        {
            return await _storage.GetDataAsync(OperationMetaEntity.ByOperationId.GeneratePartitionKey(),
                OperationMetaEntity.ByOperationId.GenerateRowKey(id));
        }

        public async Task<bool> Exist(Guid id)
        {
            return await Get(id) != null;
        }
    }
}
