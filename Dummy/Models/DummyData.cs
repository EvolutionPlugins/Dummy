using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace EvolutionPlugins.Dummy.Models
{
    public struct DummyData
    {
        public List<CSteamID> Owners;

        public override bool Equals(object obj)
        {
            return obj is DummyData data && data.Owners == Owners && data.Coroutine == Coroutine;
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
