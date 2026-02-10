using System.Diagnostics.CodeAnalysis;
using NewtonsoftJson = Newtonsoft.Json;
using STJ = System.Text.Json;
using STJSerialization = System.Text.Json.Serialization;

namespace EPR.Calculator.API.Data.Models
{
    [ExcludeFromCodeCoverage]
    [STJSerialization.JsonConverter(typeof(RelativeYearSystemTextJsonConverter))]
    [NewtonsoftJson.JsonConverter(typeof(RelativeYearNewtonsoftJsonConverter))]
    public sealed record RelativeYear(int Value)
    {
        public override string ToString() => this.Value.ToString();

        public string ToFinancialYear() =>
            $"{this.Value}-{(this.Value + 1) % 100:D2}";
    }

    [ExcludeFromCodeCoverage]
    public class RelativeYearSystemTextJsonConverter : STJSerialization.JsonConverter<RelativeYear>
    {
        public override RelativeYear Read(ref STJ.Utf8JsonReader reader, Type typeToConvert, STJ.JsonSerializerOptions options)
        {
            if (reader.TokenType != STJ.JsonTokenType.Number)
            {
                throw new STJ.JsonException("RelativeYear must be an integer");
            }

            return new RelativeYear(reader.GetInt32());
        }

        public override void Write(STJ.Utf8JsonWriter writer, RelativeYear value, STJ.JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.Value);
        }
    }

    [ExcludeFromCodeCoverage]
    public class RelativeYearNewtonsoftJsonConverter : NewtonsoftJson.JsonConverter<RelativeYear>
    {
        public override RelativeYear? ReadJson(NewtonsoftJson.JsonReader reader, Type objectType, RelativeYear? existingValue, bool hasExistingValue, NewtonsoftJson.JsonSerializer serializer)
        {
            if (reader.TokenType != NewtonsoftJson.JsonToken.Integer)
            {
                throw new NewtonsoftJson.JsonSerializationException("RelativeYear must be an integer");
            }

            return new RelativeYear(Convert.ToInt32(reader.Value));
        }

        public override void WriteJson(NewtonsoftJson.JsonWriter writer, RelativeYear? value, NewtonsoftJson.JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue(value.Value);
        }
    }
}
