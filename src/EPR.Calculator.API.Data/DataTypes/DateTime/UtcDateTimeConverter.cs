using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EPR.Calculator.API.Data.DataTypes.DateTime;

/// <summary>
///     DateTime converter for Entity Framework that ensures UTC time is used for storage and retrieval.
/// </summary>
/// <remarks>
///     Default EF behaviour when reading from the DB is to interpret DateTime values as DateTimeKind.Unspecified, this
///     ensures the correct UTC kind is used instead.
/// </remarks>
internal sealed class UtcDateTimeConverter()
    : ValueConverter<System.DateTime?, System.DateTime?>(ToProvider, FromProvider)
{
    private static Expression<Func<System.DateTime?, System.DateTime?>> ToProvider =>
        dt => dt.HasValue ? dt.Value.ToUniversalTime() : null;

    private static Expression<Func<System.DateTime?, System.DateTime?>> FromProvider =>
        dt => dt.HasValue ? System.DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc) : null;
}
