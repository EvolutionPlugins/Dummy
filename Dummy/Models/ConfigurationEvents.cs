extern alias JetBrainsAnnotations;
using JetBrainsAnnotations::JetBrains.Annotations;

namespace Dummy.Models
{
    [UsedImplicitly]
    public class ConfigurationEvents
    {
        public bool AllowDamage { get; set; }
        public bool CallOnCheckValidWithExplanation { get; set; }
        public bool CallOnCheckBanStatusWithHwid { get; set; }
    }
}