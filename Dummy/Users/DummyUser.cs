using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Dummy.Players;
using Dummy.Threads;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OpenMod.API.Users;
using OpenMod.Core.Users;
using OpenMod.Extensions.Games.Abstractions.Players;
using OpenMod.UnityEngine.Extensions;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Color = System.Drawing.Color;

namespace Dummy.Users
{
    public class DummyUser : UnturnedUser, IPlayerUser<DummyPlayer>, IAsyncDisposable, IEquatable<DummyUser>
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public DummyUserActionThread Actions { get; }
        public DummyUserSimulationThread Simulation { get; }
        public new DummyPlayer Player { get; }
        public HashSet<CSteamID> Owners { get; }

        public CSteamID SteamID => Player.SteamId;
        public SteamPlayer SteamPlayer => Player.SteamPlayer;

        IPlayer IPlayerUser.Player => Player;

        public DummyUser(UnturnedUserProvider userProvider, IUserDataStore userDataStore, SteamPlayer steamPlayer,
            ILoggerFactory loggerFactory, IStringLocalizer stringLocalizer, bool disableSimulation,
            HashSet<CSteamID>? owners = null)
            : base(userProvider, userDataStore, steamPlayer.player)
        {
            Player = new(steamPlayer);
            m_StringLocalizer = stringLocalizer;
            Owners = owners ?? new HashSet<CSteamID>();
            Actions = new(this, loggerFactory.CreateLogger($"Dummy.{Id}.Action"));
            Simulation = new(this, loggerFactory.CreateLogger($"Dummy.{Id}.Simulation"));

            Actions.Enabled = true;
            Simulation.Enabled = !disableSimulation;

            Actions.Start().Forget();
            Simulation.Start().Forget();
        }

        public override Task PrintMessageAsync(string message)
        {
            return PrintMessageAsync(message, Color.White, true, null);
        }

        public override Task PrintMessageAsync(string message, Color color)
        {
            return PrintMessageAsync(message, color, true, null);
        }

        private new Task PrintMessageAsync(string message, Color color, bool isRich, string? iconUrl)
        {
            async UniTask PrintMessageTask()
            {
                await UniTask.SwitchToMainThread();

                foreach (var owner in Owners)
                {
                    var player = PlayerTool.getPlayer(owner);
                    if (player == null)
                    {
                        continue;
                    }

                    ChatManager.serverSendMessage(m_StringLocalizer["events:chatted", new { Id, Text = message }],
                        color.ToUnityColor(),
                        toPlayer: player.channel.owner, iconURL: iconUrl, useRichTextFormatting: isRich);
                }
            }

            return PrintMessageTask().AsTask();
        }

        public bool Equals(DummyUser other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.SteamID.Equals(SteamID);
        }

        public override bool Equals(object obj)
        {
            return Equals((obj as DummyUser)!);
        }

        public override int GetHashCode()
        {
            return Player.GetHashCode();
        }

        public async ValueTask DisposeAsync()
        {
            await Simulation.DisposeAsync();
            await Actions.DisposeAsync();

            if (Session == null)
            {
                return;
            }

            if (Session is UnturnedUserSession session)
            {
                session.OnSessionEnd();
            }

            await Session.DisconnectAsync();
        }
    }
}