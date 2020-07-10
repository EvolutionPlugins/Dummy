using EvolutionPlugins.Dummy.Models;
using OpenMod.API.Ioc;
using Steamworks;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.API
{
    [Service]
    public interface IDummyProvider
    {
        IReadOnlyDictionary<CSteamID, DummyData> Dummies { get; }

        Task<bool> AddDummyAsync(CSteamID Id, DummyData dummyData);

        Task<bool> RemoveDummyAsync(CSteamID Id);

        Task ClearAllDummies();

        CSteamID GetAvailableId();
    }
}
