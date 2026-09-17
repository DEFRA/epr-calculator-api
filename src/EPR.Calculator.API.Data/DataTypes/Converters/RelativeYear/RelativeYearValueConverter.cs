using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EPR.Calculator.API.Data.DataTypes.Converters.RelativeYear;

/// <summary>
///     Converter for Entity Framework that stores an int representation of RelativeYear.
/// </summary>
internal sealed class RelativeYearValueConverter()
    : ValueConverter<DataTypes.RelativeYear, int>(
        relativeYear => relativeYear.Value,
        i => new DataTypes.RelativeYear(i));
