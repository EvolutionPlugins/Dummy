﻿using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Dummy.Commands
{
    public class DummyCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "dummy";

        public string Help => "";

        public string Syntax => "/dummy <create | remove | clear>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "dummy" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var player = (UnturnedPlayer)caller;

            if (command.Length == 0)
            {
                UnturnedChat.Say(player, $"Wrong command usage. Use correct: {Syntax}", Color.yellow);
                return;
            }

            switch (command[0].ToLower())
            {
                case "create":
                    CreateDummy(player);
                    return;
                case "remove":
                    if (command.Length != 2 || !byte.TryParse(command[1], out var id))
                    {
                        UnturnedChat.Say(player, "Wrong command usage. Use correct: /dummy remove <id>", Color.yellow);
                        return;
                    }

                    RemoveDummy(player, id);
                    return;
                case "clear":
                    if(command.Length != 2)
                    {
                        UnturnedChat.Say(player, "Wrong command usage. Use correct: /dummy clear", Color.yellow);
                    }

                    ClearAllDummies(player);
                    return;
                default:
                    UnturnedChat.Say(player, $"Wrong command usage. Use correct: {Syntax}", Color.yellow);
                    break;
            }
        }

        private void ClearAllDummies(UnturnedPlayer player)
        {
            foreach (var dummy in Dummy.Instance.Dummies)
            {
                var _dummy = Provider.clients.Find(k => k.playerID.steamID == dummy);
                // It can't be null but I add check
                if (_dummy == null)
                {
                    UnturnedChat.Say(player, $"Dummy ({_dummy.playerID.steamID}) failed to remove!", Color.red);
                }
                Provider.kick(_dummy.playerID.steamID, "");
            }
            UnturnedChat.Say(player, "Dummies were removed", Color.green);
        }

        private void RemoveDummy(UnturnedPlayer player, byte id)
        {
            if(!Dummy.Instance.Dummies.Any(k => k.m_SteamID == id))
            {
                UnturnedChat.Say(player, $"Dummy ({id}) not found", Color.red);
            }
            Dummy.Instance.Dummies.Remove(new CSteamID(id));

            var dummy = Provider.clients.Find(k => k.playerID.steamID.m_SteamID == id);
            // It can't be null but I add check
            if (dummy == null)
            {
                UnturnedChat.Say(player, $"Dummy ({id}) not found", Color.red);
            }
            Provider.kick(dummy.playerID.steamID, "");
            UnturnedChat.Say(player, $"Dummy ({id}) was removed", Color.red);
        }

        private void CreateDummy(UnturnedPlayer player)
        {
            var id = Dummy.GetAvailableID();

            Dummy.Instance.Dummies.Add(id);

            var pending = new SteamPending(new SteamPlayerID(id, 0, "dummy", "dummy", "dummy", CSteamID.Nil), true, 0, 0,
                0, Color.white, Color.white, Color.white, false, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, Array.Empty<ulong>(),
                EPlayerSkillset.NONE, "english", CSteamID.Nil);
            Provider.pending.Add(pending);
            Provider.accept(pending);

            var dummy = Provider.clients.Last();
            dummy.player.teleportToLocationUnsafe(player.Position, player.Rotation);

            UnturnedChat.Say(player, $"Dummy ({id.m_SteamID}) has created");
        }
    }
}