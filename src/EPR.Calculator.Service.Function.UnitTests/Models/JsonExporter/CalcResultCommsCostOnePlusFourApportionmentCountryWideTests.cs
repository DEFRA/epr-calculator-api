using System.Text.Json;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    [TestClass]
    public class CalcResultCommsCostOnePlusFourApportionmentCountryWideTests
    {
        private IFixture Fixture { get; init; }

        public CalcResultCommsCostOnePlusFourApportionmentCountryWideTests()
        {
            Fixture = new Fixture();
        }

        [TestMethod]
        public void CalcResultCommsCostOnePlusFourApportionmentCountryWideTests_CanCallFrom_WithValidData()
        {
            // Arrange
            var countryData = new ByCountryCost()
            {
                England         = 1.23m,
                Wales           = 2.34m,
                Scotland        = 3.45m,
                NorthernIreland = 4.56m
            };

            // Act
            var result = CalcResultCommsCostOnePlusFourApportionmentCountryWide.From(countryData);

            // Assert
            Assert.IsNotNull(result);
            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);
            var expectedJson = """
                {
                "name": "2c Comms Costs - by Country",
                "englandCommsCostByCountry"        : "£1.23",
                "walesCommsCostByCountry"          : "£2.34",
                "scotlandCommsCostByCountry"       : "£3.45",
                "northernIrelandCommsCostByCountry": "£4.56",
                "totalCommsCostByCountry"          : "£11.58"
                }
                """;
            JsonTestUtils.AssertJson(expectedJson, json);
        }

        [TestMethod]
        public void From_WithNullData_ReturnsNull()
        {
            // Act
            var result = CalcResultCommsCostOnePlusFourApportionmentCountryWide.From(null);

            // Assert
            Assert.IsNull(result);
        }
    }
}
