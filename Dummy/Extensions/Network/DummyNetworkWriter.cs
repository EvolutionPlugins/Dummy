using System.IO;

namespace EvolutionPlugins.Dummy.Extensions.Network
{
    public class DummyNetworkWriter : StreamWriter
    {
        public DummyNetworkWriter(PlayerDummy dummy) : base(new DummyNetworkStream(dummy))
        {
        }
        
        public DummyNetworkWriter(DummyNetworkStream stream) : base(stream)
        {
            
        }
        
    }
}