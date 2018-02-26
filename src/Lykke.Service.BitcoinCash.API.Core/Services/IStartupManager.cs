using System.Threading.Tasks;

namespace Lykke.Service.BitcoinCash.API.Core.Services
{
    public interface IStartupManager
    {
        Task StartAsync();
    }
}