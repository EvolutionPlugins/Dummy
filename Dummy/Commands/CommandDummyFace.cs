using OpenMod.Core.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Commands
{
    [Command("face")]
    [CommandDescription("Send a face to dummy")]
    [CommandParent(typeof(CommandDummy))]
    [CommandSyntax("<face>")]
    public class CommandDummyFace : Command
    {
        public CommandDummyFace(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override Task OnExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}
