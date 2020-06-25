using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
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
            }

            switch (command[0].ToLower())
            {
                case "create":
                    CreateDummy(player);
                    return;
                case "remove":
                    if (command.Length != 2)
                    {
                        UnturnedChat.Say(player, "Wrong command usage. Use correct: /dummy remove <id>", Color.yellow);
                    }

                    RemoveDummy(player);
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
            throw new NotImplementedException();
        }

        private void RemoveDummy(UnturnedPlayer player)
        {
            throw new NotImplementedException();
        }

        private void CreateDummy(UnturnedPlayer player)
        {
            pla
        }
    }
}
