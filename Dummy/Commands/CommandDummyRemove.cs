using OpenMod.Core.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Commands
{
    [Command("remove")]
    [CommandAlias("kick")]
    [CommandAlias("delete")]
    [CommandDescription("Removes a dummy by id")]
    [CommandParent(typeof(CommandDummy))]
    [CommandSyntax("<id>")]
    public class CommandDummyRemove : Command
    {
        public CommandDummyRemove(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override Task OnExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}
