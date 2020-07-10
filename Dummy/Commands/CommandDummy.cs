using OpenMod.Core.Commands;
using System;
using System.Threading.Tasks;
using Command = OpenMod.Core.Commands.Command;

namespace EvolutionPlugins.Dummy.Commands
{
    [Command("dummy")]
    [CommandDescription("---")]
    [CommandSyntax("<create|remove|clear|teleport|execute|gesture|stance|face>")]
    public class CommandDummy : Command
    {
        public CommandDummy(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override Task OnExecuteAsync()
        {
            throw new CommandWrongUsageException(Context);
        }
    }

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
