using System;
using System.ComponentModel;
using System.Globalization;
using System.Net;

namespace Dummy.ConfigurationEx;
internal class IPAddressTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        return value is string address ? IPAddress.Parse(address) : base.ConvertFrom(context, culture, value);
    }
}
