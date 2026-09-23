using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EPR.Calculator.API.Data.DataTypes.Converters.RelativeYear;

/// <summary>
///     A value comparer for RelativeYear to enable proper equality comparison and hash code generation for Entity
///     Framework's change tracking and database operations.
/// </summary>
internal sealed class RelativeYearValueComparer()
    : ValueComparer<DataTypes.RelativeYear>(
        (relativeYear, other) => relativeYear == other,
        relativeYear => relativeYear.GetHashCode());
