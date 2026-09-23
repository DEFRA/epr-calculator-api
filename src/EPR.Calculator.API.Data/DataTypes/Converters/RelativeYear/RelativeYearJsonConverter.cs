using System.Text.Json;
using System.Text.Json.Serialization;

namespace EPR.Calculator.API.Data.DataTypes.Converters.RelativeYear;

internal class RelativeYearJsonConverter : JsonConverter<DataTypes.RelativeYear>
{
    public override DataTypes.RelativeYear Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
            throw new JsonException("RelativeYear must be an integer");

        return (DataTypes.RelativeYear) reader.GetInt32();
    }

    public override void Write(Utf8JsonWriter writer, DataTypes.RelativeYear relativeYear, JsonSerializerOptions options) =>
        writer.WriteNumberValue(relativeYear);
}
