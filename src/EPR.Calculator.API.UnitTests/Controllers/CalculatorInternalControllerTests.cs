using AutoFixture;
using Castle.Components.DictionaryAdapter.Xml;
using EPR.Calculator.API.Builder;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Exporter;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.UnitTests.Controllers;
using EPR.Calculator.API.Utils;
using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Wrapper;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Amqp.Transaction;
using Microsoft.EntityFrameworkCore.InMemory.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Net;
using System.Threading.Tasks;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class CalculatorInternalControllerTests : BaseControllerTest
    {
        private Fixture Fixture { get; init; } = new Fixture();
        private readonly Mock<CalculatorRunValidator> mockValidator;

        private IConfiguration Configuration { get; init; } = new ConfigurationBuilder().Build();

        public CalculatorInternalControllerTests()
        {
            mockValidator = new Mock<CalculatorRunValidator>();
            MockTransposeService = new Mock<ITransposePomAndOrgDataService>();
            MockTransposeService.Setup(t => t.Transpose(
                It.IsAny<CalcResultsRequestDto>(),
                new CancellationToken(false)))
                .ReturnsAsync(true);
            MockTransposeService.Setup(t => t.Transpose(
                It.IsAny<CalcResultsRequestDto>(),
                new CancellationToken(true)))
                .ThrowsAsync(new TaskCanceledException("Timed out."));
        }

        private Mock<ITransposePomAndOrgDataService> MockTransposeService { get; init; }

        [TestMethod]
        public async Task UpdateRpdStatus_With_Missing_RunId()
        {
            var request = new Dtos.UpdateRpdStatus { isSuccessful = false, RunId = 999, UpdatedBy = "User1" };
            var result = await this.calculatorInternalController.UpdateRpdStatus(request);
            var objResult = result as ObjectResult;
            Assert.IsNotNull(objResult);
            Assert.AreEqual(400, objResult.StatusCode);
            Assert.AreEqual("Calculator Run 999 is missing", objResult.Value);
        }

        [TestMethod]
        public async Task UpdateRpdStatus_With_RunId_Having_OrganisationDataMasterId()
        {
            this.dbContext?.SaveChanges();

            this.dbContext?.CalculatorRuns.Add(new CalculatorRun
            {
                CalculatorRunClassificationId = 1,
                CreatedAt = DateTime.Now,
                CreatedBy = "Some User",
                Financial_Year = "2024-25",
                Name = "CalculationRun1-Test",
                DefaultParameterSettingMasterId = 1,
                LapcapDataMasterId = 1,
                CalculatorRunOrganisationDataMasterId = 1,
            });

            this.dbContext?.SaveChanges();


            var request = new Dtos.UpdateRpdStatus { isSuccessful = false, RunId = 3, UpdatedBy = "User1" };
            var result = await this.calculatorInternalController.UpdateRpdStatus(request);
            var objResult = result as ObjectResult;
            Assert.IsNotNull(objResult);
            Assert.AreEqual(422, objResult.StatusCode);
            Assert.AreEqual("Calculator Run 3 already has OrganisationDataMasterId associated with it", objResult.Value);
        }

        [TestMethod]
        public async Task UpdateRpdStatus_With_RunId_Having_PomDataMasterId()
        {
            var pomMaster = new CalculatorRunPomDataMaster
            {
                CalendarYear = "2023",
                CreatedAt = DateTime.Now,
                CreatedBy = "Some User",
                EffectiveFrom = DateTime.Now,
                EffectiveTo = null,
            };

            var pomDataDetail = new CalculatorRunPomDataDetail
            {
                OrganisationId = 1234,
                SubsidaryId = "4455",
                LoadTimeStamp = DateTime.Now,
                CalculatorRunPomDataMaster = pomMaster,
                Id = 0,
                SubmissionPeriod = "some-period",
                CalculatorRunPomDataMasterId = pomMaster.Id,
                SubmissionPeriodDesc = "some-period-desc",
            };

            this.dbContext?.CalculatorRunPomDataDetails.Add(pomDataDetail);

            this.dbContext?.SaveChanges();

            var calcRun = this.dbContext?.CalculatorRuns.Single(run => run.Id == 1);
            if (calcRun != null)
            {
                calcRun.CalculatorRunPomDataMaster = pomMaster;
                calcRun.CalculatorRunPomDataMasterId = pomMaster.Id;
            }

            this.dbContext?.SaveChanges();

            var request = new Dtos.UpdateRpdStatus { isSuccessful = false, RunId = 1, UpdatedBy = "User1" };
            var result = await this.calculatorInternalController.UpdateRpdStatus(request);
            var objResult = result as ObjectResult;
            Assert.IsNotNull(objResult);
            Assert.AreEqual(422, objResult.StatusCode);
            Assert.AreEqual("Calculator Run 1 already has PomDataMasterId associated with it", objResult.Value);
        }

        [TestMethod]
        public async Task UpdateRpdStatus_With_RunId_With_Incorrect_Classification()
        {
            var calcRun = this.dbContext?.CalculatorRuns.Single(x => x.Id == 1);
            if (calcRun != null)
            {
                calcRun.CalculatorRunClassificationId = 3;
                this.dbContext?.CalculatorRuns.Update(calcRun);
            }

            var request = new Dtos.UpdateRpdStatus { isSuccessful = false, RunId = 1, UpdatedBy = "User1" };
            var result = await this.calculatorInternalController.UpdateRpdStatus(request);
            var objResult = result as ObjectResult;
            Assert.IsNotNull(objResult);
            Assert.AreEqual(422, objResult.StatusCode);
            Assert.AreEqual("Calculator Run 1 classification should be RUNNING or IN THE QUEUE", objResult.Value);
        }

        [TestMethod]
        public async Task UpdateRpdStatus_With_RunId_When_Not_Successful()
        {
            var request = new Dtos.UpdateRpdStatus { isSuccessful = false, RunId = 1, UpdatedBy = "User1" };
            var result = await this.calculatorInternalController.UpdateRpdStatus(request);
            var objResult = result as ObjectResult;
            Assert.AreEqual(201, objResult?.StatusCode);
            var updatedRun = this.dbContext?.CalculatorRuns.Single(x => x.Id == 1);
            Assert.IsNotNull(updatedRun);
            Assert.AreEqual(5, updatedRun.CalculatorRunClassificationId);
        }

        [TestMethod]
        public async Task UpdateRpdStatus_With_RunId_Having_Pom_Data_Missing()
        {
            var request = new Dtos.UpdateRpdStatus { isSuccessful = true, RunId = 1, UpdatedBy = "User1" };
            var result = await this.calculatorInternalController.UpdateRpdStatus(request);
            var objResult = result as ObjectResult;
            Assert.AreEqual(422, objResult?.StatusCode);
            Assert.AreEqual("PomData or Organisation Data is missing", objResult?.Value);
        }

        [TestMethod]
        public async Task UpdateRpdStatus_With_RunId_When_Successful()
        {
            var organisation = new OrganisationData
            {
                LoadTimestamp = DateTime.UtcNow,
                OrganisationName = "OrgName",
                OrganisationId = 1345,
                SubsidaryId = "Sub1d1",
                SubmissionPeriodDesc = "some-period-desc"
            };
            var pomData = new PomData
            {
                LoadTimeStamp = DateTime.Now,
                OrganisationId = 1567,
                SubsidaryId = "Sub1d1",
                PackagingType = "packtype",
                PackagingClass = "class1",
                PackagingMaterial = "Aluminium",
                PackagingMaterialWeight = 200,
                SubmissionPeriod = "2024-25",
                SubmissionPeriodDesc = "November 2024 to June 2025"
            };
            var pomDataList = new List<PomData> { pomData };
            var organisationDataList = new List<OrganisationData> { organisation };
            var mock = new Mock<IOrgAndPomWrapper>();
            mock.Setup(x => x.AnyPomData()).Returns(true);
            mock.Setup(x => x.AnyOrganisationData()).Returns(true);
            mock.Setup(x => x.ExecuteSqlAsync(It.IsAny<FormattableString>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(-1);

            if (dbContext != null)
            {
                var controller = new CalculatorInternalController(
                    dbContext,
                    new RpdStatusDataValidator(mock.Object),
                    mock.Object,
                    new Mock<ICalcResultBuilder>().Object,
                    new Mock<ICalcResultsExporter<CalcResult>>().Object,
                    new Mock<ITransposePomAndOrgDataService>().Object,
                    new Mock<IStorageService>().Object,
                    new Mock<CalculatorRunValidator>().Object,
                    new CommandTimeoutService(this.Configuration)
                );
                controller.ControllerContext.HttpContext = new Mock<HttpContext>().Object;

                var request = new Dtos.UpdateRpdStatus { isSuccessful = true, RunId = 1, UpdatedBy = "User1" };
                var result = await controller.UpdateRpdStatus(request);

                var objResult = result as ObjectResult;
                Assert.AreEqual(201, objResult?.StatusCode);
                var calcRun = dbContext.CalculatorRuns.Single(x => x.Id == 1);
                Assert.IsNotNull(calcRun);
                Assert.AreEqual(2, calcRun.CalculatorRunClassificationId);
                mock.Verify(x => x.ExecuteSqlAsync(
                    It.IsAny<FormattableString>(), It.IsAny<CancellationToken>()),
                    Times.Exactly(2));
            }

        }

        [TestMethod]
        public void PrepareCalcResults_ShouldReturnCreatedStatus()
        {
            var requestDto = new CalcResultsRequestDto() { RunId = 4 };
            var calcResult = new CalcResult
            {
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
            var mockStorageService = new Mock<IStorageService>();
            var mockExporter = new Mock<ICalcResultsExporter<CalcResult>>();
            var mockBuilder = new Mock<ICalcResultBuilder>();
            mockExporter.Setup(x => x.Export(It.IsAny<CalcResult>())).Returns("some");
            mockBuilder.Setup(x => x.Build(It.IsAny<CalcResultsRequestDto>())).ReturnsAsync(new CalcResult
            {
                CalcResultDetail = new CalcResultDetail
                {
                    RunId = 4,
                    RunDate = DateTime.Now,
                    RunName = "SomeRun"
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
            });

            var mockTranspose = new Mock<ITransposePomAndOrgDataService>();
            var mockCalcResultBuilder = new Mock<ICalcResultBuilder>();
            var mockValidator = new Mock<CalculatorRunValidator>();

            var controller = new CalculatorInternalController(
               dbContext,
               new RpdStatusDataValidator(wrapper),
               wrapper,
               mockBuilder.Object,
               mockExporter.Object,
               mockTranspose.Object,
               mockStorageService.Object,
               mockValidator.Object,
               new CommandTimeoutService(this.Configuration)
            );
            controller.ControllerContext.HttpContext = new Mock<HttpContext>().Object;

            mockTranspose.Setup(x => x.Transpose(It.IsAny<CalcResultsRequestDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            mockStorageService.Setup(x => x.UploadResultFileContentAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("expected result");

            mockCalcResultBuilder.Setup(b => b.Build(requestDto)).ReturnsAsync(calcResult);
            var task = controller.PrepareCalcResults(requestDto);
            task.Wait();
            var result = task.Result as ObjectResult;
            var calculatorRun = dbContext.CalculatorRuns.SingleOrDefault(run => run.Id == 4);
            Assert.IsNotNull(result);
            Assert.AreEqual((int)RunClassification.UNCLASSIFIED, calculatorRun?.CalculatorRunClassificationId);
            Assert.AreEqual(201, result.StatusCode);
        }

        [TestMethod]
        public void PrepareCalcResults_ShouldReturnNotFound()
        {
            var requestDto = new CalcResultsRequestDto() { RunId = 0 };
            var calcResult = Fixture.Create<CalcResult>();

            var mockCalcResultBuilder = new Mock<ICalcResultBuilder>();
            var controller = new CalculatorInternalController(
                dbContext,
                new RpdStatusDataValidator(wrapper),
                wrapper,
                new Mock<ICalcResultBuilder>().Object,
                new Mock<ICalcResultsExporter<CalcResult>>().Object,
                new Mock<ITransposePomAndOrgDataService>().Object,
                new Mock<IStorageService>().Object,
                new Mock<CalculatorRunValidator>().Object,
                new CommandTimeoutService(this.Configuration)
            );
            controller.ControllerContext.HttpContext = new Mock<HttpContext>().Object;

            mockCalcResultBuilder.Setup(b => b.Build(requestDto)).ReturnsAsync(calcResult);
            var task = controller.PrepareCalcResults(requestDto);
            var result = task.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
        }

        /// <summary>
        /// Checks that when the call to get the calculator runs times out,
        /// the controller returns a 408 status code.
        /// </summary>
        [TestMethod]
        public async Task TransposeBeforeCalcResults_ShouldReturn408_WhenGetCalculatorRunsTimesOut()
        {
            // Arrange
            var controller = new CalculatorInternalController(
                dbContext,
                new RpdStatusDataValidator(wrapper),
                wrapper,
                new Mock<ICalcResultBuilder>().Object,
                new Mock<ICalcResultsExporter<CalcResult>>().Object,
                MockTransposeService.Object,
                new Mock<IStorageService>().Object,
                mockValidator.Object,
                new CommandTimeoutService(this.Configuration)
            );
            var context = new Mock<HttpContext>();
            context.Setup(c => c.RequestAborted).Returns(new CancellationToken(true));
            controller.ControllerContext.HttpContext = context.Object;

            // Act
            var task = await controller.TransposeBeforeCalcResults(new CalcResultsRequestDto() { RunId = 4 });
            var result = (ObjectResult)task;

            // Assert
            Assert.AreEqual((int)HttpStatusCode.RequestTimeout, result.StatusCode);
        }

        /// <summary>
        /// Checks that when <see cref="CalculatorInternalController.TransposeBeforeCalcResults(CalcResultsRequestDto)"/>
        /// calls <see cref="ITransposePomAndOrgDataService.Transpose(CalcResultsRequestDto, CancellationToken)"/>
        /// , but times out, the controller returns a 408 status code.
        /// </summary>
        [TestMethod]
        public async Task TransposeBeforeCalcResults_ShouldReturn408_WhenTransposeTimesOut()
        {
            // Arrange
            var controller = new CalculatorInternalController(
                dbContext,
                new RpdStatusDataValidator(wrapper),
                wrapper,
                new Mock<ICalcResultBuilder>().Object,
                new Mock<ICalcResultsExporter<CalcResult>>().Object,
                MockTransposeService.Object,
                new Mock<IStorageService>().Object,
                mockValidator.Object,
                new CommandTimeoutService(this.Configuration)
            );
            var context = new Mock<HttpContext>();
            context.SetupSequence(c => c.RequestAborted)
                .Returns(new CancellationToken(false))
                .Returns(new CancellationToken(true));
            controller.ControllerContext.HttpContext = context.Object;

            // Act
            var task = await controller.TransposeBeforeCalcResults(new CalcResultsRequestDto() { RunId = 4 });
            var result = (ObjectResult)task;

            // Assert
            Assert.AreEqual((int)HttpStatusCode.RequestTimeout, result.StatusCode);
        }

        /// <summary>
        /// Checks that when <see cref="CalculatorInternalController.TransposeBeforeCalcResults(CalcResultsRequestDto)"/>
        /// calls to get the calculator run, but fails for a reason not covered by other scenarios
        /// (such as a timeout), the controller returns a 500 status code.
        /// </summary>
        [TestMethod]
        public async Task TransposeBeforeCalcResults_ShouldReturn500_WhenGetCalculatorRunsMiscError()
        {
            // Arrange
            var controller = new CalculatorInternalController(
                dbContext,
                new RpdStatusDataValidator(wrapper),
                wrapper,
                new Mock<ICalcResultBuilder>().Object,
                new Mock<ICalcResultsExporter<CalcResult>>().Object,
                new Mock<ITransposePomAndOrgDataService>().Object,
                new Mock<IStorageService>().Object,
                mockValidator.Object,
                new CommandTimeoutService(this.Configuration)
            );
            var context = new Mock<HttpContext>();
            context.SetupSequence(c => c.RequestAborted).Throws(new Exception("A test error occured."));
            controller.ControllerContext.HttpContext = context.Object;

            // Act
            var task = await controller.TransposeBeforeCalcResults(new CalcResultsRequestDto() { RunId = 4 });
            var result = (ObjectResult)task;

            // Assert
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, result.StatusCode);
        }

        /// <summary>
        /// Checks that when <see cref="CalculatorInternalController.TransposeBeforeCalcResults(CalcResultsRequestDto)"/>
        /// calls <see cref="ITransposePomAndOrgDataService.Transpose(CalcResultsRequestDto, CancellationToken)"/>
        /// , but fails for a reason not covered by other scenarios, the controller returns a 500 status code.
        /// </summary>
        [TestMethod]
        public async Task TransposeBeforeCalcResults_ShouldReturn500_WhenTransposeMiscError()
        {
            // Arrange
            var controller = new CalculatorInternalController(
                dbContext,
                new RpdStatusDataValidator(wrapper),
                wrapper,
                new Mock<ICalcResultBuilder>().Object,
                new Mock<ICalcResultsExporter<CalcResult>>().Object,
                MockTransposeService.Object,
                new Mock<IStorageService>().Object,
                mockValidator.Object,
                new CommandTimeoutService(this.Configuration)
            );
            var context = new Mock<HttpContext>();
            context.SetupSequence(c => c.RequestAborted)
                .Returns(new CancellationToken(false))
                .Throws(new Exception("A test error occured."));
            controller.ControllerContext.HttpContext = context.Object;

            // Act
            var task = await controller.TransposeBeforeCalcResults(new CalcResultsRequestDto() { RunId = 4 });
            var result = (ObjectResult)task;

            // Assert
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, result.StatusCode);
        }

        /// <summary>
        /// Checks that when <see cref="CalculatorInternalController.PrepareCalcResults(CalcResultsRequestDto)"/>
        /// requests the calculator runs but times out, the controller returns a 408 status code.
        /// </summary>
        [TestMethod]
        public async Task PrepareCalcResults_ShouldReturn408_WhenGetCalculatorRunsTimesOut()
        {
            // Arrange
            var requestDto = new CalcResultsRequestDto() { RunId = 0 };
            var calcResult = Fixture.Create<CalcResult>();

            var controller = new CalculatorInternalController(
                dbContext,
                new RpdStatusDataValidator(wrapper),
                wrapper,
                new Mock<ICalcResultBuilder>().Object,
                new Mock<ICalcResultsExporter<CalcResult>>().Object,
                new Mock<ITransposePomAndOrgDataService>().Object,
                new Mock<IStorageService>().Object,
                mockValidator.Object,
                new CommandTimeoutService(this.Configuration)
            );
            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(Object => Object.RequestAborted).Returns(new CancellationToken(true));
            controller.ControllerContext.HttpContext = httpContext.Object;


            // Act
            var task = await controller.PrepareCalcResults(requestDto);
            var result = (ObjectResult)task;

            // Assert
            Assert.AreEqual((int)HttpStatusCode.RequestTimeout, result.StatusCode);
        }

        // <summary>
        /// Checks that when that when
        /// <see cref="CalculatorInternalController.PrepareCalcResults(CalcResultsRequestDto)"/>
        /// calls to update the calculator run record, but times out, the controller returns a 408 status code.
        /// </summary>
        [TestMethod]
        public async Task PrepareCalcResults_ShouldReturn408_WhenTransposeTimesOut()
        {
            // Arrange
            var requestDto = new CalcResultsRequestDto() { RunId = 4 };
            var calcResult = Fixture.Create<CalcResult>();

            var mockCalcResultBuilder = new Mock<ICalcResultBuilder>();
            mockCalcResultBuilder.Setup(b => b.Build(requestDto)).ReturnsAsync(calcResult);

            var mockStorage = new Mock<IStorageService>();
            mockStorage.Setup(m => m.UploadResultFileContentAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("expected ");

            var controller = new CalculatorInternalController(
                dbContext,
                new RpdStatusDataValidator(wrapper),
                wrapper,
                mockCalcResultBuilder.Object,
                new Mock<ICalcResultsExporter<CalcResult>>().Object,
                MockTransposeService.Object,
                mockStorage.Object,
                mockValidator.Object,
                new CommandTimeoutService(this.Configuration)
            );
            var httpContext = new Mock<HttpContext>();
            httpContext.SetupSequence(c => c.RequestAborted)
                .Returns(new CancellationToken(false))
                .Returns(new CancellationToken(true));

            controller.ControllerContext.HttpContext = httpContext.Object;

            // Act
            var task = await controller.PrepareCalcResults(requestDto);
            var result = (ObjectResult)task;

            // Assert
            Assert.AreEqual((int)HttpStatusCode.RequestTimeout, result.StatusCode);
        }

        /// <summary>
        /// Checks that when that when
        /// <see cref="CalculatorInternalController.PrepareCalcResults(CalcResultsRequestDto)"/>
        /// calls to update the calculator run record, but times out, the controller returns a 408 status code.
        /// </summary>
        [TestMethod]
        public async Task PrepareCalcResults_ShouldReturn408_WhenUpdateCalculatorRecordTimesOut()
        {
            // Arrange
            var requestDto = new CalcResultsRequestDto() { RunId = 4 };
            var calcResult = Fixture.Create<CalcResult>();

            var mockCalcResultBuilder = new Mock<ICalcResultBuilder>();
            mockCalcResultBuilder.Setup(b => b.Build(requestDto)).ReturnsAsync(calcResult);

            var mockStorage = new Mock<IStorageService>();
            mockStorage.Setup(m => m.UploadResultFileContentAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("expected result");

            var controller = new CalculatorInternalController(
                dbContext,
                new RpdStatusDataValidator(wrapper),
                wrapper,
                mockCalcResultBuilder.Object,
                new Mock<ICalcResultsExporter<CalcResult>>().Object,
                MockTransposeService.Object,
                mockStorage.Object,
                mockValidator.Object,
                new CommandTimeoutService(this.Configuration)
            );
            var httpContext = new Mock<HttpContext>();
            httpContext.SetupSequence(c => c.RequestAborted)
                .Returns(new CancellationToken(false))
                .Returns(new CancellationToken(true));
            controller.ControllerContext.HttpContext = httpContext.Object;

            // Act
            var task = await controller.PrepareCalcResults(requestDto);
            var result = (ObjectResult)task;

            // Assert
            Assert.AreEqual((int)HttpStatusCode.RequestTimeout, result.StatusCode);
        }

        /// <summary>
        /// Checks that when <see cref="CalculatorInternalController.PrepareCalcResults(CalcResultsRequestDto)"/>
        /// calls to get the calculator run, but fails for a reason not covered by other scenarios,
        /// the controller returns a 500 status code.
        /// </summary>
        [TestMethod]
        public async Task PrepareCalcResults_ShouldReturn500_WhenGetCalculatorRunMiscError()
        {
            // Arrange
            var requestDto = new CalcResultsRequestDto() { RunId = 0 };

            var controller = new CalculatorInternalController(
                dbContext,
                new RpdStatusDataValidator(wrapper),
                wrapper,
                new Mock<ICalcResultBuilder>().Object,
                new Mock<ICalcResultsExporter<CalcResult>>().Object,
                MockTransposeService.Object,
                new Mock<IStorageService>().Object,
                mockValidator.Object,
                new CommandTimeoutService(this.Configuration)
            );
            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(Object => Object.RequestAborted)
                .Throws(new Exception("A test error occured."));
            controller.ControllerContext.HttpContext = httpContext.Object;


            // Act
            var task = await controller.PrepareCalcResults(requestDto);
            var result = (ObjectResult)task;

            // Assert
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, result.StatusCode);
        }

        /// <summary>
        /// Checks that when <see cref="CalculatorInternalController.PrepareCalcResults(CalcResultsRequestDto)"/>
        /// calls to update the calculator run record, but fails for a reason not covered by other scenarios,
        /// the controller returns a 500 status code.
        /// </summary>
        [TestMethod]
        public async Task PrepareCalcResults_ShouldReturn500_WhenGetUpdateCalculatorRunMiscError()
        {
            // Arrange
            var requestDto = new CalcResultsRequestDto() { RunId = 4 };
            var calcResult = Fixture.Create<CalcResult>();

            var mockCalcResultBuilder = new Mock<ICalcResultBuilder>();
            mockCalcResultBuilder.Setup(b => b.Build(requestDto)).ReturnsAsync(calcResult);

            var mockStorage = new Mock<IStorageService>();
            mockStorage.Setup(m => m.UploadResultFileContentAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("expected result");

            var controller = new CalculatorInternalController(
                dbContext,
                new RpdStatusDataValidator(wrapper),
                wrapper,
                mockCalcResultBuilder.Object,
                new Mock<ICalcResultsExporter<CalcResult>>().Object,
                MockTransposeService.Object,
                mockStorage.Object,
                mockValidator.Object,
                new CommandTimeoutService(this.Configuration)
            );
            var httpContext = new Mock<HttpContext>();
            httpContext.SetupSequence(Object => Object.RequestAborted)
                .Returns(new CancellationToken(false))
                .Throws(new Exception("A test error occured."));
            controller.ControllerContext.HttpContext = httpContext.Object;


            // Act
            var task = await controller.PrepareCalcResults(requestDto);
            var result = (ObjectResult)task;

            // Assert
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [TestMethod]
        public void FinancialYear_ShouldBeEmpty_WhenCalcRunIsNull()
        {
            CalculatorRun? calcRun = null;

            string financialYear = calcRun?.Financial_Year ?? string.Empty;

            Assert.AreEqual(string.Empty, financialYear);
        }

        [TestMethod]
        public void FinancialYear_ShouldReturnValue_WhenCalcRunIsNotNull()
        {
            CalculatorRun calcRun = new()
            {
                Name = Fixture.Create<string>(),
                Financial_Year = "2024-25"
            };

            string fy = calcRun?.Financial_Year ?? string.Empty;

            Assert.AreEqual("2024-25", fy);
        }

        [TestMethod]
        public void GetCalendarYear_FromValidFinancialYear()
        {
            string financialYear = "2024-25";

            string result = Util.GetCalendarYear(financialYear);

            Assert.AreEqual("2023", result);
        }

        [TestMethod]
        public void GetCalendarYear_InvalidFinancialYear_ThrowsException()
        {
            string financialYear = "InvalidYear";

            var exception = Assert.ThrowsException<FormatException>(() => Util.GetCalendarYear(financialYear));
            Assert.AreEqual("Financial year format is invalid. Expected format is 'YYYY-YY'.", exception.Message);
        }

        [TestMethod]
        public void GetCalendarYear_EmptyFinancialYear_ThrowsException()
        {
            string financialYear = "";

            var exception = Assert.ThrowsException<ArgumentException>(() => Util.GetCalendarYear(financialYear));
            Assert.AreEqual("Financial year cannot be null or empty (Parameter 'financialYear')", exception.Message);
        }
    }
}