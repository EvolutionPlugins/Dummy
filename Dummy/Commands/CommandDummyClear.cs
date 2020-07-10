using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy.API;
using OpenMod.Core.Commands;
using SDG.Unturned;
using System;
using System.Threading.Tasks;
using Command = OpenMod.Core.Commands.Command;

namespace EvolutionPlugins.Dummy.Commands
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

        protected override async Task OnExecuteAsync()
        {
            await UniTask.SwitchToMainThread();

            foreach (var cSteamID in m_DummyProvider.Dummies.Keys)
            {
                Provider.kick(cSteamID, "");
            }
            await m_DummyProvider.ClearAllDummies();
        }
    }
}
