using AutoFixture;
using EPR.Calculator.API.Exporter;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Text;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class CalcResultsExporterTests
    {
        private readonly CalcResultsExporter _calcResultsExporter;
        private readonly Mock<IStorageService> _blobStorageServiceMock;

        public CalcResultsExporterTests()
        {
            this.Fixture = new Fixture();
            _blobStorageServiceMock = new Mock<IStorageService>();
            _calcResultsExporter = new CalcResultsExporter(_blobStorageServiceMock.Object);
        }

        private Fixture Fixture { get; init; }

        [TestMethod]
        public void Export_ShouldHandleIOExceptionGracefully()
        {
            var calcResult = new CalcResult
            {
                CalcResultParameterOtherCost = Fixture.Create<CalcResultParameterOtherCost>(),
                CalcResultDetail = new CalcResultDetail
                {
                    RunName = "Test Run",
                    RunId = 123,
                    RunDate = DateTime.Now,
                    RunBy = "Tester",
                    FinancialYear = "2023-24",
                    LapcapFile = "Lapcap.csv,2023-10-01,John Doe",
                    ParametersFile = "Params.csv,2023-10-02,Jane Doe",
                },
                CalcResultLapcapData = Fixture.Create<CalcResultLapcapData>(),
                CalcResultLateReportingTonnageData = Fixture.Create<CalcResultLateReportingTonnage>(),
            };

            _blobStorageServiceMock
                .Setup(service => service.UploadResultFileContentAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new IOException("Simulated IO error"));
            var exception = Assert.ThrowsException<IOException>(() => _calcResultsExporter.Export(calcResult));
            Assert.AreEqual("File upload failed: Simulated IO error", exception.Message);
        }


        //[TestMethod]
        //public void AppendFileInfo_ShouldNotAppendIfFilePartsAreInvalid()
        //{
        //    var calcResult = new CalcResult
        //    {
        //        CalcResultDetail = new CalcResultDetail
        //        {
        //            RunName = "Test Run",
        //            RunId = 123,
        //            RunDate = DateTime.Now,
        //            RunBy = "Tester",
        //            FinancialYear = "2023-24",
        //            LapcapFile = "InvalidFileInfo",
        //            ParametersFile = "InvalidFileInfo",
        //            RpdFileORG = "04/11/2024 12:06",
        //            RpdFilePOM = "04/11/2024 12:07",
        //        },
        //        CalcResultLateReportingTonnageData = new CalcResultLateReportingTonnage
        //        {
        //            Name = "Late Reporting Tonnages",
        //            MaterialHeading = "Material",
        //            TonnageHeading = "Tonnage",
        //            CalcResultLateReportingTonnageDetails = new List<CalcResultLateReportingTonnageDetail>
        //            {
        //                new CalcResultLateReportingTonnageDetail { Name = "Aluminium", TotalLateReportingTonnage = 100.000M },
        //                new CalcResultLateReportingTonnageDetail { Name = "Fibre composite", TotalLateReportingTonnage = 200.000M },
        //                new CalcResultLateReportingTonnageDetail { Name = "Total", TotalLateReportingTonnage = 300.000M }
        //            }
        //        }
        //    };

        //    var expectedCsvContent = new StringBuilder();
        //    expectedCsvContent.AppendLine("Run Name,Test Run");
        //    expectedCsvContent.AppendLine("Run Id,123");
        //    expectedCsvContent.AppendLine("Run Date," + calcResult.CalcResultDetail.RunDate.ToString("dd/MM/yyyy HH:mm"));
        //    expectedCsvContent.AppendLine("Run by,Tester");
        //    expectedCsvContent.AppendLine("Financial Year,2023-24");
        //    expectedCsvContent.AppendLine("RPD File - ORG,04/11/2024 12:06,RPD File - POM,04/11/2024 12:07");

        //    expectedCsvContent.AppendLine();
        //    expectedCsvContent.AppendLine();
        //    expectedCsvContent.AppendLine("Late Reporting Tonnages");
        //    expectedCsvContent.AppendLine("Material,Tonnage");
        //    expectedCsvContent.AppendLine("Aluminium,100.000");
        //    expectedCsvContent.AppendLine("Fibre composite,200.000");
        //    expectedCsvContent.AppendLine("Total,300.000");

        //    _calcResultsExporter.Export(calcResult);

        //    _blobStorageServiceMock.Verify(service => service.UploadResultFileContentAsync(
        //        $"{calcResult.CalcResultDetail.RunId}-{DateTime.Now:yyyy-MM-dd-HHmm}.csv",
        //        It.Is<StringBuilder>(content => content.ToString() == expectedCsvContent.ToString())
        //    ), Times.Once);
        //}

        [TestMethod]
        public void Export_CreatesCorrectCsvContent()
        {
            // Arrange
            var calcResult = new CalcResult
            {
                CalcResultParameterOtherCost = Fixture.Create<CalcResultParameterOtherCost>(),
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
                },
                CalcResultLapcapData = Fixture.Create<CalcResultLapcapData>(),
            };

            var expectedFileName = $"{calcResult.CalcResultDetail.RunId}" +
                $"-{calcResult.CalcResultDetail.RunName}" +
                $"_Results File" +
                $"_{calcResult.CalcResultDetail.RunDate:yyyyMMdd}.csv";

            // Act
            _calcResultsExporter.Export(calcResult);

            // Assert
            _blobStorageServiceMock.Verify(x => x.UploadResultFileContentAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once); 
            _blobStorageServiceMock.Verify(x => x.UploadResultFileContentAsync(expectedFileName, It.IsAny<string>()), Times.Once);
        }
    }
}
