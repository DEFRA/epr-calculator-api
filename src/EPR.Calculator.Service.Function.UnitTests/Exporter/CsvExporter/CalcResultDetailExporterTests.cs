using System.Text;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Detail;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter
{
    [TestClass]
    public class CalcResultDetailExporterTests
    {
        private readonly CalcResultDetailExporter exporter;

        public CalcResultDetailExporterTests() => exporter = new CalcResultDetailExporter();

        [TestMethod]
        public void CalcResultDetailExporter_CanCallExport()
        {
            // Arrange
            var calcResultDetail = new CalcResultDetail
            {
                RunId          = 999,
                RunName        = "SomeRunName",
                RunDate        = new DateTime(2026, 7, 9),
                RelativeYear   = new RelativeYear(2026),
                RunBy          = "Me",
                RpdFileORG     = "09/07/2026 16:27",
                RpdFilePOM     = "09/07/2026 16:27",
                LapcapFile     = "09/07/2026 15:27",
                ParametersFile = "09/07/2026 15:27",
                CutOffDate     = null
            };

            var csvContent = new StringBuilder();

            // Act
            exporter.Export(calcResultDetail, csvContent);

            var result = csvContent.ToString()
                .ReplaceLineEndings("\n")
                .Split("\n")
                .Select(s => s.TrimEnd(','))
                .ToArray();

            var expected = new[]
            {
                @"Run Name,""SomeRunName""",
                @"Run Id,""999""",
                @"Run Date,""09/07/2026 00:00""",
                @"Run by,""Me""",
                @"Financial Year,""2026-27""",
                @"Cut-off Date,""NA""",
                @"RPD File - ORG,""09/07/2026 16:27"",,RPD File - POM,""09/07/2026 16:27""",
                ""
            };

            Assert.HasCount(expected.Length, result, "CSV line count differs");

            for (var i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], result[i], $"CSV line {i + 1} differs");
            }
        }
    }
}
