using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.BitcoinCash.API.AzureRepositories.Helpers
{
    public static class AzureTableStoragePaginationHelper
    {
        public static async Task<IEnumerable<T>> GetPagedResult<T>(this INoSQLTableStorage<T> storage, string partition, int skip, int take) where T:ITableEntity, new()
        {
            var all = await storage.GetDataAsync(partition);
            return all.Skip(skip).Take(take);
        } 
    }
}
