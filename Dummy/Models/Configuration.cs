extern alias JetBrainsAnnotations;
using JetBrainsAnnotations::JetBrains.Annotations;

namespace Dummy.Models
{
    [UsedImplicitly]
    public class Configuration
    {
        public ConfigurationOptions? Options { get; set; }
        public ConfigurationEvents? Events { get; set; }
        public ConfigurationConnection? Connection { get; set; }
        public ConfigurationFun? Fun { get; set; }
        public ConfigurationSettings? Default { get; set; }
    }
}
