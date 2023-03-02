using Dummy.Commands.Helpers;
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
        internal static readonly CommandArgument s_NameArgument = new("--name");

        public CommandDummy(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override Task OnExecuteAsync()
        {
            return Task.FromException(new CommandWrongUsageException(Context));
        }
    }
}