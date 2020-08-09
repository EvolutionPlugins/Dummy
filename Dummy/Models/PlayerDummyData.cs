using OpenMod.Unturned.Users;
using Steamworks;
using System.Collections.Generic;

namespace EvolutionPlugins.Dummy.Models
{
    public class PlayerDummyData
    {
        public PlayerDummyData(List<CSteamID> owners, UnturnedUser unturnedUser)
        {
            Owners = owners;
            UnturnedUser = unturnedUser;
        }

        public List<CSteamID> Owners { get; }
        public UnturnedUser UnturnedUser { get; }

        public override bool Equals(object obj)
        {
            return obj is PlayerDummyData data && data.Owners == Owners && UnturnedUser == data.UnturnedUser;
        }

        public override int GetHashCode()
        {
            return 1599248077 + EqualityComparer<List<CSteamID>>.Default.GetHashCode(Owners);
        }

        public static bool operator ==(PlayerDummyData left, PlayerDummyData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlayerDummyData left, PlayerDummyData right)
        {
            return !(left == right);
        }
    }
}
