using EvolutionPlugins.Dummy.API;
using EvolutionPlugins.Dummy.Providers;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using OpenMod.Core.Users;

namespace EvolutionPlugins.Dummy
{
    public class ServiceConfigurator : IServiceConfigurator
    {
        public void ConfigureServices(IOpenModServiceConfigurationContext openModStartupContext, IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IDummyProvider, DummyProvider>();

            serviceCollection.Configure<UserManagerOptions>(options => options.AddUserProvider<DummyProvider>());
        }
    }
}
