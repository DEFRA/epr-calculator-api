using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EPR.Calculator.API.Data.DataTypes.Converters.Enums;

internal sealed class IntEnumConverter<TEnum> () : ValueConverter<TEnum, int>(
    value => Convert.ToInt32(value),
    value => (TEnum)Enum.ToObject(typeof(TEnum), value))
    where TEnum : struct, Enum;
