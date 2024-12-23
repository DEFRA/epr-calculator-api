using EPR.Calculator.API.Exporter;
using EPR.Calculator.API.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class CalcResultsExporterTests
    {
        private CalcResultsExporter _calcResultsExporter;

        [TestInitialize]
        public void Setup()
        {
            _calcResultsExporter = new CalcResultsExporter();
        }

        [TestMethod]
        public void Export_CreatesCorrectCsvContent()
        {
            // Arrange
            var calcResult = new CalcResult
            {
                CalcResultDetail = new CalcResultDetail
                {
                    RunName = "Test Run",
                    RunId = 123,
                    RunDate = DateTime.Now,
                    RunBy = "Tester",
                    FinancialYear = "2024",
                    LapcapFile = "lapcap.csv,2024-11-01,Tester",
                    ParametersFile = "params.csv,2024-11-01,Tester"
                },
                CalcResultLateReportingTonnageData = new CalcResultLateReportingTonnage
                {
                    Name = "Late Reporting Tonnages",
                    MaterialHeading = "Material",
                    TonnageHeading = "Tonnage",
                    CalcResultLateReportingTonnageDetails = new List<CalcResultLateReportingTonnageDetail>
                    {
                        new CalcResultLateReportingTonnageDetail { Name = "Aluminium", TotalLateReportingTonnage = 100.000M },
                        new CalcResultLateReportingTonnageDetail { Name = "Fibre composite", TotalLateReportingTonnage = 200.000M },
                        new CalcResultLateReportingTonnageDetail { Name = "Total", TotalLateReportingTonnage = 300.000M }
                    }
                }
            };

            var expectedFileName = $"{calcResult.CalcResultDetail.RunId}" +
                $"-{calcResult.CalcResultDetail.RunName}" +
                $"_Results File" +
                $"_{calcResult.CalcResultDetail.RunDate:yyyyMMdd}.csv";

            // Act
            _calcResultsExporter.Export(calcResult);

        }
    }
}
