using System.ComponentModel;

namespace EPR.Calculator.Service.Function.Misc
{
    public static class TypeConverterUtil
    {
        public static T? ConvertTo<T>(object? value)
        {
            var targetType = typeof(T);

            if(value == null || value == DBNull.Value)
            {
                if (targetType.IsValueType)
                    return (T?)Activator.CreateInstance(targetType);
                return default;
            }

            if (value is T variable) return variable;

            if (targetType.IsInstanceOfType(value))
                return (T)value;

            try
            {
                return (T)Convert.ChangeType(value, targetType);
            }
            catch (Exception)
            {
                var converter =  TypeDescriptor.GetConverter(targetType);
                if (converter != null && converter.CanConvertFrom(value.GetType()))
                {
                    return (T?)converter.ConvertFrom(value);
                }

                return default(T);
            }
        }
    }
}
