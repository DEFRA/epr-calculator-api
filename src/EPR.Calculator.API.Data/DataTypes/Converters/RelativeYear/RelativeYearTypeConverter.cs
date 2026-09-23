using System.ComponentModel;
using System.Globalization;

namespace EPR.Calculator.API.Data.DataTypes.Converters.RelativeYear;

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
            return new DataTypes.RelativeYear(year);
        }

        return base.ConvertFrom(context, culture, value);
    }
}
