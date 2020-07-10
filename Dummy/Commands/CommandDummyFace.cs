using EvolutionPlugins.Dummy.API;
using OpenMod.API.Commands;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Core.Users;
using OpenMod.Unturned.Users;
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
        private readonly IUserManager m_UserManager;

        public CommandDummyFace(IServiceProvider serviceProvider, IDummyProvider dummyProvider, IUserManager userManager) : base(serviceProvider)
        {
            m_DummyProvider = dummyProvider;
            m_UserManager = userManager;
        }

        protected override async Task OnExecuteAsync()
        {
            if (Context.Parameters.Count == 0)
            {
                throw new CommandWrongUsageException(Context);
            }

            var id = await Context.Parameters.GetAsync<CSteamID>(0);

            if (!m_DummyProvider.Dummies.TryGetValue(id, out _))
            {
                throw new UserFriendlyException($"Dummy \"{id}\" has not found!");
            }

            var dummy = (UnturnedUser)await m_UserManager.FindUserAsync(KnownActorTypes.Player, id.ToString(), UserSearchMode.Id);

            var faceId = await Context.Parameters.GetAsync<byte>(1);

            if (faceId > Customization.FACES_FREE + Customization.FACES_PRO)
            {
                throw new UserFriendlyException($"Can't change to {faceId} because is higher {Customization.FACES_FREE + Customization.FACES_PRO}");
            }

            dummy.Player.clothing.channel.send("tellSwapFace", ESteamCall.NOT_OWNER, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
            {
                faceId
            });
        }
    }
}
