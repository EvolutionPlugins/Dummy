extern alias JetBrainsAnnotations;
using System;
using System.Reflection;
using HarmonyLib;
using SDG.NetPak;
using SDG.Unturned;

namespace Dummy.Extensions
{
    public static class ServerInvocationContextExtenison
    {
        private static readonly ConstructorInfo s_ServerInvocationContextConstructor =
            AccessTools.Constructor(typeof(ServerInvocationContext),
                new[]
                {
                    typeof(ServerInvocationContext.EOrigin), typeof(SteamPlayer), typeof(NetPakReader),
                    typeof(ServerMethodInfo)
                });

        public static ServerInvocationContext GetContext(SteamPlayer steamPlayer)
        {
            if (steamPlayer is null)
            {
                throw new ArgumentNullException(nameof(steamPlayer));
            }

            // also can be used Activator.CreateInstance
            var context = (ServerInvocationContext)s_ServerInvocationContextConstructor.Invoke(new object[]
            {
                ServerInvocationContext.EOrigin.Obsolete, steamPlayer, null!, null!
            });
            return context;
        }
    }
}