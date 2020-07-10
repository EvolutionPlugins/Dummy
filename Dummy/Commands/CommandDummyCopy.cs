using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy.API;
using EvolutionPlugins.Dummy.Models;
using Microsoft.Extensions.Configuration;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Core.Helpers;
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
    [Command("copy")]
    [CommandDescription("Creates a dummy and copy your skin, hait, beard, etc...")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummyCopy : Command
    {
        private readonly IConfiguration m_Configuration;
        private readonly IDummyProvider m_DummyProvider;
        private readonly IUserDataSeeder m_UserDataSeeder;
        private readonly IUserDataStore m_UserDataStore;

        public CommandDummyCopy(IServiceProvider serviceProvider, IConfiguration configuration,
            IDummyProvider dummyProvider, IUserDataSeeder userDataSeeder, IUserDataStore userDataStore) : base(serviceProvider)
        {
            m_Configuration = configuration;
            m_DummyProvider = dummyProvider;
            m_UserDataSeeder = userDataSeeder;
            m_UserDataStore = userDataStore;
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

            var id = await m_DummyProvider.GetAvailableIdAsync();

            var steamPlayer = user.SteamPlayer;

            Provider.pending.Add(new SteamPending(new SteamPlayerID(id, 0, "dummy", "dummy", "dummy", CSteamID.Nil),
                true, steamPlayer.face, steamPlayer.hair, steamPlayer.beard, steamPlayer.skin, steamPlayer.color,
                UnityEngine.Color.white, steamPlayer.hand, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, Array.Empty<ulong>(),
                EPlayerSkillset.NONE, "english", CSteamID.Nil));

            await UniTask.SwitchToMainThread();

            Provider.accept(new SteamPlayerID(id, 0, "dummy", "dummy", "dummy", CSteamID.Nil), true, false,
                steamPlayer.face, steamPlayer.hair, steamPlayer.beard, steamPlayer.skin, steamPlayer.color,
                UnityEngine.Color.white, steamPlayer.hand, steamPlayer.shirtItem, steamPlayer.pantsItem, steamPlayer.hatItem,
                steamPlayer.backpackItem, steamPlayer.vestItem, steamPlayer.maskItem, steamPlayer.glassesItem,
                steamPlayer.skinItems, steamPlayer.skinTags, steamPlayer.skinDynamicProps, EPlayerSkillset.NONE,
                "english", CSteamID.Nil);

            var dummy = Provider.clients.Last();
            dummy.player.teleportToLocationUnsafe(user.Player.transform.position, user.Player.transform.rotation.eulerAngles.y);

            await UniTask.SwitchToTaskPool();

            var dummyUser = new UnturnedUser(m_UserDataStore, dummy.player);

            await m_UserDataSeeder.SeedUserDataAsync(dummyUser.Id, dummyUser.Type, dummyUser.DisplayName); // https://github.com/openmod/openmod/pull/109
            await m_DummyProvider.AddDummyAsync(id, new DummyData(new List<CSteamID> { user.SteamId }, dummyUser));

            var kickTimer = m_Configuration.GetSection("KickDummyAfterSeconds").Get<uint>();
            if(kickTimer != 0)
            {
                AsyncHelper.Schedule("Kick a dummy timer", () => m_DummyProvider.KickTimerTask(dummyUser.SteamId.m_SteamID, kickTimer));
            }

            await user.PrintMessageAsync($"Dummy ({id.m_SteamID}) has created");
        }
    }
}
