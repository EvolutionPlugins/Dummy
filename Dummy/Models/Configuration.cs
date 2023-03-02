extern alias JetBrainsAnnotations;
using JetBrainsAnnotations::JetBrains.Annotations;

namespace Dummy.Models
{
    [UsedImplicitly]
    public class Configuration
    {
        public ConfigurationOptions Options { get; set; } = new();
        public ConfigurationEvents Events { get; set; } = new();
        public ConfigurationConnection Connection { get; set; } = new();
        public ConfigurationFun Fun { get; set; } = new();
        public ConfigurationSettings Default { get; set; } = new();
    }
}
