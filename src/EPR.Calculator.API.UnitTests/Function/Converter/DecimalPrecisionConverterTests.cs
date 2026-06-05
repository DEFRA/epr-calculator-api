using System.Text;
using System.Text.Json;
using EPR.Calculator.Service.Function.Converter;

namespace EPR.Calculator.Service.Function.UnitTests.Converter
{
    [TestClass]
    public class DecimalPrecisionConverterTests
    {
        [TestMethod]
        public void WriteJson_RoundDemcimal()
        {
            // Act
            var converter = new DecimalPrecisionConverter(3);
            var val = 123.9879908m;
            var stream = new MemoryStream();
            var jsonWriter = new Utf8JsonWriter(stream);

            converter.Write(jsonWriter, val, new JsonSerializerOptions());
            jsonWriter.Flush();
            stream.Position = 0;
            var result = new StreamReader(stream).ReadToEnd();

            // Assert
            Assert.AreEqual("123.988",result);
        }

        [TestMethod]
        public void ReadJson_RoundDemcimal()
        {
            // Act
            var converter = new DecimalPrecisionConverter(3);
            var json = "\"123.988\"";
            var bytes = Encoding.UTF8.GetBytes(json);
            var jsonReader = new Utf8JsonReader(bytes);

            jsonReader.Read();
            var result = converter.Read(ref jsonReader, typeof(decimal), new JsonSerializerOptions());

            // Assert
            Assert.AreEqual(123.988m, result);
        }
       
        [TestMethod]
        public void CanConvertReturnsFalse()
        {
            // Act
            var converter = new DecimalPrecisionConverter(3);

            Assert.IsFalse(converter.CanConvert(typeof(string)));
        }
    }
}