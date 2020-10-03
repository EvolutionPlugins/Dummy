using Dummy.API;
using Microsoft.Extensions.Localization;
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
        private readonly IStringLocalizer m_StringLocalizer;

        public CommandDummyClear(IServiceProvider serviceProvider, IDummyProvider dummyProvider,
            IStringLocalizer stringLocalizer) : base(serviceProvider)
        {
            m_DummyProvider = dummyProvider;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            await m_DummyProvider.ClearDummiesAsync();
            await PrintAsync(m_StringLocalizer["commands:general:clear"]);
        }
    }
}