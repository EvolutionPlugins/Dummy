using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dummy.API.Exceptions;
using Dummy.Models;
using Dummy.Users;
using OpenMod.API.Ioc;
using OpenMod.Unturned.Users;
using Steamworks;

namespace Dummy.API
{
    [Service]
    public interface IDummyProvider
    {
        /// <summary>
        /// The list of dummies
        /// </summary>
        IReadOnlyCollection<DummyUser> Dummies { get; }

        /// <summary>
        /// Spawn a dummy with default settings in <b>config.yaml</b> file
        /// </summary>
        /// <param name="id">The <see cref="CSteamID"/> to spawn a dummy. Set to null to make sure the id will be unique</param>
        /// <param name="owners">Owners get all logs about a dummy</param>
        /// <exception cref="DummyContainsException">Dummy with id already created</exception>
        /// <exception cref="DummyOverflowsException">Dummies limit reached</exception>
        /// <exception cref="DummyCanceledSpawnException">The game or plugin cancelled connection of dummy</exception>
        Task<DummyUser> AddDummyAsync(CSteamID? id, HashSet<CSteamID>? owners);

        /// <summary>
        /// Spawn a dummy and copy all parameters of the user
        /// </summary>
        /// <param name="id">The <see cref="CSteamID"/> to spawn a dummy. Set to <see langword="null"/> to make sure the id will be unique</param>
        /// <param name="owners">The list of owners that get all notification about a dummy</param>
        /// <param name="userCopy">The user to copy partially all parameters to dummy</param>
        /// <exception cref="DummyContainsException">Dummy with <paramref name="id"/> already created</exception>
        /// <exception cref="DummyOverflowsException">Dummies limit reached</exception>
        /// <exception cref="DummyCanceledSpawnException">The game or plugin cancelled connection of dummy</exception>
        Task<DummyUser> AddCopiedDummyAsync(CSteamID? id, HashSet<CSteamID>? owners, UnturnedUser? userCopy);

        /// <summary>
        /// Spawn a dummy and copy all parameters of the user
        /// </summary>
        /// <param name="id">The steamId to spawn a dummy. Set to <see langword="null"/> to make sure the id will be unique</param>
        /// <param name="owners">OThe list of owners that get all notification about a dummy</param>
        /// <param name="settings">Parameters to spawn dummy. If it's <see langword="null"/> then it will use default settings in <b>config.yaml</b> file</param>
        /// <exception cref="DummyContainsException">Dummy with <paramref name="id"/> already created</exception>
        /// <exception cref="DummyOverflowsException">Dummies limit reached</exception>
        /// <exception cref="DummyCanceledSpawnException">The game or plugin cancelled connection of dummy</exception>
        Task<DummyUser> AddDummyByParameters(CSteamID? id, HashSet<CSteamID>? owners, ConfigurationSettings? settings);

        /// <summary>
        /// Spawn a dummy with <paramref name="settings"/> or try to copy from <paramref name="userCopy"/>.
        /// </summary>
        /// <remarks><b>NOTE:</b> <paramref name="settings"/> has higher priority than <paramref name="userCopy"/>, so you can change the name of dummy and copy all other parameters from the user</remarks>
        /// <param name="id">The <see cref="CSteamID"/> to spawn a dummy. Set to null to make sure the id will be unique</param>
        /// <param name="owners">The list of owners that get all notification about a dummy</param>
        /// <param name="userCopy">The user to copy partially all parameters to dummy</param>
        /// <param name="settings">Parameters to spawn dummy. If it's <see langword="null"/> then it will use default settings in <b>config.yaml</b> file</param>
        /// <exception cref="DummyContainsException">Dummy with <paramref name="id"/> already created</exception>
        /// <exception cref="DummyOverflowsException">Dummies limit reached</exception>
        /// <exception cref="DummyCanceledSpawnException">The game or plugin cancelled connection of dummy</exception>
        Task<DummyUser> AddDummyAsync(CSteamID? id, HashSet<CSteamID>? owners = null, UnturnedUser? userCopy = null, ConfigurationSettings? settings = null);

        /// <summary>
        /// Remove a dummy by <see cref="CSteamID"/>
        /// </summary>
        /// <param name="id">The steamId of dummy to remove</param>
        /// <returns>Returns <see langword="true"/> if dummy removed; otherwise <see langword="false"/></returns>
        Task<bool> RemoveDummyAsync(CSteamID id);

        /// <summary>
        /// Removes all dummies
        /// </summary>
        Task ClearDummiesAsync();

        [Obsolete("Use instead a method that accept CSteamID")]
        Task<DummyUser?> FindDummyUserAsync(ulong id);

        /// <summary>
        /// Searches a dummy by <see cref="CSteamID"/>
        /// </summary>
        /// <param name="id">The steamId of dummy to find it</param>
        Task<DummyUser?> FindDummyUserAsync(CSteamID id);
    }
}