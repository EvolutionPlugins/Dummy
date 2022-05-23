using System;

namespace Dummy.Extensions
{
    public static class ConvertorExtension
    {
        public static byte[] GetBytes(this string? data)
        {
            if (data is null)
            {
                return Array.Empty<byte>();
            }

            var length = (data.Length + 1) / 3;
            var array = new byte[length];
            for (var i = 0; i < length; i++)
            {
                array[i] = Convert.ToByte(data.Substring(3 * i, 2), 16);
            }
            return array;
        }
    }
}
