using SDG.Unturned;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace Dummy
{
    public struct DummyData
    {
        public List<CSteamID> Owners;
        public Coroutine Coroutine;

        public Player player;

        public byte analog;
        public uint count;
        public float tick;
        public uint buffer;
        public uint simulation;
        public int sequence;
        public int recov;
        public uint consumed;
        public uint clock;
        public ushort[] flags;
        public float yaw;
        public float pitch;

        public List<PlayerInputPacket> playerInputPackets;

        public DummyData(List<CSteamID> owners, Coroutine coroutine)
        {
            Owners = owners;
            Coroutine = coroutine;

            analog = 0;
            count = 0;
            tick = Time.realtimeSinceStartup;
            buffer = 0;
            simulation = 0;
            sequence = -1;
            recov = -1;
            consumed = 0;
            clock = 0;
            yaw = 0;
            pitch = 0;

            playerInputPackets = new List<PlayerInputPacket>();

            player = null;

            flags = new ushort[(9 + ControlsSettings.NUM_PLUGIN_KEYS)];
            byte b = 0;
            while (b < (9 + ControlsSettings.NUM_PLUGIN_KEYS))
            {
                flags[b] = (ushort)(1 << b);
                b += 1;
            }
        }


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
