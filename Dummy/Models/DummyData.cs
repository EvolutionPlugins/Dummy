using OpenMod.Unturned.Users;
using Steamworks;
using System.Collections.Generic;

namespace EvolutionPlugins.Dummy.Models
{
    public struct DummyData
    {
        public DummyData(List<CSteamID> owners, UnturnedUser unturnedUser)
        {
            Owners = owners;
            UnturnedUser = unturnedUser;
        }

        public List<CSteamID> Owners { get; }
        public UnturnedUser UnturnedUser { get; }

        public override bool Equals(object obj)
        {
            return obj is DummyData data && data.Owners == Owners && UnturnedUser == data.UnturnedUser;
        }

        public override int GetHashCode()
        {
            return 1599248077 + EqualityComparer<List<CSteamID>>.Default.GetHashCode(Owners);
        }

        public static bool operator ==(DummyData left, DummyData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DummyData left, DummyData right)
        {
            return !(left == right);
        }
    }
}
