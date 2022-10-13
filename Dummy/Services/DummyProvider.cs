extern alias JetBrainsAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Models;
using Dummy.Users;
using JetBrainsAnnotations::JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using OpenMod.API.Users;
using OpenMod.Unturned.Users;
using Steamworks;

namespace Dummy.Services
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    [UsedImplicitly]
    public class DummyProvider : IDummyProvider
    {
        private readonly DummyUserProvider m_Provider;

        public IReadOnlyCollection<DummyUser> Dummies => m_Provider.DummyUsers;

        public DummyProvider(IUserManager userManager)
        {
            m_Provider = (DummyUserProvider)userManager.UserProviders.First(x => x is DummyUserProvider)!;
        }

        public Task<DummyUser> AddDummyAsync(CSteamID? id, HashSet<CSteamID>? owners)
        {
            return m_Provider.AddDummyAsync(id, owners);
        }

        public Task<DummyUser> AddCopiedDummyAsync(CSteamID? id, HashSet<CSteamID>? owners, UnturnedUser? userCopy)
        {
            return m_Provider.AddDummyAsync(id, owners, userCopy);
        }

        public Task<DummyUser> AddDummyByParameters(CSteamID? id, HashSet<CSteamID>? owners,
            ConfigurationSettings? settings)
        {
            return m_Provider.AddDummyAsync(id, owners, settings: settings);
        }

        public async Task<bool> RemoveDummyAsync(CSteamID id)
        {
            await UniTask.SwitchToMainThread();

            var playerDummy = await FindDummyUserAsync(id);
            if (playerDummy == null)
            {
                return false;
            }
            
            await playerDummy.DisposeAsync();
            m_Provider.DummyUsers.Remove(playerDummy);

            return true;
        }

        public async Task ClearDummiesAsync()
        {
            await UniTask.SwitchToMainThread();

            for (var i = m_Provider.DummyUsers.Count - 1; i >= 0; i--)
            {
                var user = m_Provider.DummyUsers.ElementAt(i);

                await user.DisposeAsync();
                m_Provider.DummyUsers.Remove(user);
            }
        }

        public Task<DummyUser?> FindDummyUserAsync(CSteamID id)
        {
            return Task.FromResult<DummyUser?>(Dummies.FirstOrDefault(x => x.SteamID == id));
        }

        public Task<CSteamID> GetAvailableIdAsync()
        {
            return Task.FromResult(m_Provider.GetAvailableId());
        }

        public Task<DummyUser?> FindDummyUserAsync(ulong id)
        {
            return FindDummyUserAsync((CSteamID)id);
        }
    }
}