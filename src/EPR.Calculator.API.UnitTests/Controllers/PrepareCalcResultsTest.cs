using Castle.Core.Configuration;
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
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Exporter;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Wrapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class PrepareCalcResultsTest : BaseControllerTest
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
        private readonly CalculatorInternalController controller;
        private CalcResultDetailBuilder detailBuilder;
        private readonly Mock<CalculatorRunValidator> mockValidator;

        private readonly Mock<ApplicationDBContext> mockContext;
        private readonly CalcResultBuilder calcResultBuilder;
        protected readonly new IOrgAndPomWrapper? wrapper;
        private readonly Mock<ICalcResultOnePlusFourApportionmentBuilder> mockICalcResultOnePlusFourApportionmentBuilder;

        private Mock<IStorageService>? mockStorageservice;

        public PrepareCalcResultsTest()
        {
            mockCalcResultBuilder = new Mock<ICalcResultBuilder>();
            mockExporter = new Mock<ICalcResultsExporter<CalcResult>>();
            mockExporter.Setup(x => x.Export(It.IsAny<CalcResult>())).Returns("Somevalue");
            wrapper = new Mock<IOrgAndPomWrapper>().Object;
            var transposePomAndOrgDataService = new Mock<ITransposePomAndOrgDataService>();
            transposePomAndOrgDataService.Setup(x => x.Transpose(It.IsAny<CalcResultsRequestDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            mockStorageservice = new Mock<IStorageService>();
            mockStorageservice.Setup(x => x.UploadResultFileContentAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("expected result");

            mockValidator = new Mock<CalculatorRunValidator>();

            controller = new CalculatorInternalController(
               dbContext,
               new RpdStatusDataValidator(wrapper),
               wrapper,
               mockCalcResultBuilder.Object,
               mockExporter.Object,
               transposePomAndOrgDataService.Object,
               mockStorageservice.Object,
               mockValidator.Object,
               new CommandTimeoutService(new ConfigurationBuilder().Build())
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
            var requestDto = new CalcResultsRequestDto() { RunId = 4 };
            var calcResult = new CalcResult
            {
                CalcResultDetail = new CalcResultDetail
                {
                    RunId = 4,
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

        [TestMethod]
        public void CheckForNullIds_ShouldReturnErrorMessages_WhenIdsAreNull()
        {
            // Arrange
            var calculatorRun = new CalculatorRun
            {
                CalculatorRunOrganisationDataMasterId = null,
                DefaultParameterSettingMasterId = null,
                CalculatorRunPomDataMasterId = 1,
                LapcapDataMasterId = null,
                Name = "soe",
                Financial_Year = "2024-25",
            };

            var validator = new CalculatorRunValidator();

            // Act
            ValidationResult result = validator.ValidateCalculatorRunIds(calculatorRun);

            // Assert
            var expectedErrors = new List<string>
            {
                "CalculatorRunOrganisationDataMasterId is null",
                "DefaultParameterSettingMasterId is null",
                "LapcapDataMasterId is null"
            };
            CollectionAssert.AreEqual(expectedErrors, result.ErrorMessages.ToList());
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public void CheckForNullIds_ShouldReturnEmptyList_WhenNoIdsAreNull()
        {
            // Arrange
            var calculatorRun = new CalculatorRun
            {
                CalculatorRunOrganisationDataMasterId = 1,
                DefaultParameterSettingMasterId = 1,
                CalculatorRunPomDataMasterId = 1,
                LapcapDataMasterId = 1,
                Name = "soe",
                Financial_Year = "2024-25",
            };

            var validator = new CalculatorRunValidator();

            // Act
            ValidationResult result = validator.ValidateCalculatorRunIds(calculatorRun);

            // Assert
            Assert.AreEqual(0, result.ErrorMessages.Count());
            Assert.IsTrue(result.IsValid);
        }
    }
}