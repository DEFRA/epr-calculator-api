using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap;
using EPR.Calculator.Service.Function.UnitTests.Builder;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Lapcap
{
    [TestClass]
    public class CalcResultLapcapDataExporterTests
    {
        private ICalcResultLapcapDataExporter lapcapDataExporter = new CalcResultLapcapDataExporter();

        [TestMethod]
        public void ExportTest_ShouldShowCorrectHeaderAndRows()
        {
            // Arrange
            var csvContent = new StringBuilder();

            // Act
            lapcapDataExporter.Export(TestDataHelper.GetCalcResultLapcapData(), TestDataHelper.GetMaterials(), csvContent);

            // Assert
            var result = csvContent.ToString().Split("\n").Select(s => s.TrimEnd(',')).ToArray();
            Console.WriteLine(string.Join("\n", result));


            var expected = new[] {
                new string[] {},
                new string[] {},
                new string[] { "LAPCAP Data" },
                new string[] { "Material"                  ,"England LA Disposal Cost","Wales LA Disposal Cost","Scotland LA Disposal Cost","Northern Ireland LA Disposal Cost","1 LA Disposal Cost Total" },
                new string[] { "Aluminium"                 ,                "£5000.00",              "£1750.00",                 "£2000.00",                         "£1250.00",               "£10000.00" },
                new string[] { "Fibre composite"           ,                "£7500.00",              "£2100.00",                 "£3400.00",                         "£1750.00",               "£14750.00" },
                new string[] { "Glass"                     ,               "£45000.00",                 "£0.00",                "£20700.00",                         "£4500.00",               "£70200.00" },
                new string[] { "Paper or card"             ,               "£12500.00",              "£2300.00",                 "£4500.00",                         "£3400.00",               "£22700.00" },
                new string[] { "Plastic"                   ,               "£23000.00",              "£4500.00",                 "£6700.00",                         "£2100.00",               "£36300.00" },
                new string[] { "Steel"                     ,               "£13400.00",                 "£0.00",                 "£7800.00",                            "£0.00",               "£21200.00" },
                new string[] { "Wood"                      ,                   "£0.00",             "£12000.00",                    "£0.00",                         "£5600.00",               "£17600.00" },
                new string[] { "Other materials"           ,                "£3400.00",              "£2100.00",                 "£4200.00",                          "£700.00",               "£10400.00" },
                new string[] { "Total"                     ,              "£109800.00",             "£24750.00",                "£49300.00",                        "£19300.00",              "£203150.00" },
                new string[] { "1 Country Apportionment %s",            "54.04873246%",          "12.18311592%",             "24.26778243%",                      "9.50036919%",           "100.00000000%" },
                new string[] {}
            };

            CsvTestUtils.AssertCsv(expected, result);
        }
    }
}
