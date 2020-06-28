using Rocket.API;

namespace Dummy.Configurations
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
