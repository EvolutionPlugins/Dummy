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
    public class PlayerDummy : IUser, IDisposable
    {
        public PlayerDummyData Data { get; }
        
        public PlayerDummyActionThread Actions { get; }

        private Thread _actionThreadControl;

        public PlayerDummy(PlayerDummyData data, int index)
        {
            Data = data;
            Session = data.UnturnedUser.Session;
            DisplayName = "Dummy " + index;
            Id = data.UnturnedUser.Id;
            Actions = new PlayerDummyActionThread(this);
            _actionThreadControl = new Thread(Actions.Start);
            _actionThreadControl.Start();
        }

        public string Id { get; } 
        
        public string Type { get; } = "Player";
        public string DisplayName { get; }
        
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

        public async Task PrintMessageAsync(string message)
        {
            throw new NotSupportedException();
        }

        public async Task PrintMessageAsync(string message, Color color)
        {
            throw new NotSupportedException();
        }

        public async Task SavePersistentDataAsync<T>(string key, T data)
        {
            throw new NotSupportedException();
        }

        public async Task<T> GetPersistentDataAsync<T>(string key)
        {
            throw new NotSupportedException();
        }

        public IUserSession Session { get; }
    }
}