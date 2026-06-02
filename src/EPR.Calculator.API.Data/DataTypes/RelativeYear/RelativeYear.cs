using System.Text.Json.Serialization;

// ReSharper disable once CheckNamespace - Avoids namespace/classname duplication weirdness
namespace EPR.Calculator.API.Data.DataTypes;

[JsonConverter(typeof(RelativeYearJsonConverter))]
public readonly record struct RelativeYear(int Value)
{
    public override string ToString() => Value.ToString();
    public string ToFinancialYear() => $"{Value}-{(Value + 1) % 100:D2}";

    public static implicit operator int(RelativeYear relativeYear) => relativeYear.Value;
    public static explicit operator RelativeYear(int value) => new(value);
}
