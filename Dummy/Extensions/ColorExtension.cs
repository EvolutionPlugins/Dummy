using OpenMod.UnityEngine.Extensions;
using System.Drawing;
using UColor = UnityEngine.Color;

namespace Dummy.Extensions
{
    public static class ColorExtension
    {
        public static UColor ToColor(this string color)
        {
            var sColor = ColorTranslator.FromHtml(color);
            return sColor.IsEmpty ? UColor.white : sColor.ToUnityColor();
        }
    }
}
