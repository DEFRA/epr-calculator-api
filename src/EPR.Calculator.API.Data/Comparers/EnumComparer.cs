using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EPR.Calculator.API.Data.Comparers;

/// <summary>
///     A value comparer for enum types to enable proper equality comparison and hash code generation
///     for Entity Framework's change tracking and database operations.
/// </summary>
/// <typeparam name="TEnum">
///     The enum type to be compared.
/// </typeparam>
internal sealed class EnumComparer<TEnum>()
    : ValueComparer<TEnum>(
        (enm1, enm2) => enm1.Equals(enm2),
        enm => enm.GetHashCode())
    where TEnum : struct, Enum;
