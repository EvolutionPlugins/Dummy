using System.Collections.Generic;
using System.Threading.Tasks;
using Dummy.Models;
using Dummy.Services;
using Dummy.Users;
using OpenMod.API.Ioc;
using OpenMod.Unturned.Users;
using Steamworks;

namespace Dummy.API
{
    [Service]
    public interface IDummyProvider
    {
        IReadOnlyCollection<DummyUser> Dummies { get; }

        /// <summary>
        /// Spawn a dummy
        /// </summary>
        /// <param name="id">Use GetAvailableIdAsync to get available ID</param>
        /// <param name="owners">Owners get all notification about a dummy</param>
        /// <exception cref="DummyContainsException">Dummy with id already created. Use GetAvailableIdAsync to get available ID</exception>
        /// <exception cref="DummyOverflowsException">Dummies limit reached</exception>
        Task<DummyUser?> AddDummyAsync(CSteamID id, HashSet<CSteamID> owners);

        /// <summary>
        /// Spawn a dummy and copy all of the user
        /// </summary>
        /// <param name="id">Use GetAvailableIdAsync to get available ID</param>
        /// <param name="owners">Owners get all notification about a dummy</param>
        /// <param name="userCopy">Copy parameters from user</param>
        /// <exception cref="DummyContainsException">Dummy with id already created. Use GetAvailableIdAsync to get available ID</exception>
        /// <exception cref="DummyOverflowsException">Dummies limit reached</exception>
        Task<DummyUser?> AddCopiedDummyAsync(CSteamID id, HashSet<CSteamID> owners, UnturnedUser userCopy);

        Task<DummyUser?> AddDummyByParameters(CSteamID id, HashSet<CSteamID> owners, ConfigurationSettings settings);

        Task<bool> RemoveDummyAsync(CSteamID id);

        Task ClearDummiesAsync();

        Task<DummyUser?> GetPlayerDummyAsync(ulong id);

        Task<CSteamID> GetAvailableIdAsync();
    }
}