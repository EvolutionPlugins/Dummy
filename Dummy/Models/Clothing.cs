extern alias JetBrainsAnnotations;
using JetBrainsAnnotations::JetBrains.Annotations;

namespace Dummy.Models
{
    [UsedImplicitly]
    public class Clothing
    {
        public ushort Shirt { get; set; }
        public ushort Pants { get; set; }
        public ushort Hat { get; set; }
        public ushort Backpack { get; set; }
        public ushort Vest { get; set; }
        public ushort Mask { get; set; }
        public ushort Glasses { get; set; }
    }
}
