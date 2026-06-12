using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

// ReSharper disable once CheckNamespace - Avoids namespace/classname duplication weirdness
namespace EPR.Calculator.API.Data.DataTypes;

/// <summary>
///     Converter for Entity Framework that stores an int representation of RelativeYear.
/// </summary>
internal sealed class RelativeYearValueConverter()
    : ValueConverter<RelativeYear, int>(
        relativeYear => relativeYear.Value,
        i => new RelativeYear(i));
