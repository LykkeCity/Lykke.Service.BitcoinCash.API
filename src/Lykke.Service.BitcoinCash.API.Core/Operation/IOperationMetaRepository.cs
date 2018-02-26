using System;
using System.Threading.Tasks;

namespace Lykke.Service.BitcoinCash.API.Core.Operation
{
    public interface IOperationMeta
    {
        Guid OperationId { get; }

        string FromAddress { get; }

        string ToAddress { get; }

        string AssetId { get; }

        long AmountSatoshi { get; }
        long FeeSatoshi { get; }

        bool IncludeFee { get; }
        DateTime Inserted { get; }
    }

    public class OperationMeta : IOperationMeta
    {
        public Guid OperationId { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string AssetId { get; set; }
        public long AmountSatoshi { get; set; }
        public long FeeSatoshi { get; set; }
        public bool IncludeFee { get; set; }
        public DateTime Inserted { get; set; }

        public static OperationMeta Create(Guid operationId, string fromAddress, string toAddress, string assetId,
            long amountSatoshi, long feeSatoshi, bool includeFee, DateTime? inserted = null)
        {
            return new OperationMeta
            {
                AmountSatoshi = amountSatoshi,
                AssetId = assetId,
                FromAddress = fromAddress,
                IncludeFee = includeFee,
                OperationId = operationId,
                ToAddress = toAddress,
                Inserted = inserted ?? DateTime.UtcNow,
                FeeSatoshi = feeSatoshi
            };
        }
    }

    public interface IOperationMetaRepository
    {
        Task Insert(IOperationMeta meta);

        Task<IOperationMeta> Get(Guid id);

        Task<bool> Exist(Guid id);
    }
}
