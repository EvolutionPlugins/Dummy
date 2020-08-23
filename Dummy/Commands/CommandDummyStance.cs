using OpenMod.Core.Commands;
using System;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Commands
{
    [Command("stance")]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummyStance : Command
    {
        public CommandDummyStance(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override Task OnExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}
