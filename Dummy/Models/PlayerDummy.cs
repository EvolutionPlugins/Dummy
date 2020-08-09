// ReSharper disable CheckNamespace

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using EvolutionPlugins.Dummy.Models;
using EvolutionPlugins.Dummy.Threads;
using OpenMod.API.Users;
using OpenMod.Unturned.Users;
using Steamworks;

namespace EvolutionPlugins.Dummy
{
    public class PlayerDummy : IDisposable
    {
        public PlayerDummyData Data { get; }
        
        public PlayerDummyActionThread Actions { get; }

        private Thread _actionThreadControl;

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
            return 1599248077 + EqualityComparer<List<CSteamID>>.Default.GetHashCode(Data.Owners);
        }

        public void Dispose()
        {
            Actions.Enabled = false;
            _actionThreadControl.Abort();
        }
        

        public IUserSession Session { get; }
    }
}