using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace Dummy.Users
{
    public sealed class DummyUser : UnturnedUser, IPlayerUser<DummyPlayer>, IAsyncDisposable, IEquatable<DummyUser>
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public DummyUserActionThread Actions { get; }
        public DummyUserSimulationThread Simulation { get; }
        public DummyPlayer Player { get; }
        public HashSet<CSteamID> Owners { get; }
        public Player? CopyUserVoice { get; set; }
        public HashSet<CSteamID> SubscribersUI { get; }

        public CSteamID SteamID => Player.SteamId;
        public SteamPlayer SteamPlayer => Player.SteamPlayer;

        IPlayer IPlayerUser.Player => Player;

        internal DummyUser(UnturnedUserProvider userProvider, IUserDataStore userDataStore, SteamPlayer steamPlayer,
            ILoggerFactory loggerFactory, IStringLocalizer stringLocalizer, bool disableSimulation,
            HashSet<CSteamID>? owners = null)
            : base(userProvider, userDataStore, steamPlayer.player, null)
        {
            SubscribersUI = new();
            Session = new DummyUserSession(this);

            Player = new DummyPlayer(steamPlayer);
            m_StringLocalizer = stringLocalizer;
            Owners = owners ?? new HashSet<CSteamID>();
            Actions = new DummyUserActionThread(this, loggerFactory.CreateLogger($"Dummy.{Id}.Action"));
            Simulation = new DummyUserSimulationThread(this, loggerFactory.CreateLogger($"Dummy.{Id}.Simulation"));

            Actions.Enabled = true;
            Simulation.Enabled = !disableSimulation;

            UniTask.Run(Actions.Start);
            UniTask.Run(Simulation.Start);
        }

        public override Task PrintMessageAsync(string message)
        {
            return PrintMessageAsync(message, Color.White, true, null);
        }

        public override Task PrintMessageAsync(string message, Color color)
        {
            return PrintMessageAsync(message, color, true, null);
        }

        private Task PrintMessageAsync(string message, Color color, bool isRich, string? iconUrl)
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

        public ValueTask DisposeAsync()
        {
            Actions.Enabled = false;
            Simulation.Enabled = false;
            if (Session is DummyUserSession dummySession)
            {
                dummySession.OnSessionEnd();
            }

            return new(Session!.DisconnectAsync());
        }

        public bool Equals(DummyUser other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.SteamId.Equals(SteamId);
        }

        public override bool Equals(object obj)
        {
            return Equals((obj as DummyUser)!);
        }

        public override int GetHashCode()
        {
            return Player.GetHashCode();
        }
    }
}