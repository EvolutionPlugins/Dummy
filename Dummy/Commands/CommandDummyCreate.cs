using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy.API;
using Microsoft.Extensions.Configuration;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Color = System.Drawing.Color;
using Command = OpenMod.Core.Commands.Command;

namespace EvolutionPlugins.Dummy.Commands
{
    [Command("create")]
    [CommandDescription("Creates a dummy")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummyCreate : Command
    {
        private readonly IConfiguration m_Configuration;
        private readonly IDummyProvider m_DummyProvider;
        private readonly IUserDataSeeder m_UserDataSeeder;
        private readonly IUserDataStore m_UserDataStore;

        public CommandDummyCreate(IServiceProvider serviceProvider, IConfiguration configuration,
            IDummyProvider dummyProvider, IUserDataSeeder userDataSeeder, IUserDataStore userDataStore) : base(serviceProvider)
        {
            m_Configuration = configuration;
            m_DummyProvider = dummyProvider;
            m_UserDataStore = userDataStore;
            m_UserDataSeeder = userDataSeeder;
        }

        protected override async Task OnExecuteAsync()
        {
            var user = (UnturnedUser)Context.Actor;

            var amountDummiesConfig = m_Configuration.GetSection("AmountDummiesInSameTime").Get<byte>();
            if (amountDummiesConfig != 0 && m_DummyProvider.Dummies.Count + 1 > amountDummiesConfig)
            {
                await user.PrintMessageAsync("Dummy can't be created. Amount dummies overflow", Color.Yellow);
                return;
            }

            var id = m_DummyProvider.GetAvailableId();

            await m_DummyProvider.AddDummyAsync(id, new DummyData() { Owners = new List<CSteamID> { user.SteamId } });

            await UniTask.SwitchToMainThread();

            Provider.pending.Add(new SteamPending(
                    new SteamPlayerID(id, 0, "dummy", "dummy", "dummy", CSteamID.Nil), true, 0, 0, 0,
                    UnityEngine.Color.white, UnityEngine.Color.white, UnityEngine.Color.white, false, 0UL, 0UL, 0UL, 0UL,
                    0UL, 0UL, 0UL, Array.Empty<ulong>(), EPlayerSkillset.NONE, "english", CSteamID.Nil));

            Provider.accept(new SteamPlayerID(id, 1, "dummy", "dummy", "dummy", CSteamID.Nil), true, false, 0, 0, 0,
                UnityEngine.Color.white, UnityEngine.Color.white, UnityEngine.Color.white, false, 0, 0, 0, 0, 0, 0,
                0, Array.Empty<int>(), Array.Empty<string>(), Array.Empty<string>(), EPlayerSkillset.NONE, "english",
                CSteamID.Nil);

            var dummy = Provider.clients.Last();
            dummy.player.teleportToLocationUnsafe(user.Player.transform.position, user.Player.transform.rotation.eulerAngles.y);

            await UniTask.SwitchToTaskPool();

            var dummyUser = new UnturnedUser(m_UserDataStore, dummy.player);
            await m_UserDataSeeder.SeedUserDataAsync(dummyUser.Id, dummyUser.Type, dummyUser.DisplayName); // https://github.com/openmod/openmod/pull/109

            await user.PrintMessageAsync($"Dummy ({id.m_SteamID}) has created");
        }
    }
}
