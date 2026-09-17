using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EPR.Calculator.API.Data.DataTypes.Converters.Enums;

/// <summary>
///     Enum converter for Entity Framework that stores an int representation of the enum.
/// </summary>
internal sealed class IntEnumConverter<TEnum>()
    : ValueConverter<TEnum, int>(
        value => Convert.ToInt32(value),
        value => (TEnum)Enum.ToObject(typeof(TEnum), value))
    where TEnum : struct, Enum;
