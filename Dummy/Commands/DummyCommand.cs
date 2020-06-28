using Rocket.API;
using Rocket.Core;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dummy.Commands
{
    public class DummyCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "dummy";

        public string Help => "";

        public string Syntax => "/dummy <create | remove | clear | teleport | execute>";

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
                    CreateDummy(player, command.Length == 2 && bool.TryParse(command[1], out var copy) && copy);
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
                    ClearAllDummies(player);
                    return;
                case "execute":
                    if (command.Length < 3 || !byte.TryParse(command[1], out id))
                    {
                        UnturnedChat.Say(player, "Wrong command usage. Use correct: /dummy execute <id> <command> [args]", Color.yellow);
                        return;
                    }
                    ExecuteCommandDummy(player, id, string.Join(" ", command.Skip(2)));
                    return;
                case "teleport":
                case "tp":
                    if (command.Length != 2 || !byte.TryParse(command[1], out id))
                    {
                        UnturnedChat.Say(player, "Wrong command usage. Use correct: /dummy tp <id>", Color.yellow);
                        return;
                    }
                    TeleportDummy(player, id);
                    return;
                default:
                    UnturnedChat.Say(player, $"Wrong command usage. Use correct: {Syntax}", Color.yellow);
                    break;
            }
        }

        private void TeleportDummy(UnturnedPlayer player, byte id)
        {
            if(!Dummy.Instance.Dummies.ContainsKey((CSteamID)id))
            {
                UnturnedChat.Say(player, $"Dummy ({id}) not found", Color.red);
                return;
            }
            var dummy = Provider.clients.Find(k => k.playerID.steamID.m_SteamID == id);
            if(dummy == null)
            {
                UnturnedChat.Say(player, $"Dummy ({id}) not found", Color.red);
                return;
            }

            (dummy.player.movement.updates ?? (dummy.player.movement.updates = new List<PlayerStateUpdate>())).Add(new PlayerStateUpdate(player.Position, player.Player.look.angle, player.Player.look.rot));
            //dummy.player.look.simulate(player.Player.look.yaw, player.Player.look.pitch, PlayerInput.RATE);
        }

        private void ExecuteCommandDummy(UnturnedPlayer player, byte id, string command)
        {
            if(!Dummy.Instance.Dummies.ContainsKey((CSteamID)id))
            {
                UnturnedChat.Say(player, $"Dummy ({id}) not found", Color.red);
                return;
            }

            var x = R.Commands.Execute(UnturnedPlayer.FromCSteamID((CSteamID)id), command);

            UnturnedChat.Say(player, $"Dummy ({id}) has {(x ? "successfully" : "unsuccessfully")} executed command");
        }

        private void ClearAllDummies(UnturnedPlayer player)
        {
            foreach (var dummy in Dummy.Instance.Dummies)
            {
                var _dummy = Provider.clients.Find(k => k.playerID.steamID == dummy.Key);
                if (_dummy == null)
                {
                    UnturnedChat.Say(player, $"Dummy ({_dummy.playerID.steamID}) failed to remove!", Color.red);
                    continue;
                }
                if (dummy.Value != null)
                    Dummy.Instance.StopCoroutine(dummy.Value);

                Provider.kick(_dummy.playerID.steamID, "");
            }

            Dummy.Instance.Dummies.Clear();
            UnturnedChat.Say(player, "Dummies were removed", Color.green);
        }

        private void RemoveDummy(UnturnedPlayer player, byte id)
        {
            if (!Dummy.Instance.Dummies.Any(k => k.Key.m_SteamID == id))
            {
                UnturnedChat.Say(player, $"Dummy ({id}) not found", Color.red);
            }
            var coroutine = Dummy.Instance.Dummies[(CSteamID)id];

            var dummy = Provider.clients.Find(k => k.playerID.steamID.m_SteamID == id);
            if (dummy == null)
            {
                UnturnedChat.Say(player, $"Dummy ({id}) not found", Color.red);
                return;
            }

            Provider.kick(dummy.playerID.steamID, "");

            if (coroutine != null)
                Dummy.Instance.StopCoroutine(coroutine);

            Dummy.Instance.Dummies.Remove(new CSteamID(id));
            UnturnedChat.Say(player, $"Dummy ({id}) was removed", Color.green);
        }

        private void CreateDummy(UnturnedPlayer player, bool copy)
        {
            if (Dummy.Instance.Config.AmountDummiesInSameTime != 0
                && Dummy.Instance.Dummies.Count + 1 > Dummy.Instance.Config.AmountDummiesInSameTime)
            {
                UnturnedChat.Say(player, "Dummy can't be created. Amount dummies overflow", Color.red);
                return;
            }

            var id = Dummy.GetAvailableID();

            Dummy.Instance.Dummies.Add(id, Dummy.Instance.GetCoroutine(id));

            if (copy)
            {
                var steamPlayer = player.SteamPlayer();

                Provider.pending.Add(new SteamPending(new SteamPlayerID(id, 0, "dummy", "dummy", "dummy", CSteamID.Nil),
                    true, steamPlayer.face, steamPlayer.hair, steamPlayer.beard, steamPlayer.skin, steamPlayer.color,
                    Color.white, steamPlayer.hand, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, Array.Empty<ulong>(),
                    EPlayerSkillset.NONE, "english", CSteamID.Nil));

                Provider.accept(new SteamPlayerID(id, 0, "dummy", "dummy", "dummy", CSteamID.Nil), true, false,
                    steamPlayer.face, steamPlayer.hair, steamPlayer.beard, steamPlayer.skin, steamPlayer.color,
                    Color.white, steamPlayer.hand, steamPlayer.shirtItem, steamPlayer.pantsItem, steamPlayer.hatItem,
                    steamPlayer.backpackItem, steamPlayer.vestItem, steamPlayer.maskItem, steamPlayer.glassesItem,
                    steamPlayer.skinItems, steamPlayer.skinTags, steamPlayer.skinDynamicProps, EPlayerSkillset.NONE,
                    "english", CSteamID.Nil);
            }
            else
            {
                Provider.pending.Add(new SteamPending(new SteamPlayerID(id, 0, "dummy", "dummy", "dummy", CSteamID.Nil),
                true, 0, 0, 0, Color.white, Color.white, Color.white, false, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL,
                Array.Empty<ulong>(), EPlayerSkillset.NONE, "english", CSteamID.Nil));

                Provider.accept(new SteamPlayerID(id, 1, "dummy", "dummy", "dummy", CSteamID.Nil), true, false, 0,
                    0, 0, Color.white, Color.white, Color.white, false, 0, 0, 0, 0, 0, 0, 0, Array.Empty<int>(), Array.Empty<string>(),
                    Array.Empty<string>(), EPlayerSkillset.NONE, "english", CSteamID.Nil);
            }

            var dummy = Provider.clients.Last();
            dummy.player.teleportToLocationUnsafe(player.Position, player.Rotation);

            UnturnedChat.Say(player, $"Dummy ({id.m_SteamID}) has created");
        }
    }
}
