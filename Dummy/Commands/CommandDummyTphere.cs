using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using System;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Commands
{
    [Command("tphere")]
    [CommandDescription("Teleport to you a dummy")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandSyntax("<id>")]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummyTphere : Command
    {
        public CommandDummyTphere(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override Task OnExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}
