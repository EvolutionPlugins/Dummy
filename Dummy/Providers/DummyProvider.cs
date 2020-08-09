using EvolutionPlugins.Dummy.API;
using EvolutionPlugins.Dummy.Models;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using OpenMod.API.Users;
using OpenMod.Core.Helpers;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Color = UnityEngine.Color;

namespace EvolutionPlugins.Dummy.Providers
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    public class DummyProvider : IDummyProvider, IAsyncDisposable
    {
        private bool m_IsDisposing;

        private readonly Dictionary<CSteamID, PlayerDummy> m_Dummies;

        public IReadOnlyDictionary<CSteamID, PlayerDummy> Dummies => m_Dummies;

        public DummyProvider()
        {
            m_Dummies = new Dictionary<CSteamID, PlayerDummy>();
            m_IsDisposing = false;

            Provider.onServerDisconnected += OnServerDisconnected;
            ChatManager.onServerSendingMessage += OnServerSendingMessage;
            DamageTool.damagePlayerRequested += DamageTool_damagePlayerRequested;
        }

        #region Events
        protected virtual void OnServerSendingMessage(ref string text, ref Color color, SteamPlayer fromPlayer,
            SteamPlayer toPlayer, EChatMode mode, ref string iconURL, ref bool useRichTextFormatting)
        {
            if (toPlayer == null)
            {
                return;
            }

            if (!Dummies.ContainsKey(toPlayer.playerID.steamID))
            {
                return;
            }

            var data = Dummies[toPlayer.playerID.steamID];

            foreach (var owner in data.Data.Owners)
            {
                var steamPlayerOwner = PlayerTool.getSteamPlayer(owner);
                if (steamPlayerOwner == null)
                {
                    continue;
                }
                ChatManager.serverSendMessage($"Dummy {toPlayer.playerID.steamID} got message: {text}", color,
                    toPlayer: steamPlayerOwner, iconURL: iconURL, useRichTextFormatting: true);
            }
        }

        protected virtual void OnServerDisconnected(CSteamID steamID)
        {
            AsyncHelper.RunSync(() => RemoveDummyAsync(steamID));
        }

        protected virtual void DamageTool_damagePlayerRequested(ref DamagePlayerParameters parameters, ref bool shouldAllow)
        {
            if (!Dummies.ContainsKey(parameters.player.channel.owner.playerID.steamID))
            {
                return;
            }
            shouldAllow = false;
            var totalTimes = parameters.times;

            if (parameters.respectArmor)
            {
                totalTimes *= DamageTool.getPlayerArmor(parameters.limb, parameters.player);
            }
            if (parameters.applyGlobalArmorMultiplier)
            {
                totalTimes *= Provider.modeConfigData.Players.Armor_Multiplier;
            }
            var totalDamage = (byte)Mathf.Min(255, parameters.damage * totalTimes);

            var killerId = parameters.killer;

            ChatManager.say(killerId, $"Amount damage to dummy: {totalDamage}", Color.green, true);
        }
        #endregion

        public Task<bool> AddDummyAsync(CSteamID Id, PlayerDummyData playerDummyData)
        {
            if (m_Dummies.ContainsKey(Id))
            {
                return Task.FromResult(false);
            }
            m_Dummies.Add(Id, new PlayerDummy(playerDummyData, Dummies.Count));
            return Task.FromResult(true);
        }

        public Task<bool> RemoveDummyAsync(CSteamID Id)
        {
            return Task.FromResult(m_Dummies.Remove(Id));
        }

        public Task ClearAllDummiesAsync()
        {
            m_Dummies.Clear();
            return Task.CompletedTask;
        }

        public async Task KickTimerTask(ulong id, uint timer)
        {
            if (timer == 0)
            {
                return;
            }

            await Task.Delay((int)(timer * 1000));

            var user = await FindDummyAsync(id);
            if (user == null)
            {
                return;
            }
            await user.Session.DisconnectAsync();
        }

        public Task<CSteamID> GetAvailableIdAsync()
        {
            var result = new CSteamID(1);

            while (Dummies.ContainsKey(result))
            {
                result.m_SteamID++;
            }
            return Task.FromResult(result);
        }

        public async Task<PlayerDummy> FindDummyAsync(ulong id)
        {
            return Dummies.Values.FirstOrDefault(p => p.Id == id.ToString());
        }

        public Task<bool> GetDummyDataAsync(ulong Id, out PlayerDummyData playerDummyData)
        {
            playerDummyData = null;
            Task<bool> result = Task.FromResult(m_Dummies.TryGetValue((CSteamID)Id, out PlayerDummy dummy));
            if (dummy != null) playerDummyData = dummy.Data;
            return result;
        }

        public ValueTask DisposeAsync()
        {
            if (m_IsDisposing)
            {
                return new ValueTask(Task.CompletedTask);
            }
            m_IsDisposing = true;

            Provider.onServerDisconnected -= OnServerDisconnected;
            ChatManager.onServerSendingMessage -= OnServerSendingMessage;
            DamageTool.damagePlayerRequested -= DamageTool_damagePlayerRequested;

            foreach (var cSteamID in m_Dummies.Keys)
            {
                Provider.kick(cSteamID, "");
            }

            return new ValueTask(ClearAllDummiesAsync());
        }
    }
}
