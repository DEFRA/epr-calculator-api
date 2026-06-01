using System.Text.Json;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Function.UnitTests.Utils;

namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    [TestClass]
    public class CalcResultCommsCostOnePlusFourApportionmentUKWideTests
    {
        private IFixture Fixture { get; init; }

        public CalcResultCommsCostOnePlusFourApportionmentUKWideTests()
        {
            Fixture = new Fixture();
        }

        [TestMethod]
        public void CalcResultCommsCostOnePlusFourApportionmentUKWideTests_CanCallFrom_WithValidData()
        {
            // Arrange
            var ukWideData = new ByCountryCost()
            {
                England         = 1.23m,
                Wales           = 2.34m,
                Scotland        = 3.45m,
                NorthernIreland = 4.56m
            };

            // Act
            var result = CalcResultCommsCostOnePlusFourApportionmentUKWide.From(ukWideData);

            // Assert
            Assert.IsNotNull(result);
            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);
            var expectedJson = """
                {
                "name": "2b Comms Costs - UK wide",
                "englandCommsCostUKWide"        : "£1.23",
                "walesCommsCostUKWide"          : "£2.34",
                "scotlandCommsCostUKWide"       : "£3.45",
                "northernIrelandCommsCostUKWide": "£4.56",
                "totalCommsCostUKWide"          : "£11.58"
                }
                """;
            JsonTestUtils.AssertJson(expectedJson, json);
        }
    }
}
