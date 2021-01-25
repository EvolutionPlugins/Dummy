using Dummy.NetTransports;
using Dummy.Permissions;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using OpenMod.Core.Permissions;
using SDG.NetTransport;

namespace Dummy
{
    public class ServiceConfigurator : IServiceConfigurator
    {
        public void ConfigureServices(IOpenModServiceConfigurationContext openModStartupContext, IServiceCollection serviceCollection)
        {
            serviceCollection.Configure<PermissionCheckerOptions>(options => options.AddPermissionCheckProvider<DummyPermissionCheckProvider>());
            serviceCollection.AddTransient<ITransportConnection, DummyTransportConnection>();
        }
    }
}