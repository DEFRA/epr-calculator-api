using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EPR.Calculator.API.Data.DataTypes.Enums;

/// <summary>
///     Enum converter for Entity Framework that stores a string representation of the enum instead of numeric.
/// </summary>
/// <typeparam name="TEnum">
///     The enum type to convert. Ideally, it should have <c>Unknown = 0</c> declared.
/// </typeparam>
/// <remarks>
///     The read conversion is case-insensitive trim. If the string value is invalid, this will resolve as <c>Unknown</c>
///     if present in the enum, or the default <c>0</c> value if not.
/// </remarks>
internal sealed class StringEnumConverter<TEnum>()
    : ValueConverter<TEnum, string>(
        enm => enm.ToString(),
        str => SafeParse(str))
    where TEnum : struct, Enum
{
    private static TEnum SafeParse(string? value)
    {
        if (Enum.TryParse(value?.Trim(), true, out TEnum enumValue))
            return enumValue;

        // Failed to parse; so try to return "Unknown" value

        // TryParse in case "Unknown" does not have the underlying value of 0
        Enum.TryParse("Unknown", true, out enumValue);

        // TEnum does not have an "Unknown" value
        // Return whatever the default (0) value is
        return enumValue;
    }
}
