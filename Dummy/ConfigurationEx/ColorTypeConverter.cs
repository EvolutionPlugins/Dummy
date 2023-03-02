using System;
using System.ComponentModel;
using System.Globalization;
using UnityEngine;

namespace Dummy.ConfigurationEx;

internal class ColorTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        return value is string str && ColorUtility.TryParseHtmlString(str, out var color)
            ? color
            : base.ConvertFrom(context, culture, value);
    }
}
