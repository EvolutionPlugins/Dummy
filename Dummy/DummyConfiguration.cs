using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dummy
{
    public class DummyConfiguration : IRocketPluginConfiguration
    {
        public byte AmountDummiesInSameTime;
        public ulong KickDummyAfterSeconds; // 0 - not kicking
        public void LoadDefaults()
        {
            AmountDummiesInSameTime = 1;
            KickDummyAfterSeconds = 60 * 5;
        }
    }
}
