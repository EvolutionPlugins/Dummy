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
    public class CommandDummy : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "dummy";

        public string Help => "";

        public string Syntax => "/dummy <create | remove | clear | teleport | execute | gesture | stance | face>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "dummy" };

        private Dummy Instance => Dummy.Instance;

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
                    CreateDummy(player, false);
                    return;

                case "copy":
                    CreateDummy(player, true);
                    return;

                case "remove":
                case "kick":
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

                case "gesture":
                    if (command.Length != 3 || !byte.TryParse(command[1], out id))
                    {
                        UnturnedChat.Say(player, "Wrong command usage. Use correct: /dummy gesture <id> <gesture>", Color.yellow);
                        return;
                    }
                    GestureDummy(player, id, command[2]);
                    return;

                case "stance":
                    if (command.Length != 3 || !byte.TryParse(command[1], out id))
                    {
                        UnturnedChat.Say(player, "Wrong command usage. Use correct: /dummy stance <id> <stance>", Color.yellow);
                        return;
                    }
                    StanceDummy(player, id, command[2]);
                    return;

                case "face":
                    if (command.Length != 3 || !byte.TryParse(command[1], out id) || !byte.TryParse(command[2], out var faceId))
                    {
                        UnturnedChat.Say(player, "Wrong command usage. Use correct: /dummy face <id> <faceId>", Color.yellow);
                        return;
                    }
                    FaceDummy(player, id, faceId);
                    return;

                case "button":
                    if (command.Length != 3 || !byte.TryParse(command[1], out id))
                    {
                        UnturnedChat.Say(player, "Wrong command usage. Use correct: /dummy button <id> <buttonName>", Color.yellow);
                        return;
                    }
                    ButtonDummy(player, id, command[2]);
                    return;

                case "inputfield":
                case "if":
                    if (command.Length < 3 || !byte.TryParse(command[1], out id))
                    {
                        UnturnedChat.Say(player, "Wrong command usage. Use correct: /dummy if <id> <inputFieldName> <inputtedText>", Color.yellow);
                        return;
                    }
                    InputFieldDummy(player, id, command[2], string.Join(" ", command.Skip(3)));
                    return;
                default:
                    UnturnedChat.Say(player, $"Wrong command usage. Use correct: {Syntax}", Color.yellow);
                    break;
            }
        }

        private void InputFieldDummy(UnturnedPlayer player, byte id, string inputFieldName, string inputtedText)
        {
            if (!Dummy.Instance.Dummies.ContainsKey((CSteamID)id))
            {
                UnturnedChat.Say(player, $"Dummy ({id}) not found", Color.red);
                return;
            }
            var dummy = Provider.clients.Find(k => k.playerID.steamID.m_SteamID == id);
            if (dummy == null)
            {
                UnturnedChat.Say(player, $"Dummy ({id}) not found", Color.red);
                return;
            }

            EffectManager.instance.tellEffectTextCommitted(dummy.playerID.steamID, inputFieldName, inputtedText);
            UnturnedChat.Say(player, $"Dummy ({id}) inputted a text", Color.green);
        }

        private void ButtonDummy(UnturnedPlayer player, byte id, string buttonName)
        {
            if (!Dummy.Instance.Dummies.ContainsKey((CSteamID)id))
            {
                UnturnedChat.Say(player, $"Dummy ({id}) not found", Color.red);
                return;
            }
            var dummy = Provider.clients.Find(k => k.playerID.steamID.m_SteamID == id);
            if (dummy == null)
            {
                UnturnedChat.Say(player, $"Dummy ({id}) not found", Color.red);
                return;
            }

            EffectManager.instance.tellEffectClicked(dummy.playerID.steamID, buttonName);
            UnturnedChat.Say(player, $"Dummy ({id}) clicked to button", Color.green);
        }

        private void FaceDummy(UnturnedPlayer player, byte id, byte faceId)
        {
            if (!Dummy.Instance.Dummies.ContainsKey((CSteamID)id))
            {
                UnturnedChat.Say(player, $"Dummy ({id}) not found", Color.red);
                return;
            }
            var dummy = Provider.clients.Find(k => k.playerID.steamID.m_SteamID == id);
            if (dummy == null)
            {
                UnturnedChat.Say(player, $"Dummy ({id}) not found", Color.red);
                return;
            }

            if (faceId > Customization.FACES_FREE + Customization.FACES_PRO)
            {
                UnturnedChat.Say(player, $"Can't change to {faceId} because is higher {Customization.FACES_FREE + Customization.FACES_PRO}", Color.red);
                return;
            }

            dummy.player.clothing.channel.send("tellSwapFace", ESteamCall.NOT_OWNER, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
            {
                faceId
            });
        }

        private void StanceDummy(UnturnedPlayer player, byte id, string stance)
        {
            if (!Dummy.Instance.Dummies.ContainsKey((CSteamID)id))
            {
                UnturnedChat.Say(player, $"Dummy ({id}) not found", Color.red);
                return;
            }
            var dummy = Provider.clients.Find(k => k.playerID.steamID.m_SteamID == id);
            if (dummy == null)
            {
                UnturnedChat.Say(player, $"Dummy ({id}) not found", Color.red);
                return;
            }

            if (!Enum.TryParse<EPlayerStance>(stance.ToUpper(), out var eStance))
            {
                UnturnedChat.Say(player, $"Unable find a stance {stance}", Color.red);
                return;
            }

            dummy.player.stance.checkStance(eStance, false);
        }

        private void GestureDummy(UnturnedPlayer player, byte id, string gesture)
        {
            if (!Dummy.Instance.Dummies.ContainsKey((CSteamID)id))
            {
                UnturnedChat.Say(player, $"Dummy ({id}) not found", Color.red);
                return;
            }
            var dummy = Provider.clients.Find(k => k.playerID.steamID.m_SteamID == id);
            if (dummy == null)
            {
                UnturnedChat.Say(player, $"Dummy ({id}) not found", Color.red);
                return;
            }

            if (!Enum.TryParse<EPlayerGesture>(gesture.ToUpper(), out var eGesture))
            {
                UnturnedChat.Say(player, $"Unable find a gesture {gesture}", Color.red);
                return;
            }

            dummy.player.animator.sendGesture(eGesture, false);
        }

        private void TeleportDummy(UnturnedPlayer player, byte id)
        {
            if (!Dummy.Instance.Dummies.ContainsKey((CSteamID)id))
            {
                UnturnedChat.Say(player, $"Dummy ({id}) not found", Color.red);
                return;
            }
            var dummy = Provider.clients.Find(k => k.playerID.steamID.m_SteamID == id);
            if (dummy == null)
            {
                UnturnedChat.Say(player, $"Dummy ({id}) not found", Color.red);
                return;
            }

            dummy.player.transform.position = player.Position;
            dummy.player.transform.rotation = player.Player.transform.rotation;

            (dummy.player.movement.updates
             ?? (dummy.player.movement.updates = new List<PlayerStateUpdate>())).Add(new PlayerStateUpdate(player.Position,
                player.Player.look.angle, player.Player.look.rot));
        }

        private void ExecuteCommandDummy(UnturnedPlayer player, byte id, string command)
        {
            if (!Dummy.Instance.Dummies.ContainsKey((CSteamID)id))
            {
                UnturnedChat.Say(player, $"Dummy ({id}) not found", Color.red);
                return;
            }

            var x = R.Commands.Execute(UnturnedPlayer.FromCSteamID((CSteamID)id), command);

            ChatManager.serverSendMessage($"Dummy ({id}) has {(x ? "successfully" : "<color=red>unsuccessfully</color>")} executed command", Color.green, toPlayer: player.SteamPlayer(), useRichTextFormatting: true);
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
                if (dummy.Value.Coroutine != null)
                    Dummy.Instance.StopCoroutine(dummy.Value.Coroutine);

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
            var data = Dummy.Instance.Dummies[(CSteamID)id];

            var dummy = Provider.clients.Find(k => k.playerID.steamID.m_SteamID == id);
            if (dummy == null)
            {
                UnturnedChat.Say(player, $"Dummy ({id}) not found", Color.red);
                return;
            }

            Provider.kick(dummy.playerID.steamID, "");

            if (data.Coroutine != null)
                Dummy.Instance.StopCoroutine(data.Coroutine);

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

            Dummy.Instance.Dummies.Add(id, new DummyData(new List<CSteamID>() { player.CSteamID }, Dummy.Instance.GetCoroutine(id)));

            if (copy)
            {
                var steamPlayer = player.SteamPlayer();

                Provider.pending.Add(new SteamPending(null, new SteamPlayerID(id, 0, "dummy", "dummy", "dummy", CSteamID.Nil),
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
                Provider.pending.Add(new SteamPending(null, new SteamPlayerID(id, 0, "dummy", "dummy", "dummy", CSteamID.Nil),
                true, 0, 0, 0, Color.white, Color.white, Color.white, false, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL,
                Array.Empty<ulong>(), EPlayerSkillset.NONE, "english", CSteamID.Nil));

                Provider.accept(new SteamPlayerID(id, 1, "dummy", "dummy", "dummy", CSteamID.Nil), true, false, 0,
                    0, 0, Color.white, Color.white, Color.white, false, 0, 0, 0, 0, 0, 0, 0, Array.Empty<int>(), Array.Empty<string>(),
                    Array.Empty<string>(), EPlayerSkillset.NONE, "english", CSteamID.Nil);
            }

            var dummy = Provider.clients.Last();
            dummy.player.teleportToLocationUnsafe(player.Position, player.Rotation);

            var data = Instance.Dummies[dummy.playerID.steamID];
            data.player = dummy.player;
            Instance.Dummies[dummy.playerID.steamID] = data;

            UnturnedChat.Say(player, $"Dummy ({id.m_SteamID}) has created");
        }
    }
}
