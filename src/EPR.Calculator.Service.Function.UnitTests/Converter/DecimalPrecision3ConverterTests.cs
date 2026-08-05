using System.Text.Json;
using EPR.Calculator.Service.Function.Converter;

namespace EPR.Calculator.Service.Function.UnitTests.Converter
{
    [TestClass]
    public class DecimalPrecision3ConverterTests
    {
        private readonly JsonSerializerOptions _options = new()
        {
            Converters = { new DecimalPrecision3Converter() }
        };

        [TestMethod]
        public void Serialize_WritesThreeDecimalPlaces()
        {
            decimal value = 1.23456m;
            var json = JsonSerializer.Serialize(value, _options);
            Assert.AreEqual("1.235", json.Trim('"'));
        }

        [TestMethod]
        public void Serialize_Zero_WritesZero()
        {
            decimal value = 0m;
            var json = JsonSerializer.Serialize(value, _options);
            Assert.AreEqual("0", json.Trim('"'));
        }

        [TestMethod]
        public void Deserialize_ReadsDecimalCorrectly()
        {
            var json = "\"2.345\"";
            var result = JsonSerializer.Deserialize<decimal>(json, _options);
            Assert.AreEqual(2.345m, result);
        }
    }
}