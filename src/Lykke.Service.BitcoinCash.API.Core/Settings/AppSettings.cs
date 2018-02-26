using Lykke.Service.BitcoinCash.API.Core.Settings.ServiceSettings;
using Lykke.Service.BitcoinCash.API.Core.Settings.SlackNotifications;

namespace Lykke.Service.BitcoinCash.API.Core.Settings
{
    public class AppSettings
    {
        public BitcoinCashApiSettings BitcoinCashApi { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
