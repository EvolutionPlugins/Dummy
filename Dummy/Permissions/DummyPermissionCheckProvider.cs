using Dummy.Users;
using Microsoft.Extensions.Configuration;
using OpenMod.API.Prioritization;
using OpenMod.Core.Permissions;

namespace Dummy.Permissions
{
    [Priority(Priority = Priority.High)]
    public class DummyPermissionCheckProvider : AlwaysGrantPermissionCheckProvider
    {
        public DummyPermissionCheckProvider(IConfiguration configuration) : base(actor => actor is DummyUser dummy
        && (dummy.SteamPlayer.isAdmin
        || configuration.GetSection("options:canExecuteCommands").Get<bool>()
        || configuration.GetSection("options:isAdmin").Get<bool>()))
        {
        }
    }
}
