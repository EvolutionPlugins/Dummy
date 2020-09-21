using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy.API;
using EvolutionPlugins.Dummy.Models.Users;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using System;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Commands
{
    public abstract class CommandDummyAction : Command
    {
        private readonly IDummyProvider m_DummyProvider;

        protected CommandDummyAction(IServiceProvider serviceProvider, IDummyProvider dummyProvider) : base(serviceProvider)
        {
            m_DummyProvider = dummyProvider;
        }

        protected override async Task OnExecuteAsync()
        {
            if (Context.Parameters.Count == 0)
            {
                throw new CommandWrongUsageException(Context);
            }
            var id = await Context.Parameters.GetAsync<ulong>(0);

            var dummy = await m_DummyProvider.GetPlayerDummyAsync(id);
            if (dummy == null)
            {
                throw new UserFriendlyException($"Dummy \"{id}\" has not found!");
            }

            await ExecuteDummyAsync(dummy);
        }

        protected abstract UniTask ExecuteDummyAsync(DummyUser playerDummy);
    }
}
