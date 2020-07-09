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
#pragma warning disable CA1063 // Implement IDisposable Correctly
    public class DummyProvider : IDummyProvider, IDisposable
#pragma warning restore CA1063 // Implement IDisposable Correctly
    {
        private bool m_IsDisposing;

        private readonly Dictionary<CSteamID, DummyData> m_Dummies;

        public DummyProvider()
        {
            m_Dummies = new Dictionary<CSteamID, DummyData>();
            m_IsDisposing = false;
        }

        public IReadOnlyDictionary<CSteamID, DummyData> Dummies => m_Dummies;

        public Task<bool> AddDummyAsync(CSteamID Id, DummyData dummyData)
        {
            if (m_Dummies.ContainsKey(Id))
            {
                return Task.FromResult(false);
            }
            m_Dummies.Add(Id, dummyData);
            return Task.FromResult(false);
        }

#pragma warning disable CA1063 // Implement IDisposable Correctly
        public void Dispose()
#pragma warning restore CA1063 // Implement IDisposable Correctly
        {
            if(m_IsDisposing)
            {
                return;
            }
            m_IsDisposing = true;
            // maybe also kick?
            m_Dummies.Clear();
        }

        public CSteamID GetAvailableId()
        {
            var result = new CSteamID(1);

            while (Dummies.ContainsKey(result))
            {
                result.m_SteamID++;
            }
            return result;
        }

        public Task<bool> RemoveDummyAsync(CSteamID Id)
        {
            return Task.FromResult(m_Dummies.Remove(Id));
        }
    }
}
