// ReSharper disable CheckNamespace

using Cysharp.Threading.Tasks;
using Dummy.Players;
using Dummy.Providers;
using Dummy.Threads;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OpenMod.API.Users;
using OpenMod.Core.Helpers;
using OpenMod.Core.Users;
using OpenMod.Extensions.Games.Abstractions.Players;
using OpenMod.UnityEngine.Extensions;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Dummy.Users
{
    public class DummyUser : UserBase, IAsyncDisposable, IEquatable<DummyUser>, IPlayerUser<DummyPlayer>
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public DummyUserActionThread Actions { get; }
        public DummyUserSimulationThread Simulation { get; }
        public DummyPlayer Player { get; }
        public HashSet<CSteamID> Owners { get; }

        public CSteamID SteamID => Player.SteamId;
        public SteamPlayer SteamPlayer => Player.SteamPlayer;

        IPlayer IPlayerUser.Player => Player;

        protected internal DummyUser(DummyProvider dummyProvider, IUserDataStore userDataStore, SteamPlayer steamPlayer,
            ILoggerFactory loggerFactory, IStringLocalizer stringLocalizer, HashSet<CSteamID> owners = null)
            : base(null /* <- todo */, userDataStore)
        {
            Id = steamPlayer.playerID.steamID.ToString();
            DisplayName = steamPlayer.playerID.characterName;
            Type = KnownActorTypes.Player;
            Session = new DummyUserSession(this);

            Player = new DummyPlayer(steamPlayer);
            m_StringLocalizer = stringLocalizer;
            Owners = owners ?? new HashSet<CSteamID>();
            Actions = new DummyUserActionThread(this, loggerFactory.CreateLogger($"Dummy.{Id}.Action"));
            Simulation = new DummyUserSimulationThread(this, loggerFactory.CreateLogger($"Dummy.{Id}.Simulation"));

            Actions.Enabled = true;
            Simulation.Enabled = true;

            AsyncHelper.Schedule($"Action a dummy {Id}", () => Actions.Start().AsTask());
            AsyncHelper.Schedule($"Simulation a dummy {Id}", () => Simulation.Start().AsTask());
        }

        public override Task PrintMessageAsync(string message)
        {
            return PrintMessageAsync(message, Color.White, true, null);
        }

        public override Task PrintMessageAsync(string message, Color color)
        {
            return PrintMessageAsync(message, color, true, null);
        }

        private Task PrintMessageAsync(string message, Color color, bool isRich, string iconUrl)
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
                    ChatManager.serverSendMessage(m_StringLocalizer["events:chatted", new[] { Id, message }], color.ToUnityColor(),
                        toPlayer: player.channel.owner, iconURL: iconUrl, useRichTextFormatting: isRich);
                }
            }

            return PrintMessageTask().AsTask();
        }

        public ValueTask DisposeAsync()
        {
            Actions.Enabled = false;
            Simulation.Enabled = false;
            if (Session is DummyUserSession dummySession)
            {
                dummySession.OnSessionEnd();
            }
            return new ValueTask(Session.DisconnectAsync());
        }

        public bool Equals(DummyUser other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return other.SteamID.Equals(SteamID);
        }
    }
}