using EvolutionPlugins.Dummy.API;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using SDG.Unturned;
using Steamworks;
using System;
using System.Threading.Tasks;
using Command = OpenMod.Core.Commands.Command;

namespace EvolutionPlugins.Dummy.Commands
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
            if (!m_DummyProvider.Dummies.TryGetValue(id, out _))
            {
                throw new UserFriendlyException($"Dummy \"{id}\" has not found!");
            }

            var dummy = PlayerTool.getPlayer(id); // https://github.com/openmod/openmod/pull/109
            if (dummy == null)
            {
                throw new UserFriendlyException($"Dummy \"{id}\" has not found!");
            }

            var faceId = await Context.Parameters.GetAsync<byte>(1);
            if (faceId > Customization.FACES_FREE + Customization.FACES_PRO)
            {
                throw new UserFriendlyException($"Can't change to {faceId} because is higher {Customization.FACES_FREE + Customization.FACES_PRO}");
            }

            dummy.clothing.channel.send("tellSwapFace", ESteamCall.NOT_OWNER, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
            {
                faceId
            });
        }
    }
}
