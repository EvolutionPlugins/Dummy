using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Providers
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    public class DummyProvider : IDummyProvider
    {
        private readonly Dictionary<CSteamID, DummyData> m_Dummies;

        public IReadOnlyDictionary<CSteamID, DummyData> Dummies => m_Dummies;

        public async Task<bool> AddDummy(CSteamID Id, DummyData dummyData)
        {
            if (m_Dummies.ContainsKey(Id))
            {
                return false;
            }
            m_Dummies.Add(Id, dummyData);
            return true;
        }

        public async Task<bool> RemoveDummy(CSteamID Id)
        {
            return m_Dummies.Remove(Id);
        }
    }
}
