using OpenMod.Core.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Commands
{
    [Command("gesture")]
    [CommandDescription("Make a dummy gesture")]
    [CommandParent(typeof(CommandDummy))]
    [CommandSyntax("<gesture>")]
    public class CommandDummyGesture : Command
    {
        public CommandDummyGesture(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override Task OnExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}
