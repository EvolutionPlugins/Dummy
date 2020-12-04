using Dummy.Users;
using OpenMod.Unturned.Players;
using SDG.Unturned;
using System;

namespace Dummy.Players
{
    public class DummyPlayer : UnturnedPlayer, IEquatable<DummyUser>
    {
        public DummyPlayer(SteamPlayer steamPlayer) : base(steamPlayer.player)
        {
        }

        public bool Equals(DummyUser other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return other.SteamID.Equals(SteamId);
        }

        public override bool Equals(object obj)
        {
            if (obj is DummyPlayer other) return Equals(other);
            else return false;
        }

        public override int GetHashCode()
        {
            return -975366258 + SteamId.GetHashCode();
        }
    }
}
