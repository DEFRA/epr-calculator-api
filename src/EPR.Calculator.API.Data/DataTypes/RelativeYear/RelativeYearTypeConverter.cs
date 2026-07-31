using System.ComponentModel;
using System.Globalization;

// ReSharper disable once CheckNamespace - Avoids namespace/classname duplication weirdness
namespace EPR.Calculator.API.Data.DataTypes;

internal sealed class RelativeYearTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object value)
    {
        if (value is string s && int.TryParse(s, out var year))
        {
            return new RelativeYear(year);
        }

        return base.ConvertFrom(context, culture, value);
    }
}
