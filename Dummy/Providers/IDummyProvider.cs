using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Providers
{
    public interface IDummyProvider
    {
        IReadOnlyDictionary<CSteamID, DummyData> Dummies { get; }

        Task<bool> AddDummy(CSteamID Id, DummyData dummyData);

        Task<bool> RemoveDummy(CSteamID Id);
    }
}
