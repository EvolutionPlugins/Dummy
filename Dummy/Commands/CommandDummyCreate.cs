using EvolutionPlugins.Dummy.Providers;
using Microsoft.Extensions.Configuration;
using OpenMod.API.Commands;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Core.Users;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Color = System.Drawing.Color;
using Command = OpenMod.Core.Commands.Command;

namespace EvolutionPlugins.Dummy.Commands
{

    [Command("create")]
    [CommandDescription("Creates a dummy")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummyCreate : Command
    {
        private readonly IConfiguration m_Configuration;
        private readonly IDummyProvider m_DummyProvider;

        public CommandDummyCreate(IServiceProvider serviceProvider, IConfiguration configuration,
            IDummyProvider dummyProvider) : base(serviceProvider)
        {
            m_Configuration = configuration;
            m_DummyProvider = dummyProvider;
        }

        protected override Task OnExecuteAsync()
        {
            return CreateDummy((UnturnedUser)Context.Actor, false);
        }

        private async Task CreateDummy(UnturnedUser user, bool copy)
        {
            var amountDummiesConfig = m_Configuration.GetSection("AmountDummiesInSameTime").Get<byte>();
            if (amountDummiesConfig != 0 && m_DummyProvider.Dummies.Count + 1 > amountDummiesConfig)
            {
                await user.PrintMessageAsync("Dummy can't be created. Amount dummies overflow", Color.Yellow);
                return;
            }

            var id = m_DummyProvider.GetAvailableId();

            await m_DummyProvider.AddDummyAsync(id, new DummyData() { Owners = new List<CSteamID> { user.SteamId } });

            var steamPlayer = user.SteamPlayer;

            Provider.pending.Add(new SteamPending(new SteamPlayerID(id, 0, "dummy", "dummy", "dummy", CSteamID.Nil),
                true, steamPlayer.face, steamPlayer.hair, steamPlayer.beard, steamPlayer.skin, steamPlayer.color,
                UnityEngine.Color.white, steamPlayer.hand, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, Array.Empty<ulong>(),
                EPlayerSkillset.NONE, "english", CSteamID.Nil));

            Provider.accept(new SteamPlayerID(id, 0, "dummy", "dummy", "dummy", CSteamID.Nil), true, false,
                steamPlayer.face, steamPlayer.hair, steamPlayer.beard, steamPlayer.skin, steamPlayer.color,
                UnityEngine.Color.white, steamPlayer.hand, steamPlayer.shirtItem, steamPlayer.pantsItem, steamPlayer.hatItem,
                steamPlayer.backpackItem, steamPlayer.vestItem, steamPlayer.maskItem, steamPlayer.glassesItem,
                steamPlayer.skinItems, steamPlayer.skinTags, steamPlayer.skinDynamicProps, EPlayerSkillset.NONE,
                "english", CSteamID.Nil);

            if (!copy)
            {
                Provider.pending.Add(new SteamPending(
                    new SteamPlayerID(id, 0, "dummy", "dummy", "dummy", CSteamID.Nil), true, 0, 0, 0,
                    UnityEngine.Color.white, UnityEngine.Color.white, UnityEngine.Color.white, false, 0UL, 0UL, 0UL, 0UL,
                    0UL, 0UL, 0UL, Array.Empty<ulong>(), EPlayerSkillset.NONE, "english", CSteamID.Nil));

                Provider.accept(new SteamPlayerID(id, 1, "dummy", "dummy", "dummy", CSteamID.Nil), true, false, 0, 0, 0,
                    UnityEngine.Color.white, UnityEngine.Color.white, UnityEngine.Color.white, false, 0, 0, 0, 0, 0, 0,
                    0, Array.Empty<int>(), Array.Empty<string>(), Array.Empty<string>(), EPlayerSkillset.NONE, "english",
                    CSteamID.Nil);
            }

            var dummy = Provider.clients.Last();
            dummy.player.teleportToLocationUnsafe(user.Player.transform.position, user.Player.transform.rotation.eulerAngles.y);

            await user.PrintMessageAsync($"Dummy ({id.m_SteamID}) has created");
        }
    }
    //public void Execute(IRocketPlayer caller, string[] command)
    //{
    //    var player = (UnturnedPlayer)caller;

    //    if (command.Length == 0)
    //    {
    //        UnturnedChat.Say(player, $"Wrong command usage. Use correct: {Syntax}", Color.yellow);
    //        return;
    //    }

    //    switch (command[0].ToLower())
    //    {
    //        case "create":
    //            CreateDummy(player, false);
    //            return;

    //        case "copy":
    //            CreateDummy(player, true);
    //            return;

    //        case "remove":
    //        case "kick":
    //            if (command.Length != 2 || !byte.TryParse(command[1], out var id))
    //            {
    //                UnturnedChat.Say(player, "Wrong command usage. Use correct: /dummy remove <id>", Color.yellow);
    //                return;
    //            }

    //            RemoveDummy(player, id);
    //            return;

    //        case "clear":
    //            ClearAllDummies(player);
    //            return;

    //        case "execute":
    //            if (command.Length < 3 || !byte.TryParse(command[1], out id))
    //            {
    //                UnturnedChat.Say(player, "Wrong command usage. Use correct: /dummy execute <id> <command> [args]", Color.yellow);
    //                return;
    //            }
    //            ExecuteCommandDummy(player, id, string.Join(" ", command.Skip(2)));
    //            return;

    //        case "teleport":
    //        case "tp":
    //            if (command.Length != 2 || !byte.TryParse(command[1], out id))
    //            {
    //                UnturnedChat.Say(player, "Wrong command usage. Use correct: /dummy tp <id>", Color.yellow);
    //                return;
    //            }
    //            TeleportDummy(player, id);
    //            return;

    //        case "gesture":
    //            if (command.Length != 3 || !byte.TryParse(command[1], out id))
    //            {
    //                UnturnedChat.Say(player, "Wrong command usage. Use correct: /dummy gesture <id> <gesture>", Color.yellow);
    //                return;
    //            }
    //            GestureDummy(player, id, command[2]);
    //            return;

    //        case "stance":
    //            if (command.Length != 3 || !byte.TryParse(command[1], out id))
    //            {
    //                UnturnedChat.Say(player, "Wrong command usage. Use correct: /dummy stance <id> <stance>", Color.yellow);
    //                return;
    //            }
    //            StanceDummy(player, id, command[2]);
    //            return;

    //        case "face":
    //            if (command.Length != 3 || !byte.TryParse(command[1], out id) || !byte.TryParse(command[2], out var faceId))
    //            {
    //                UnturnedChat.Say(player, "Wrong command usage. Use correct: /dummy face <id> <faceId>", Color.yellow);
    //                return;
    //            }
    //            FaceDummy(player, id, faceId);
    //            return;
    //        default:
    //            UnturnedChat.Say(player, $"Wrong command usage. Use correct: {Syntax}", Color.yellow);
    //            break;
    //    }
    //}

    // TODO:
    /*private void FaceDummy(UnturnedPlayer player, byte id, byte faceId)
        {
            if (!global::Dummy.Instance.Dummies.ContainsKey((CSteamID)id))
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
            if (!global::Dummy.Instance.Dummies.ContainsKey((CSteamID)id))
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
            if (!global::Dummy.Instance.Dummies.ContainsKey((CSteamID)id))
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
            if (!global::Dummy.Instance.Dummies.ContainsKey((CSteamID)id))
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
            if (!global::Dummy.Instance.Dummies.ContainsKey((CSteamID)id))
            {
                UnturnedChat.Say(player, $"Dummy ({id}) not found", Color.red);
                return;
            }

            var x = R.Commands.Execute(UnturnedPlayer.FromCSteamID((CSteamID)id), command);

            ChatManager.serverSendMessage($"Dummy ({id}) has {(x ? "successfully" : "<color=red>unsuccessfully</color>")} executed command", Color.green, toPlayer: player.SteamPlayer(), useRichTextFormatting: true);
        }

        private void ClearAllDummies(UnturnedPlayer player)
        {
            foreach (var dummy in global::Dummy.Instance.Dummies)
            {
                var _dummy = Provider.clients.Find(k => k.playerID.steamID == dummy.Key);
                if (_dummy == null)
                {
                    UnturnedChat.Say(player, $"Dummy ({_dummy.playerID.steamID}) failed to remove!", Color.red);
                    continue;
                }
                if (dummy.Value.Coroutine != null)
                    global::Dummy.Instance.StopCoroutine(dummy.Value.Coroutine);

                Provider.kick(_dummy.playerID.steamID, "");
            }

            global::Dummy.Instance.Dummies.Clear();
            UnturnedChat.Say(player, "Dummies were removed", Color.green);
        }

        private void RemoveDummy(UnturnedPlayer player, byte id)
        {
            if (!global::Dummy.Instance.Dummies.Any((object k) => k.Key.m_SteamID == id))
            {
                UnturnedChat.Say(player, $"Dummy ({id}) not found", Color.red);
            }
            var data = global::Dummy.Instance.Dummies[(CSteamID)id];

            var dummy = Provider.clients.Find(k => k.playerID.steamID.m_SteamID == id);
            if (dummy == null)
            {
                UnturnedChat.Say(player, $"Dummy ({id}) not found", Color.red);
                return;
            }

            Provider.kick(dummy.playerID.steamID, "");

            if (data.Coroutine != null)
                global::Dummy.Instance.StopCoroutine(data.Coroutine);

            global::Dummy.Instance.Dummies.Remove(new CSteamID(id));
            UnturnedChat.Say(player, $"Dummy ({id}) was removed", Color.green);
        }*/


}
