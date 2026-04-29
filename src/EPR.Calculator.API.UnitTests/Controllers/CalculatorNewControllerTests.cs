using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using AutoFixture;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Services.Abstractions;
using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Wrapper;
using EPR.Calculator.Service.Function.Services;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class CalculatorNewControllerTests
    {
        private readonly Mock<IBillingFileService> mockBillingFileService;
        private readonly Mock<ICalculatorRunStatusDataValidator> mockValidator;
        private readonly Mock<IInvoiceDetailsService> mockInvoiceDetailsService;
        private readonly Mock<ICalculationRunService> mockCalculationRunService;
        private readonly ApplicationDBContext context;
        private readonly CalculatorNewController controller;

        public CalculatorNewControllerTests()
        {
            Fixture = new Fixture();

            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            context = new ApplicationDBContext(dbContextOptions);
            context.Database.EnsureCreated();

            mockValidator = new Mock<ICalculatorRunStatusDataValidator>();
            mockBillingFileService = new Mock<IBillingFileService>();
            mockInvoiceDetailsService = new Mock<IInvoiceDetailsService>();
            mockCalculationRunService = new Mock<ICalculationRunService>();

            var config = TelemetryConfiguration.CreateDefault();
            var telemetryClient = new TelemetryClient(config);

            controller = new CalculatorNewController(
                context,
                mockValidator.Object,
                mockBillingFileService.Object,
                mockInvoiceDetailsService.Object,
                telemetryClient,
                mockCalculationRunService.Object);

            context.CalculatorRuns.Add(new CalculatorRun
            {
                Classification = RunClassification.InitialRun,
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                Id = 1,
            });
            context.CalculatorRuns.Add(new CalculatorRun
            {
                Classification = RunClassification.Unclassified,
                RelativeYear = new RelativeYear(2024),
                Name = "Second run",
                Id = 2,
            });
            context.CalculatorRuns.Add(new CalculatorRun
            {
                Classification = RunClassification.InitialRun,
                RelativeYear = new RelativeYear(2024),
                Name = "Calc Billing Run Test",
                BillingRunStatus = BillingRunStatus.Completed,
                BillingRunStartedAt = DateTime.UtcNow,
                Id = 3,
            });
            context.CalculatorRunBillingFileMetadata.Add(new CalculatorRunBillingFileMetadata
            {
                Id = 1,
                BillingCsvFileName = "test.csv",
                BillingJsonFileName = "test.json",
                BillingFileCreatedBy = "testUser",
                BillingFileAuthorisedDate = DateTime.UtcNow,
                BillingFileAuthorisedBy = "testUser",
                BillingFileCreatedDate = DateTime.UtcNow,
                CalculatorRunId = 3,
            });
            context.SaveChanges();
        }

        public TestContext TestContext { get; set; }

        private Fixture Fixture { get; init; }

        [TestCleanup]
        public void CleanUp()
        {
            context.Database.EnsureDeleted();
        }

        [TestMethod]
        public void PrepareBillingFileSendToFSS_SendFile_Successfully()
        {
            ControllerContext();

            // Set up the mock to return a value
            mockBillingFileService
                .Setup(x => x.WasBillingGeneratedAfterLatestInstructions(3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            mockBillingFileService
                .Setup(x => x.MoveBillingJsonFile(3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var task = controller.PrepareBillingFileSendToFSS(3, CancellationToken.None);
            task.Wait(TestContext.CancellationTokenSource.Token);

            var result = task.Result as StatusCodeResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(202, result.StatusCode);
        }

        [TestMethod]
        public void PrepareBillingFileSendToFSS_Invalid()
        {
            ControllerContext();
            var task = controller.PrepareBillingFileSendToFSS(-1, CancellationToken.None);
            task.Wait(TestContext.CancellationTokenSource.Token);

            var result = task.Result as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual("Invalid Run Id -1", result.Value);
        }

        [TestMethod]
        public async Task GetCalculatorRunWithBillingDetails_Get_Valid_Run()
        {
            ControllerContext();
            var response = await controller.GetCalculatorRun(3) as ObjectResult;

            Assert.IsNotNull(response);
            var run = response.Value as CalculatorRunDto;
            Assert.IsNotNull(run);
            Assert.AreEqual(3, run.RunId);
            Assert.AreEqual(RunClassification.InitialRun, run.RunClassification);
            Assert.IsNull(run.UpdatedAt);
            Assert.IsNull(run.UpdatedBy);
            Assert.IsNotNull(run.CompletedBillingRun);
            Assert.AreEqual("test.json", run.CompletedBillingRun.JsonFileName);
            Assert.AreEqual("test.csv", run.CompletedBillingRun.CsvFileName);
        }

        [TestMethod]
        public async Task GetCalculatorRunWithBillingDetails_Get_NotFound_Run()
        {
            ControllerContext();
            var response = await controller.GetCalculatorRun(5) as ObjectResult;
            Assert.IsNotNull(response);
            Assert.AreEqual(404, response.StatusCode);
            Assert.AreEqual("Unable to find Run Id 5", response.Value);
        }

        [TestMethod]
        public async Task GetCalculatorRunWithBillingDetails_Get_InValid_Run()
        {
            ControllerContext();
            var response = await controller.GetCalculatorRun(-1) as ObjectResult;
            Assert.IsNotNull(response);
            Assert.AreEqual(400, response.StatusCode);
            Assert.AreEqual("Invalid Run Id -1", response.Value);
        }

        [TestMethod]
        public void PrepareBillingFileSendToFSS_MoveBillingJsonFileFails_Returns422()
        {
            ControllerContext();

            // Arrange: runId 1 is valid and meets preconditions
            mockBillingFileService
                .Setup(x => x.WasBillingGeneratedAfterLatestInstructions(3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            mockBillingFileService
                .Setup(x => x.MoveBillingJsonFile(3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var task = controller.PrepareBillingFileSendToFSS(3, CancellationToken.None);
            task.Wait(TestContext.CancellationTokenSource.Token);

            // Assert
            var result = task.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(422, result.StatusCode);
            Assert.AreEqual("Unable to move billing json file for Run Id 3", result.Value);

            context.CalculatorRunBillingFileMetadata.RemoveRange(context.CalculatorRunBillingFileMetadata);
            context.SaveChanges();
        }

        /// <summary>
        /// Checks that the CalculatorRunClassificationId value is updated to the correct "completed" status
        /// when the classification is set to valid initial values.
        /// </summary>
        /// <param name="initialValue">The initial value of the run status.</param>
        /// <param name="expectedNewValue">The value the status should have after the method is run.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        [DataRow(RunClassification.InitialRun, RunClassification.InitialRunCompleted)]
        [DataRow(RunClassification.InterimRecalculationRun, RunClassification.InterimRecalculationRunCompleted)]
        [DataRow(RunClassification.FinalRecalculationRun, RunClassification.FinalRecalculationRunCompleted)]
        [DataRow(RunClassification.FinalRun, RunClassification.FinalRunCompleted)]
        public async Task PrepareBillingFileSendToFSS_UpdateToCompleted(
            RunClassification initialValue,
            RunClassification expectedNewValue)
        {
            // Arrange
            ControllerContext();

            var calculatorRunId = 1;
            var calculatorRun = context.CalculatorRuns
                .Single(run => run.Id == calculatorRunId);

            calculatorRun.Classification = initialValue;
            calculatorRun.BillingRunStatus = BillingRunStatus.Completed;
            calculatorRun.BillingFileMetadata = new CalculatorRunBillingFileMetadata
            {
                BillingFileCreatedDate = DateTime.UtcNow,
                BillingFileCreatedBy = "ignored",
                BillingCsvFileName = "ignored",
                BillingJsonFileName = "ignored"
            };

            // Set up the billings service to report that the file was successfully transfered.
            mockBillingFileService
                .Setup(x => x.MoveBillingJsonFile(calculatorRunId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            mockBillingFileService
                .Setup(x => x.WasBillingGeneratedAfterLatestInstructions(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = (StatusCodeResult)await controller.PrepareBillingFileSendToFSS(calculatorRunId, CancellationToken.None);
            var newClassification = context.CalculatorRuns
                .Single(run => run.Id == calculatorRunId).Classification;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.Accepted, (HttpStatusCode)result.StatusCode);
            Assert.AreEqual(expectedNewValue, newClassification);

            context.CalculatorRunBillingFileMetadata.RemoveRange(context.CalculatorRunBillingFileMetadata);
        }

        /// <summary>
        /// Checks that when the initial CalculatorRunClassificationId is not one of the valid initial
        /// values, an error is returned, and the classification is not updated.
        /// </summary>
        /// <param name="initialValue">The initial value of the run status.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        [DataRow(RunClassification.Running)]
        [DataRow(RunClassification.TestRun)]
        [DataRow(RunClassification.Deleted)]
        [DataRow(RunClassification.None)]
        public async Task PrepareBillingFileSendToFSS_FailWhenInvalidClassification(RunClassification initialValue)
        {
            ControllerContext();

            var calculatorRunId = 1;
            var calculatorRun = context.CalculatorRuns
                .Single(run => run.Id == calculatorRunId);

            calculatorRun.Classification = initialValue;
            calculatorRun.BillingRunStatus = BillingRunStatus.Completed;
            calculatorRun.BillingFileMetadata = new CalculatorRunBillingFileMetadata
            {
                BillingFileCreatedDate = DateTime.UtcNow,
                BillingFileCreatedBy = "ignored",
                BillingCsvFileName = "ignored",
                BillingJsonFileName = "ignored"
            };

            mockBillingFileService
                .Setup(x => x.WasBillingGeneratedAfterLatestInstructions(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = (ObjectResult)await controller.PrepareBillingFileSendToFSS(1, CancellationToken.None);
            var newClassification = context.CalculatorRuns
                    .Single(run => run.Id == calculatorRunId).Classification;

            // Assert
            Assert.AreEqual(HttpStatusCode.UnprocessableContent, (HttpStatusCode)result.StatusCode!);
            Assert.AreEqual(initialValue, newClassification);
            var expectedMessage = $"Classification {calculatorRun.Classification} is not valid to be completed.";
            Assert.AreEqual(expectedMessage, result.Value);

            context.CalculatorRunBillingFileMetadata.RemoveRange(context.CalculatorRunBillingFileMetadata);
            context.SaveChanges();
        }

        private void ControllerContext()
        {
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            var userContext = new DefaultHttpContext()
            {
                User = principal,
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = userContext,
            };
        }
    }
}
