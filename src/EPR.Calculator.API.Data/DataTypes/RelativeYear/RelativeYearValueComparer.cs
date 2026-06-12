using Microsoft.EntityFrameworkCore.ChangeTracking;

// ReSharper disable once CheckNamespace - Avoids namespace/classname duplication weirdness
namespace EPR.Calculator.API.Data.DataTypes;

/// <summary>
///     A value comparer for RelativeYear to enable proper equality comparison and hash code generation for Entity
///     Framework's change tracking and database operations.
/// </summary>
internal sealed class RelativeYearValueComparer()
    : ValueComparer<RelativeYear>(
        (relativeYear, other) => relativeYear == other,
        relativeYear => relativeYear.GetHashCode());
