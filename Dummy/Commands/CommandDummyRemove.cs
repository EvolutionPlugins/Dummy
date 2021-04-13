extern alias JetBrainsAnnotations;
using System;
using System.Threading.Tasks;
using Dummy.API;
using JetBrainsAnnotations::JetBrains.Annotations;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using Steamworks;

namespace Dummy.Commands
{
    [Command("remove")]
    [CommandAlias("kick")]
    [CommandAlias("delete")]
    [CommandDescription("Removes a dummy by id")]
    [CommandParent(typeof(CommandDummy))]
    [CommandSyntax("<id>")]
    [UsedImplicitly]
    public class CommandDummyRemove : Command
    {
        private readonly IDummyProvider m_DummyProvider;
        private readonly IStringLocalizer m_StringLocalizer;

        public CommandDummyRemove(IServiceProvider serviceProvider, IDummyProvider dummyProvider,
            IStringLocalizer stringLocalizer) : base(serviceProvider)
        {
            m_DummyProvider = dummyProvider;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            if (Context.Parameters.Count == 0)
            {
                throw new CommandWrongUsageException(Context);
            }

            var id = (CSteamID)await Context.Parameters.GetAsync<ulong>(0);

            if (await m_DummyProvider.RemoveDummyAsync(id))
            {
                await PrintAsync(m_StringLocalizer["commands:general:remove:success", new { Id = id }]);
                return;
            }

            await PrintAsync(m_StringLocalizer["commands:general:remove:fail", new { Id = id }]);
        }
    }
}