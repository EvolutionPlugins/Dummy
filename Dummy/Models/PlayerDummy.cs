// ReSharper disable CheckNamespace

using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy.Models;
using EvolutionPlugins.Dummy.Threads;
using OpenMod.API.Users;
using OpenMod.Core.Helpers;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;

namespace EvolutionPlugins.Dummy
{
#pragma warning disable CA1063 // Implement IDisposable Correctly
    public class PlayerDummy : IDisposable
#pragma warning restore CA1063 // Implement IDisposable Correctly
    {
        public PlayerDummyData Data { get; }
        public PlayerDummyActionThread Actions { get; }
        public PlayerDummySimulationThread Simulation { get; }

        public IUserSession Session => Data.UnturnedUser.Session;
        public CSteamID SteamID => Data.UnturnedUser.SteamId;
        public Player Player => Data.UnturnedUser.Player.Player;

        public PlayerDummy(PlayerDummyData data)
        {
            Data = data;
            Actions = new PlayerDummyActionThread(this);
            Simulation = new PlayerDummySimulationThread(this);

            Actions.Enabled = true;
            Simulation.Enabled = true;

            AsyncHelper.Schedule($"Action a dummy {data.UnturnedUser.Id}", () => Actions.Start());
            AsyncHelper.Schedule($"Simulation a dummy {data.UnturnedUser.Id}", () => Simulation.StartSimulation().AsTask());
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerDummy data && data.Data == Data;
        }

        public override int GetHashCode()
        {
            return 1599248077 + EqualityComparer<HashSet<CSteamID>>.Default.GetHashCode(Data.Owners);
        }

        public void Dispose()
        {
            Actions.Enabled = false;
            Simulation.Enabled = false;
        }
    }
}