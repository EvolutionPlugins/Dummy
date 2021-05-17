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
            //Session = new DummyUserSession(this);

            Player = new DummyPlayer(steamPlayer);
            m_StringLocalizer = stringLocalizer;
            Owners = owners ?? new HashSet<CSteamID>();
            Actions = new DummyUserActionThread(this, loggerFactory.CreateLogger($"Dummy.{Id}.Action"));
            Simulation = new DummyUserSimulationThread(this, loggerFactory.CreateLogger($"Dummy.{Id}.Simulation"));

            Actions.Enabled = true;
            Simulation.Enabled = !disableSimulation;
#if DEBUG
            //Simulation.Move = new Vector3(1f, 0f);
#endif
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
    }
}