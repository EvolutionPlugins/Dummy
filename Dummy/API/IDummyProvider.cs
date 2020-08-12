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
        IReadOnlyDictionary<CSteamID, PlayerDummy> Dummies { get; }

        Task<bool> AddDummyAsync(CSteamID id, PlayerDummyData playerDummyData);

        Task<bool> RemoveDummyAsync(CSteamID id);

        Task ClearAllDummiesAsync();

        Task KickTimerTask(ulong id, uint timerSeconds);

        Task<PlayerDummy> FindDummyAsync(ulong id);
        
        

        Task<bool> GetDummyDataAsync(ulong id, out PlayerDummyData playerDummyData);

        Task<CSteamID> GetAvailableIdAsync();
    }
}
