using Dummy.API;
using Dummy.Permissions;
using Dummy.Providers;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using OpenMod.Core.Permissions;
using OpenMod.Core.Users;

namespace Dummy
{
    public class ServiceConfigurator : IServiceConfigurator
    {
        public void ConfigureServices(IOpenModServiceConfigurationContext openModStartupContext, IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IDummyProvider, DummyProvider>();

            serviceCollection.Configure<UserManagerOptions>(options => options.AddUserProvider<DummyProvider>());
            serviceCollection.Configure<PermissionCheckerOptions>(options =>
            {
                options.AddPermissionCheckProvider<DummyPermissionCheckProvider>();
            });
        }
    }
}