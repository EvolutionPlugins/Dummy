using EvolutionPlugins.Dummy.Models;
using EvolutionPlugins.Dummy.Providers;
using OpenMod.API.Ioc;
using OpenMod.Unturned.Users;
using Steamworks;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.API
{
    [Service]
    public interface IDummyProvider
    {
        IReadOnlyDictionary<CSteamID, PlayerDummy> Dummies { get; }

        /// <summary>
        /// Spawn a dummy
        /// </summary>
        /// <param name="id">Use GetAvailableIdAsync to get available ID</param>
        /// <param name="owners">Owners get all notification about a dummy</param>
        /// <exception cref="DummyContainsException">Dummy with id already created. Use GetAvailableIdAsync to get available ID</exception>
        /// <exception cref="DummyOverflowsException">Dummies limit reached</exception>
        Task<PlayerDummy> AddDummyAsync(CSteamID id, HashSet<CSteamID> owners);
        /// <summary>
        /// Spawn a dummy and copy all of the user
        /// </summary>
        /// <param name="id">Use GetAvailableIdAsync to get available ID</param>
        /// <param name="owners">Owners get all notification about a dummy</param>
        /// <exception cref="DummyContainsException">Dummy with id already created. Use GetAvailableIdAsync to get available ID</exception>
        /// <exception cref="DummyOverflowsException">Dummies limit reached</exception>
        Task<PlayerDummy> AddCopiedDummyAsync(CSteamID id, HashSet<CSteamID> owners, UnturnedUser userCopy);

        Task<bool> RemoveDummyAsync(CSteamID id);

        Task ClearDummies();

        Task<PlayerDummy> GetPlayerDummy(ulong id);

        Task<bool> GetDummyDataAsync(ulong id, out PlayerDummyData playerDummyData);

        Task<CSteamID> GetAvailableIdAsync();
    }
}
