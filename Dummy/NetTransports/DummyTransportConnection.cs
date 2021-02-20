using Microsoft.Extensions.Configuration;
using OpenMod.API.Plugins;
using SDG.NetTransport;
using SDG.Unturned;
using System.Net;

namespace Dummy.NetTransports
{
    public class DummyTransportConnection : ITransportConnection
    {
        private readonly IPluginAccessor<Dummy> m_PluginAccessor;

        private IConfiguration m_Configuration => m_PluginAccessor.Instance.Configuration;

        public DummyTransportConnection(IPluginAccessor<Dummy> pluginAccessor)
        {
            m_PluginAccessor = pluginAccessor;
        }

        public void CloseConnection()
        {
        }

        public bool Equals(ITransportConnection other)
        {
            return this == other;
        }

        public bool TryGetIPv4Address(out uint address)
        {
            address = Parser.getUInt32FromIP(m_Configuration["default:ip"]);
            return true;
        }

        public bool TryGetPort(out ushort port)
        {
            port = m_Configuration.GetSection("default:port").Get<ushort>();
            return true;
        }

        public IPAddress GetAddress()
        {
            return IPAddress.Parse(m_Configuration["default:ip"] + ":" + m_Configuration["default:port"]);
        }

        public string GetAddressString(bool withPort)
        {
            return m_Configuration["default:ip"] + (withPort ? (":" + m_Configuration["default:port"]) : string.Empty);
        }

        public void Send(byte[] buffer, long size, ENetReliability sendType)
        {
        }
    }
}
