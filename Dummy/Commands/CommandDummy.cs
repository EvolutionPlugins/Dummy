using OpenMod.Core.Commands;
using System;
using System.Threading.Tasks;
using Command = OpenMod.Core.Commands.Command;

namespace EvolutionPlugins.Dummy.Commands
{
    [Command("dummy")]
    [CommandDescription("---")]
    [CommandSyntax("<create|remove|clear|tphere|execute|gesture|stance|face>")]
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
}
