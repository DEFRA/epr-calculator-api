using System.Diagnostics.CodeAnalysis;
using STJ = System.Text.Json;
using STJSerialization = System.Text.Json.Serialization;

namespace EPR.Calculator.API.Data.Models;

[ExcludeFromCodeCoverage]
[STJSerialization.JsonConverter(typeof(RelativeYearSystemTextJsonConverter))]
public sealed record RelativeYear(int Value)
{
    public override string ToString() => Value.ToString();

    public string ToFinancialYear() =>
        $"{Value}-{(Value + 1) % 100:D2}";
}

[ExcludeFromCodeCoverage]
public class RelativeYearSystemTextJsonConverter : STJSerialization.JsonConverter<RelativeYear>
{
    public override RelativeYear Read(ref STJ.Utf8JsonReader reader, Type typeToConvert, STJ.JsonSerializerOptions options)
    {
        if (reader.TokenType != STJ.JsonTokenType.Number)
            throw new STJ.JsonException("RelativeYear must be an integer");

        return new RelativeYear(reader.GetInt32());
    }

    public override void Write(STJ.Utf8JsonWriter writer, RelativeYear value, STJ.JsonSerializerOptions options) => writer.WriteNumberValue(value.Value);
}
