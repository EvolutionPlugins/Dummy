using SDG.NetTransport;

namespace Dummy.NetTransports
{
    internal class NullTransportConnection : ITransportConnection
    {
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
    }
}
