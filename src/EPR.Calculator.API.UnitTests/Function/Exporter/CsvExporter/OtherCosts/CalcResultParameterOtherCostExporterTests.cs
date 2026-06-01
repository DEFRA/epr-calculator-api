using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.OtherCosts
{
    [TestClass]
    public class CalcResultParameterOtherCostExporterTests
    {
        private CalcResultParameterOtherCostExporter exporter = new CalcResultParameterOtherCostExporter();

        [TestMethod]
        public void CanCallExportCommsCost()
        {
            // Arrange
            var otherCost = new CalcResultParameterOtherCost
            {
                SaOperatingCost = new() { England = 25000, Wales = 14000, Scotland = 17000, NorthernIreland = 9000 },
                LaDataPrepCharge = new() { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 },
                CountryApportionment = new() { England = 43.83561644m, Wales = 19.17808219m, Scotland = 24.65753425m, NorthernIreland = 12.32876712m },
                SchemeSetupCost = new () { England = 17500, Wales = 23400, Scotland = 12400, NorthernIreland = 9450 },
                BadDebtValue = 6,
                MaterialityIncrease = new Materiality { Amount = 5000, Percentage = 2 },
                MaterialityDecrease = new Materiality { Amount = -1000, Percentage = -1 },
                TonnageChangeIncrease = new Materiality { Amount = 50, Percentage = 2 },
                TonnageChangeDecrease = new Materiality { Amount = -10, Percentage = -0.5m }
            };

            // Act
            var csvContent = new StringBuilder();
            exporter.Export(otherCost, csvContent);

            var result = csvContent.ToString().Split("\n").Select(s => s.TrimEnd(',')).ToArray();
            Console.WriteLine(string.Join("\n", result));

            // Assert
            var expected = new[] {
                new string[] {},
                new string[] {},
                new[] { "Parameters - Other" },
                new[] { null,"England","Wales","Scotland","Northern Ireland","Total" },
                new[] { "3 SA Operating Costs","£25000.00","£14000.00","£17000.00","£9000.00","£65000.00" },
                new string[] {},
                new[] { "4 LA Data Prep Charge","£40.00","£30.00","£20.00","£10.00","£100.00" },
                new[] { "4 Country Apportionment %s","43.83561644%","19.17808219%","24.65753425%","12.32876712%","100.00000000%" },
                new string[] {},
                new[] { "5 Scheme set up cost Yearly Cost","£17500.00","£23400.00","£12400.00","£9450.00","£62750.00" },
                new string[] {},
                new[] { "6 Bad Debt Provision","6.00%" },
                new string[] {},
                new[] { "7 Materiality","Amount £s","%" },
                new[] { "Increase","£5000.00","2.00%" },
                new[] { "Decrease","-£1000.00","-1.00%" },
                new[] { "8 Tonnage Change","Amount £s","%" },
                new[] { "Increase","£50.00","2.00%" },
                new[] { "Decrease","-£10.00","-0.50%" },
                new string[] {}
            };

            CsvTestUtils.AssertCsv(expected, result);
        }
    }
}
