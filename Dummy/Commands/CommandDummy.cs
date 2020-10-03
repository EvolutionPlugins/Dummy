using OpenMod.Core.Commands;
using System;
using System.Threading.Tasks;
using Command = OpenMod.Core.Commands.Command;

namespace Dummy.Commands
{
    [Command("dummy")]
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