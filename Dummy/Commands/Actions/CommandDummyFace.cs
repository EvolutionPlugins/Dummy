using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy.API;
using EvolutionPlugins.Dummy.Extensions.Interaction.Actions;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using SDG.Unturned;
using Steamworks;
using System;
using System.Threading.Tasks;
using Command = OpenMod.Core.Commands.Command;

namespace EvolutionPlugins.Dummy.Commands.Actions
{
    [Command("face")]
    [CommandDescription("Send a face to dummy")]
    [CommandParent(typeof(CommandDummy))]
    [CommandSyntax("<id> <face>")]
    public class CommandDummyFace : Command
    {
        private readonly IDummyProvider m_DummyProvider;

        public CommandDummyFace(IServiceProvider serviceProvider, IDummyProvider dummyProvider) : base(serviceProvider)
        {
            m_DummyProvider = dummyProvider;
        }

        protected override async Task OnExecuteAsync()
        {
            if (Context.Parameters.Count == 0)
            {
                throw new CommandWrongUsageException(Context);
            }

            var id = (CSteamID)await Context.Parameters.GetAsync<ulong>(0);

            var dummy = await m_DummyProvider.GetPlayerDummy(id.m_SteamID);
            if (dummy == null)
            {
                throw new UserFriendlyException($"Dummy \"{id}\" has not found!");
            }

            var faceId = await Context.Parameters.GetAsync<byte>(1);
            if (faceId > Customization.FACES_FREE + Customization.FACES_PRO)
            {
                throw new UserFriendlyException($"Can't change to {faceId} because is higher {Customization.FACES_FREE + Customization.FACES_PRO}");
            }
            dummy.Actions.Actions.Enqueue(new FaceAction(faceId));
        }
    }
}
