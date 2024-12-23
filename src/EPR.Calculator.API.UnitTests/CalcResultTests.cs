using EPR.Calculator.API.Builder;
using EPR.Calculator.API.Builder.LaDisposalCost;
using EPR.Calculator.API.Builder.Lapcap;
using EPR.Calculator.API.Builder.LateReportingTonnages;
using EPR.Calculator.API.Builder.Summary;
using EPR.Calculator.API.Builder.OnePlusFourApportionment;
using EPR.Calculator.API.Builder.ParametersOther;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Exporter;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Wrapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using EPR.Calculator.API.Builder.CommsCost;
using EPR.Calculator.API.Builder.Detail;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Tests.Controllers;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class CalcResultTests : BaseControllerTest
    {
        private Mock<ICalcResultBuilder> mockCalcResultBuilder;
        private Mock<ICalcResultsExporter<CalcResult>> mockExporter;
        private Mock<ICalcResultDetailBuilder> mockDetailBuilder;
        private Mock<ICalcResultLapcapDataBuilder> mockLapcapBuilder;
        private Mock<ICalcResultSummaryBuilder> mockSummaryBuilder;
        private Mock<ICalcResultLateReportingBuilder> mocklateReportingBuilder;
        private Mock<ICalcRunLaDisposalCostBuilder> mockLaDisposalCostBuilder;
        private Mock<ICalcResultCommsCostBuilder> mockCommsCostReportBuilder;
        private Mock<ICalcResultParameterOtherCostBuilder> mockCalcResultParameterOtherCostBuilder;

        private Mock<ApplicationDBContext> mockContext;
        private CalculatorInternalController controller;
        private CalcResultBuilder calcResultBuilder;
        private CalcResultDetailBuilder detailBuilder;
        private CalcResultsExporter exporter;
        protected IOrgAndPomWrapper? wrapper;
        private Mock<ICalcResultOnePlusFourApportionmentBuilder> mockICalcResultOnePlusFourApportionmentBuilder;
        private Mock<IStorageService>? mockStorageservice;

        [TestInitialize]
        public void Setup()
        {
            mockCalcResultBuilder = new Mock<ICalcResultBuilder>();
            mockExporter = new Mock<ICalcResultsExporter<CalcResult>>();
            wrapper = new Mock<IOrgAndPomWrapper>().Object;
            var transposePomAndOrgDataService = new Mock<ITransposePomAndOrgDataService>();
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
            var requestDto = new CalcResultsRequestDto() { RunId = 1 };
            var calcResult = new CalcResult
            {
                CalcResultDetail = new CalcResultDetail
                {
                    RunId = 1,
                    RunName = "Some",
                    RunDate = DateTime.Now
                }
            };
            mockCalcResultBuilder.Setup(b => b.Build(It.IsAny<CalcResultsRequestDto>())).Returns(calcResult);

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
            mockDetailBuilder.Setup(d => d.Construct(requestDto)).Returns(detail);

            var result = calcResultBuilder.Build(requestDto);

            Assert.IsNotNull(result);
            Assert.AreEqual(detail, result.CalcResultDetail);
        }
    }
}

