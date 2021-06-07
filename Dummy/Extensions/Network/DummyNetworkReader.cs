using System.Collections.Generic;
using System.IO;
using EvolutionPlugins.Dummy.Extensions.Network.Patches;
using SDG.Unturned;

namespace EvolutionPlugins.Dummy.Extensions.Network
{
    public class DummyNetworkReader : StreamReader
    {
        
        
        public DummyNetworkReader(PlayerDummy dummy) : base(new DummyNetworkStream(dummy))
        {
        }
        
        public DummyNetworkReader(DummyNetworkStream stream) : base(stream)
        {
            
        }

        


       
    }
}