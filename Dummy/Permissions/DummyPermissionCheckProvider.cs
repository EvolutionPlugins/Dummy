using Dummy.Users;
using Microsoft.Extensions.Configuration;
using OpenMod.API.Plugins;
using OpenMod.API.Prioritization;
using OpenMod.Core.Permissions;

namespace Dummy.Permissions
{
    [Priority(Priority = Priority.High)]
    public class DummyPermissionCheckProvider : AlwaysGrantPermissionCheckProvider
    {
        public DummyPermissionCheckProvider(IPluginAccessor<Dummy> pluginAccessor) : base(actor => actor is DummyUser dummy
        && (pluginAccessor.Instance.Configuration.GetSection("options:canExecuteCommands").Get<bool>()
        || pluginAccessor.Instance.Configuration.GetSection("options:isAdmin").Get<bool>()
        || dummy.SteamPlayer.isAdmin))
        {
        }
    }
}
