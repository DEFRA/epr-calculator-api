using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter
{
    [TestClass]
    public class CalcResultOnePlusFourApportionmentExporterTests
    {
        private CalcResultOnePlusFourApportionmentExporter _testClass = new CalcResultOnePlusFourApportionmentExporter();
        private CalcResultOnePlusFourApportionment calcResult1Plus4Apportionment;

        public CalcResultOnePlusFourApportionmentExporterTests()
        {
            calcResult1Plus4Apportionment = new CalcResultOnePlusFourApportionment
            {
                LaDisposalCost   = new() { England = 109800, Wales = 24750, Scotland = 49300, NorthernIreland = 19300 },
                LADataPrepCharge = new() { England =  16000, Wales =  7000, Scotland =  9000, NorthernIreland =  4500 }
            };
        }

        [TestMethod]
        public void OnePlusFourApportionment_CanCallExport()
        {
            // Arrange
            var csvContent = new StringBuilder();

            // Act
            _testClass.Export(calcResult1Plus4Apportionment, csvContent);

            // Assert
            var result = csvContent.ToString().Split("\n").Select(s => s.TrimEnd(',')).ToArray();
            Console.WriteLine(string.Join("\n", result));

            var expected = new[] {
                new string[] {},
                new string[] {},
                new[] { "1 + 4 Apportionment %s" },
                new[] { null,
                        "England",
                        "Wales",
                        "Scotland",
                        "Northern Ireland",
                        "Total"
                },
                new[] { "1 Fee for LA Disposal Costs",   "£109800.00",   "£24750.00",   "£49300.00",  "£19300.00",    "£203150.00" },
                new[] { "4 LA Data Prep Charge"      ,    "£16000.00",    "£7000.00",    "£9000.00",   "£4500.00",     "£36500.00" },
                new[] { "Total of 1 + 4"             ,   "£125800.00",   "£31750.00",   "£58300.00",  "£23800.00",    "£239650.00" },
                new[] { "1 + 4 Apportionment %s"     , "52.49321928%","13.24848738%","24.32714375%","9.93114959%", "100.00000000%" },
                new string[] { }
            };


            CsvTestUtils.AssertCsv(expected, result);
        }
    }
}
