using Dummy.API;
using OpenMod.Core.Commands;
using System;
using System.Threading.Tasks;
using Command = OpenMod.Core.Commands.Command;

namespace Dummy.Commands
{
    [Command("clear")]
    [CommandDescription("Clears all dummies")]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummyClear : Command
    {
        private readonly IDummyProvider m_DummyProvider;

        public CommandDummyClear(IServiceProvider serviceProvider, IDummyProvider dummyProvider) : base(serviceProvider)
        {
            m_DummyProvider = dummyProvider;
        }

        protected override Task OnExecuteAsync()
        {
            return m_DummyProvider.ClearDummiesAsync();
        }
    }
}
