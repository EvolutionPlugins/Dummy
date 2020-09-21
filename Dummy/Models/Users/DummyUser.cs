// ReSharper disable CheckNamespace

using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy.Models.Players;
using EvolutionPlugins.Dummy.Providers;
using EvolutionPlugins.Dummy.Threads;
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

namespace EvolutionPlugins.Dummy.Models.Users
{
    public class DummyUser : UserBase, IAsyncDisposable, IEquatable<DummyUser>, IPlayerUser<DummyPlayer>
    {
        public DummyUserActionThread Actions { get; }
        public DummyUserSimulationThread Simulation { get; }
        public DummyPlayer Player { get; }
        public HashSet<CSteamID> Owners { get; }
        public int InternalIndex { get; }

        public CSteamID SteamID => Player.SteamID;
        public SteamPlayer SteamPlayer => Player.SteamPlayer;

        IPlayer IPlayerUser.Player => Player;

        protected internal DummyUser(DummyProvider dummyProvider, IUserDataStore userDataStore, SteamPlayer steamPlayer,
            int internalIndex, HashSet<CSteamID> owners = null)
            : base(dummyProvider, userDataStore)
        {
            Id = steamPlayer.playerID.steamID.ToString();
            DisplayName = steamPlayer.playerID.characterName;
            Type = KnownActorTypes.Player;
            InternalIndex = internalIndex;
            Session = new DummyUserSession(this);

            Player = new DummyPlayer(steamPlayer);
            Owners = owners ?? new HashSet<CSteamID>();
            Actions = new DummyUserActionThread(this);
            Simulation = new DummyUserSimulationThread(this);

            Actions.Enabled = true;
            Simulation.Enabled = true;

            AsyncHelper.Schedule($"Action a dummy {Id}", () => Actions.Start());
            AsyncHelper.Schedule($"Simulation a dummy {Id}", () => Simulation.StartSimulation().AsTask());
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
                var lines = message.Replace(Environment.NewLine, "\n").Split('\n');
                if (lines.Length == 0)
                {
                    return;
                }

                await UniTask.SwitchToMainThread();

                foreach (var line in lines)
                {
                    var lineToDisplay = line.Trim();
                    if (lineToDisplay.Length == 0)
                    {
                        continue;
                    }

                    foreach (var lline in WrapLine(line))
                    {
                        ChatManager.serverSendMessage(text: lline, color: color.ToUnityColor(), toPlayer: Player.SteamPlayer,
                            iconURL: iconUrl, useRichTextFormatting: isRich);
                    }
                }
            }

            return PrintMessageTask().AsTask();
        }

        private IEnumerable<string> WrapLine(string line)
        {
            var words = line.Split(' ');
            var lines = new List<string>();
            var currentLine = "";
            const int maxLength = 90;

            foreach (var currentWord in words)
            {
                if (currentLine.Length > maxLength ||
                    currentLine.Length + currentWord.Length > maxLength)
                {
                    lines.Add(currentLine);
                    currentLine = "";
                }

                if (currentLine.Length > 0)
                {
                    currentLine += " " + currentWord;
                }
                else
                {
                    currentLine += currentWord;
                }
            }

            if (currentLine.Length > 0)
            {
                lines.Add(currentLine);
            }

            return lines;
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