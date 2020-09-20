using OpenMod.Unturned.Users;
using Steamworks;
using System.Collections.Generic;

namespace EvolutionPlugins.Dummy.Models
{
    public class PlayerDummyData
    {
        public PlayerDummyData(HashSet<CSteamID> owners, UnturnedUser unturnedUser)
        {
            Owners = owners;
            UnturnedUser = unturnedUser;
        }

        public HashSet<CSteamID> Owners { get; }
        public UnturnedUser UnturnedUser { get; }
    }
}
