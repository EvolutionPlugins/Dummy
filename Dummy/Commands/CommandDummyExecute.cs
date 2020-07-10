using EvolutionPlugins.Dummy.API;
using OpenMod.API.Commands;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using System;
using System.Linq;
using System.Threading.Tasks;
using Color = System.Drawing.Color;
using Command = OpenMod.Core.Commands.Command;

namespace EvolutionPlugins.Dummy.Commands
{

    [Command("execute")]
    [CommandDescription("Execute a command by Dummy")]
    [CommandSyntax("<id> <command>")]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummyExecute : Command
    {
        private readonly IDummyProvider m_DummyProvider;
        private readonly ICommandExecutor m_CommandExecutor;
        private readonly IUserDataStore m_UserDataStore;

        public CommandDummyExecute(IServiceProvider serviceProvider, IDummyProvider dummyProvider,
            ICommandExecutor commandExecutor, IUserDataStore userDataStore) : base(serviceProvider)
        {
            m_DummyProvider = dummyProvider;
            m_CommandExecutor = commandExecutor;
            m_UserDataStore = userDataStore;
        }

        protected override async Task OnExecuteAsync()
        {
            if (Context.Parameters.Count < 2)
            {
                throw new CommandWrongUsageException(Context);
            }

            var id = (CSteamID)await Context.Parameters.GetAsync<ulong>(0);

            if (!m_DummyProvider.Dummies.TryGetValue(id, out _))
            {
                throw new UserFriendlyException($"Dummy \"{id}\" has not found!");
            }

            var dummy = PlayerTool.getPlayer(id); // https://github.com/openmod/openmod/pull/109
            if (dummy == null)
            {
                throw new UserFriendlyException($"Dummy \"{id}\" has not found!");
            }

            var commandContext = await m_CommandExecutor.ExecuteAsync(new UnturnedUser(m_UserDataStore, dummy), Context.Parameters.Skip(1).ToArray(), "");

            await PrintAsync($"Dummy has {(commandContext.Exception == null ? "<color=green>successfully" : "<color=red>unsuccessfully")}</color> executed command");
            if (commandContext.Exception != null && !(commandContext.Exception is UserFriendlyException))
            {
                await PrintAsync(commandContext.Exception.Message, Color.Red);
            }
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
