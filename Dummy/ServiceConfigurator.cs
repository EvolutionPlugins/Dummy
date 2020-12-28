using EvolutionPlugins.Dummy.API;
using EvolutionPlugins.Dummy.Providers;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;

namespace EvolutionPlugins.Dummy
{
    public class ServiceConfigurator : IServiceConfigurator
    {
        public void ConfigureServices(IOpenModServiceConfigurationContext openModStartupContext, IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IDummyProvider, DummyProvider>();
        }
    }
}
