using OpenMod.Core.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Commands
{
    [Command("clear")]
    [CommandDescription("Clears all dummies")]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummyClear : Command
    {
        public CommandDummyClear(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override Task OnExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}
