using System.Text.Json;
using System.Text.Json.Serialization;

// ReSharper disable once CheckNamespace - Avoids namespace/classname duplication weirdness
namespace EPR.Calculator.API.Data.DataTypes;

internal class RelativeYearJsonConverter : JsonConverter<RelativeYear>
{
    public override RelativeYear Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
            throw new JsonException("RelativeYear must be an integer");

        return (RelativeYear) reader.GetInt32();
    }

    public override void Write(Utf8JsonWriter writer, RelativeYear relativeYear, JsonSerializerOptions options) =>
        writer.WriteNumberValue(relativeYear);
}
