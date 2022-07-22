using OpenMod.Core.Commands;
using System;
using System.Threading.Tasks;
using Command = OpenMod.Core.Commands.Command;

namespace Dummy.Commands
{
    [Command("dummy")]
    // ReSharper disable StringLiteralTypo
    [CommandSyntax("<copy/create/remove/clear/tphere/button/execute/face/gesture/inputfield/jump/look>")]
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