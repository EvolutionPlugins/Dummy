using EvolutionPlugins.Dummy.Models;
using OpenMod.API.Ioc;
using OpenMod.API.Users;
using Steamworks;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.API
{
    [Service]
    public interface IDummyProvider
    {
        IReadOnlyDictionary<CSteamID, DummyData> Dummies { get; }

        Task<bool> AddDummyAsync(CSteamID id, DummyData dummyData);

        Task<bool> RemoveDummyAsync(CSteamID id);

        Task ClearAllDummiesAsync();

        Task KickTimerTask(ulong id, uint timerSeconds);

        Task<IUser> FindDummyAsync(ulong id);

        Task<bool> GetDummyDataAsync(ulong id, out DummyData dummyData);

        Task<CSteamID> GetAvailableIdAsync();
    }
}
