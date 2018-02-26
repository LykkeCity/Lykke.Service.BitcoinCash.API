using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Service.BitcoinCash.API.Core.Services;
using Lykke.Service.BitcoinCash.API.Core.Settings.ServiceSettings;
using Lykke.Service.BitcoinCash.API.Services.Health;
using Lykke.Service.BitcoinCash.API.Services.LifeiteManagers;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.BitcoinCash.API.Modules
{
    public class BitcoinCashApiModule : Module
    {
        private readonly IReloadingManager<BitcoinCashApiSettings> _settings;
        private readonly ILog _log;
        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies
        private readonly IServiceCollection _services;

        public BitcoinCashApiModule(IReloadingManager<BitcoinCashApiSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            builder.Populate(_services);
        }
    }
}
