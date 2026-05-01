using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EPR.Calculator.API.Data.Converters;

/// <summary>
///     DateTime converter for Entity Framework that ensures UTC time is used for storage and retrieval.
/// </summary>
/// <remarks>
///     Default EF behaviour when reading from the DB is to interpret DateTime values as DateTimeKind.Unspecified, this
///     ensures the correct UTC kind is used instead.
/// </remarks>
internal sealed class UtcDateTimeConverter()
    : ValueConverter<DateTime?, DateTime?>(ToProvider, FromProvider)
{
    private static Expression<Func<DateTime?, DateTime?>> ToProvider =>
        dt => dt.HasValue ? dt.Value.ToUniversalTime() : null;

    private static Expression<Func<DateTime?, DateTime?>> FromProvider =>
        dt => dt.HasValue ? DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc) : null;
}
