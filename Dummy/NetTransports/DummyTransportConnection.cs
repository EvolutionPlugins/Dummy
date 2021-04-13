using System.Net;
using Microsoft.Extensions.Configuration;
using OpenMod.API.Plugins;
using SDG.NetTransport;
using SDG.Unturned;

namespace Dummy.NetTransports
{
    public class DummyTransportConnection : ITransportConnection
    {
        private readonly IPluginAccessor<Dummy> m_PluginAccessor;

        private IConfiguration Configuration => m_PluginAccessor.Instance!.Configuration;

        public DummyTransportConnection(IPluginAccessor<Dummy> pluginAccessor)
        {
            m_PluginAccessor = pluginAccessor;
        }

        public void CloseConnection()
        {
        }

        public bool Equals(ITransportConnection other)
        {
            return Equals(this, other);
        }

        public bool TryGetIPv4Address(out uint address)
        {
            address = Parser.getUInt32FromIP(Configuration["default:ip"]);
            return true;
        }

        public bool TryGetPort(out ushort port)
        {
            port = Configuration.GetSection("default:port").Get<ushort>();
            return true;
        }

        public IPAddress GetAddress()
        {
            return IPAddress.Parse(Configuration["default:ip"] + ":" + Configuration["default:port"]);
        }

        public string GetAddressString(bool withPort)
        {
            return Configuration["default:ip"] + (withPort ? (":" + Configuration["default:port"]) : string.Empty);
        }

        public void Send(byte[] buffer, long size, ENetReliability sendType)
        {
        }
    }
}