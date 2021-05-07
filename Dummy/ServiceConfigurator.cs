extern alias JetBrainsAnnotations;
using Dummy.NetTransports;
using Dummy.Permissions;
using Dummy.Services;
using JetBrainsAnnotations::JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using OpenMod.Core.Permissions;
using OpenMod.Core.Users;
using SDG.NetTransport;

namespace Dummy
{
    [UsedImplicitly]
    public class ServiceConfigurator : IServiceConfigurator
    {
        public void ConfigureServices(IOpenModServiceConfigurationContext openModStartupContext,
            IServiceCollection serviceCollection)
        {
            serviceCollection.Configure<PermissionCheckerOptions>(options =>
                options.AddPermissionCheckProvider<DummyPermissionCheckProvider>());
            serviceCollection.AddTransient<ITransportConnection, DummyTransportConnection>();
            serviceCollection.Configure<UserManagerOptions>(options => options.AddUserProvider<DummyUserProvider>());
        }
    }
}