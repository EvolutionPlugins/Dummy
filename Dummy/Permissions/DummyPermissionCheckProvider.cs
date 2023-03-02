extern alias JetBrainsAnnotations;
using Dummy.Models;
using Dummy.Users;
using JetBrainsAnnotations::JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using OpenMod.API.Plugins;
using OpenMod.API.Prioritization;
using OpenMod.Core.Permissions;

namespace Dummy.Permissions
{
    [Priority(Priority = Priority.High)]
    [UsedImplicitly]
    public class DummyPermissionCheckProvider : AlwaysGrantPermissionCheckProvider
    {
        public DummyPermissionCheckProvider(IPluginAccessor<Dummy> pluginAccessor) : base((actor) =>
        {
            if (actor is not DummyUser user)
            {
                return false;
            }

            if (user.SteamPlayer.isAdmin)
            {
                return true;
            }

            var options = pluginAccessor.Instance!.Configuration.GetValue<ConfigurationOptions>("options");
            return options.CanExecuteCommands || options.IsAdmin;
        })
        {
        }
    }
}