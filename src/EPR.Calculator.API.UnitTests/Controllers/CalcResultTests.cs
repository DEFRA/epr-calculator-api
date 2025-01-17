using EPR.Calculator.API.Builder;
using EPR.Calculator.API.Builder.CommsCost;
using EPR.Calculator.API.Builder.Detail;
using EPR.Calculator.API.Builder.LaDisposalCost;
using EPR.Calculator.API.Builder.Lapcap;
using EPR.Calculator.API.Builder.LateReportingTonnages;
using EPR.Calculator.API.Builder.OnePlusFourApportionment;
using EPR.Calculator.API.Builder.ParametersOther;
using EPR.Calculator.API.Builder.Summary;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Exporter;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Wrapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class CalcResultTests : BaseControllerTest
    {
        private readonly Mock<ICalcResultBuilder> mockCalcResultBuilder;
        private readonly Mock<ICalcResultsExporter<CalcResult>> mockExporter;
        private readonly Mock<ICalcResultDetailBuilder> mockDetailBuilder;
        private readonly Mock<ICalcResultLapcapDataBuilder> mockLapcapBuilder;
        private readonly Mock<ICalcResultSummaryBuilder> mockSummaryBuilder;
        private readonly Mock<ICalcResultLateReportingBuilder> mocklateReportingBuilder;
        private readonly Mock<ICalcRunLaDisposalCostBuilder> mockLaDisposalCostBuilder;
        private readonly Mock<ICalcResultCommsCostBuilder> mockCommsCostReportBuilder;
        private readonly Mock<ICalcResultParameterOtherCostBuilder> mockCalcResultParameterOtherCostBuilder;
        private readonly Mock<ITransposePomAndOrgDataService> transposePomAndOrgDataService;
        private CalculatorInternalController controller;
        private CalcResultDetailBuilder detailBuilder;

        private readonly Mock<ApplicationDBContext> mockContext;
        private readonly CalcResultBuilder calcResultBuilder;
        protected readonly new IOrgAndPomWrapper? wrapper;
        private readonly Mock<ICalcResultOnePlusFourApportionmentBuilder> mockICalcResultOnePlusFourApportionmentBuilder;

        private Mock<IStorageService>? mockStorageservice;

        public CalcResultTests()
        {
            mockCalcResultBuilder = new Mock<ICalcResultBuilder>();
            mockExporter = new Mock<ICalcResultsExporter<CalcResult>>();
            mockExporter.Setup(x => x.Export(It.IsAny<CalcResult>())).Returns("Somevalue");
            wrapper = new Mock<IOrgAndPomWrapper>().Object;
            transposePomAndOrgDataService = new Mock<ITransposePomAndOrgDataService>();
            mockStorageservice = new Mock<IStorageService>();
            mockStorageservice.Setup(x => x.UploadResultFileContentAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            controller = new CalculatorInternalController(
               dbContext,
               new RpdStatusDataValidator(wrapper),
               wrapper,
               mockCalcResultBuilder.Object,
               mockExporter.Object,
               transposePomAndOrgDataService.Object,
               mockStorageservice.Object
            );
            controller.ControllerContext.HttpContext = new Mock<HttpContext>().Object;

            mockDetailBuilder = new Mock<ICalcResultDetailBuilder>();
            mockLapcapBuilder = new Mock<ICalcResultLapcapDataBuilder>();
            mockSummaryBuilder = new Mock<ICalcResultSummaryBuilder>();
            mocklateReportingBuilder = new Mock<ICalcResultLateReportingBuilder>();
            mockLaDisposalCostBuilder = new Mock<ICalcRunLaDisposalCostBuilder>();
            mockCommsCostReportBuilder = new Mock<ICalcResultCommsCostBuilder>();
            mockCalcResultParameterOtherCostBuilder = new Mock<ICalcResultParameterOtherCostBuilder>();
            mockICalcResultOnePlusFourApportionmentBuilder = new Mock<ICalcResultOnePlusFourApportionmentBuilder>();
            calcResultBuilder = new CalcResultBuilder(
                mockDetailBuilder.Object,
                mockLapcapBuilder.Object,
                mockCalcResultParameterOtherCostBuilder.Object,
                mockICalcResultOnePlusFourApportionmentBuilder.Object,
                mockCommsCostReportBuilder.Object,
                mocklateReportingBuilder.Object,
                mockLaDisposalCostBuilder.Object,
                mockSummaryBuilder.Object);

            mockContext = new Mock<ApplicationDBContext>();
            detailBuilder = new CalcResultDetailBuilder(mockContext.Object);

        }

        [TestMethod]
        public void PrepareCalcResults_ShouldReturnCreatedStatus()
        {
            this.transposePomAndOrgDataService.Setup(x => x.Transpose(It.IsAny<CalcResultsRequestDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var requestDto = new CalcResultsRequestDto() { RunId = 1 };
            var calcResult = new CalcResult
            {
                CalcResultDetail = new CalcResultDetail
                {
                    RunId = 1,
                    RunDate = DateTime.Now,
                    RunName = "RunName"
                },
                CalcResultLapcapData = new CalcResultLapcapData
                {
                    Name = string.Empty,
                    CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>()
                },
                CalcResultParameterOtherCost = new()
                {
                    BadDebtProvision = new KeyValuePair<string, string>(),
                    Name = string.Empty,
                    Details = new List<CalcResultParameterOtherCostDetail>(),
                    Materiality = new List<CalcResultMateriality>(),
                    SaOperatingCost = new List<CalcResultParameterOtherCostDetail>(),
                    SchemeSetupCost = new CalcResultParameterOtherCostDetail()
                },
                CalcResultLateReportingTonnageData = new()
                {
                    Name = string.Empty,
                    CalcResultLateReportingTonnageDetails = new List<CalcResultLateReportingTonnageDetail>(),
                    MaterialHeading = string.Empty,
                    TonnageHeading = string.Empty
                }
            };

            mockCalcResultBuilder.Setup(b => b.Build(It.IsAny<CalcResultsRequestDto>())).ReturnsAsync(calcResult);

            var task = controller.PrepareCalcResults(requestDto);
            task.Wait();

            var result = task.Result as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(201, result.StatusCode);
            mockExporter.Verify(e => e.Export(calcResult), Times.Once);
        }

        [TestMethod]
        public void Build_ShouldReturnCalcResultWithDetail()
        {
            var requestDto = new CalcResultsRequestDto();
            var detail = new CalcResultDetail();
            mockDetailBuilder.Setup(d => d.Construct(requestDto)).ReturnsAsync(detail);

            var results = calcResultBuilder.Build(requestDto);
            results.Wait();
            var result = results.Result;

            Assert.IsNotNull(result);
            Assert.AreEqual(detail, result.CalcResultDetail);
        }
    }
}

