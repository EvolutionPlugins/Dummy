using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Core.Ioc;

namespace Dummy.Commands
{
    [DontAutoRegister]
    public abstract class CommandDummyAction : Command
    {
        private readonly IDummyProvider m_DummyProvider;
        private readonly IStringLocalizer m_StringLocalizer;

        protected CommandDummyAction(IServiceProvider serviceProvider, IDummyProvider dummyProvider,
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

            var id = await Context.Parameters.GetAsync<ulong>(0);

            var dummy = await m_DummyProvider.FindDummyUserAsync(id);
            if (dummy == null)
            {
                throw new UserFriendlyException(m_StringLocalizer["commands:dummyNotFound", new { Id = id }]);
            }

            await ExecuteDummyAsync(dummy);
        }

        protected abstract UniTask ExecuteDummyAsync(DummyUser playerDummy);
    }
}