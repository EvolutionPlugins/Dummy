using SDG.NetTransport;
using System.Net;

namespace Dummy.NetTransports
{
    public sealed class NullTransportConnection : ITransportConnection
    {
        private static NullTransportConnection m_Instance;
        public static NullTransportConnection Instance => m_Instance ??= new();

        private NullTransportConnection()
        {
        }

        public void CloseConnection()
        {
        }

        public bool Equals(ITransportConnection other)
        {
            return this == other;
        }

        public void Send(byte[] buffer, long size, ESendType sendType)
        {
        }

        public bool TryGetIPv4Address(out uint address)
        {
            // todo: add default IP
            address = 0;
            return false;
        }

        public bool TryGetPort(out ushort port)
        {
            port = 0;
            return false;
        }

        public IPAddress GetAddress()
        {
            return new IPAddress(0);
        }

        public string GetAddressString(bool withPort)
        {
            return null;
        }
    }
}
