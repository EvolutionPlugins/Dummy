using EvolutionPlugins.Dummy.Providers;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy
{
    public class ServiceConfigurator : IServiceConfigurator
    {
        public void ConfigureServices(IOpenModStartupContext openModStartupContext, IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IDummyProvider, DummyProvider>();
        }
    }
}
