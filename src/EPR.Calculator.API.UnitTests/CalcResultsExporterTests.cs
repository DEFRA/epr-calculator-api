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
        private CalcResultsExporter _calcResultsExporter;
        private Mock<IBlobStorageService> _blobStorageServiceMock;

        [TestInitialize]
        public void Setup()
        {
            _blobStorageServiceMock = new Mock<IBlobStorageService>();
            _calcResultsExporter = new CalcResultsExporter(_blobStorageServiceMock.Object);
        }

        //[TestMethod]
        //public void Export_ShouldHandleIOExceptionGracefully()
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
        //            LapcapFile = "Lapcap.csv,2023-10-01,John Doe",
        //            ParametersFile = "Params.csv,2023-10-02,Jane Doe"
        //        }
        //    };

        //    _blobStorageServiceMock
        //        .Setup(service => service.UploadResultFileContentAsync(It.IsAny<string>(), It.IsAny<StringBuilder>()))
        //        .Throws(new IOException("Simulated IO error"));
        //    var exception = Assert.ThrowsException<IOException>(() => _calcResultsExporter.Export(calcResult));
        //    Assert.AreEqual("File upload failed: Simulated IO error", exception.Message);
        //}


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
        //        }
        //    };

        //    var expectedCsvContent = new StringBuilder();
        //    expectedCsvContent.AppendLine("Run Name,Test Run");
        //    expectedCsvContent.AppendLine("Run Id,123");
        //    expectedCsvContent.AppendLine("Run Date," + calcResult.CalcResultDetail.RunDate.ToString("dd/MM/yyyy HH:mm"));
        //    expectedCsvContent.AppendLine("Run by,Tester");
        //    expectedCsvContent.AppendLine("Financial Year,2023-24");
        //    expectedCsvContent.AppendLine("RPD File - ORG,04/11/2024 12:06,RPD File - POM,04/11/2024 12:07");

        //    _calcResultsExporter.Export(calcResult);

        //    _blobStorageServiceMock.Verify(service => service.UploadResultFileContentAsync(
        //        $"{calcResult.CalcResultDetail.RunId}-{DateTime.Now:yyyy-MM-dd-HHmm}.csv",
        //        It.Is<StringBuilder>(content => content.ToString() == expectedCsvContent.ToString())
        //    ), Times.Once);
        //}

        //[TestMethod]
        //public void Export_CreatesCorrectCsvContent()
        //{
        //    var calcResult = new CalcResult
        //    {
        //        CalcResultDetail = new CalcResultDetail
        //        {
        //            RunName = "Test Run",
        //            RunId = 123,
        //            RunDate = DateTime.Now,
        //            RunBy = "Tester",
        //            FinancialYear = "2024",
        //            LapcapFile = "lapcap.csv,2024-11-01,Tester",
        //            ParametersFile = "params.csv,2024-11-01,Tester"
        //        }
        //    };

        //    _calcResultsExporter.Export(calcResult);

        //    _blobStorageServiceMock.Verify(x => x.UploadResultFileContentAsync(It.IsAny<string>(), It.IsAny<StringBuilder>()), Times.Once);
        //    var expectedFileName = $"{calcResult.CalcResultDetail.RunId}-{DateTime.Now:yyyy-MM-dd-HHmm}.csv";
        //    _blobStorageServiceMock.Verify(x => x.UploadResultFileContentAsync(expectedFileName, It.IsAny<StringBuilder>()), Times.Once);
        //}
    }
}
