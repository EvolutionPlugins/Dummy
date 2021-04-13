extern alias JetBrainsAnnotations;
using JetBrainsAnnotations::JetBrains.Annotations;

namespace Dummy.Models
{
    [UsedImplicitly]
    public class ConfigurationFun
    {
        public bool AlwaysRotate { get; set; }
        public float RotateYaw { get; set; }
    }
}
