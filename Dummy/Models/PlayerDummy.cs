// ReSharper disable CheckNamespace

using EvolutionPlugins.Dummy.Models;
using EvolutionPlugins.Dummy.Threads;
using OpenMod.API.Users;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Threading;

namespace EvolutionPlugins.Dummy
{
#pragma warning disable CA1063 // Implement IDisposable Correctly
    public class PlayerDummy : IDisposable
#pragma warning restore CA1063 // Implement IDisposable Correctly
    {
        public PlayerDummyData Data { get; }
        public IUserSession Session { get; }
        public PlayerDummyActionThread Actions { get; }

        private readonly Thread _actionThreadControl;

        public PlayerDummy(PlayerDummyData data)
        {
            Data = data;
            Actions = new PlayerDummyActionThread(this);
            _actionThreadControl = new Thread(Actions.Start);
            _actionThreadControl.Start();
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
            _actionThreadControl.Abort();
        }
    }
}