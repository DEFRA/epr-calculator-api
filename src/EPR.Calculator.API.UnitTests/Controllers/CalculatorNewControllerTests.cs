using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using AutoFixture;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Services.Abstractions;
using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Wrapper;
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
        private readonly Mock<IOrgAndPomWrapper> mockWrapper;
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
            mockWrapper = new Mock<IOrgAndPomWrapper>();
            mockCalculationRunService = new Mock<ICalculationRunService>();

            var config = TelemetryConfiguration.CreateDefault();
            var telemetryClient = new TelemetryClient(config);

            controller = new CalculatorNewController(
                context,
                mockValidator.Object,
                mockBillingFileService.Object,
                mockWrapper.Object,
                telemetryClient,
                mockCalculationRunService.Object);

            context.CalculatorRuns.Add(new CalculatorRun
            {
                CalculatorRunClassificationId = 8,
                Financial_Year = new CalculatorRunFinancialYear { Name = "2024-25" },
                Name = "Name",
                Id = 1,
            });
            context.CalculatorRuns.Add(new CalculatorRun
            {
                CalculatorRunClassificationId = 3,
                Financial_Year = new CalculatorRunFinancialYear { Name = "2025-26" },
                Name = "Second run",
                Id = 2,
            });
            context.CalculatorRuns.Add(new CalculatorRun
            {
                CalculatorRunClassificationId = 7,
                Financial_Year = new CalculatorRunFinancialYear { Name = "2023-24" },
                Name = "Calc Billing Run Test",
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
                .Setup(x => x.MoveBillingJsonFile(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            mockBillingFileService
               .Setup(x => x.IsBillingFileGeneratedLatest(1, It.IsAny<CancellationToken>()))
               .ReturnsAsync(true);

            context.CalculatorRunBillingFileMetadata.Add(new CalculatorRunBillingFileMetadata
            {
                BillingCsvFileName = "test2.csv",
                BillingJsonFileName = "test2.json",
                BillingFileCreatedBy = "testUser",
                BillingFileCreatedDate = DateTime.UtcNow,
                CalculatorRunId = 1,
            });
            context.SaveChanges();
            var task = controller.PrepareBillingFileSendToFSS(1, CancellationToken.None);
            task.Wait(TestContext.CancellationTokenSource.Token);

            var result = task.Result as StatusCodeResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(202, result.StatusCode);
        }

        [TestMethod]
        public void PrepareBillingFileSendToFSS_SendFile_BillingFileOutdated()
        {
            ControllerContext();

            // Set up the mock to return a value
            mockBillingFileService
                .Setup(x => x.MoveBillingJsonFile(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            mockBillingFileService
                .Setup(x => x.IsBillingFileGeneratedLatest(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            context.CalculatorRunBillingFileMetadata.Add(new CalculatorRunBillingFileMetadata
            {
                BillingCsvFileName = "test2.csv",
                BillingJsonFileName = "test2.json",
                BillingFileCreatedBy = "testUser",
                BillingFileCreatedDate = DateTime.UtcNow,
                CalculatorRunId = 1,
            });
            context.SaveChanges();

            using var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var task = controller.PrepareBillingFileSendToFSS(1, cancellationToken);
            task.Wait(cancellationToken);

            var result = task.Result as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(422, result.StatusCode);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual("Billing file is not the latest one.", result.Value);
        }

        [TestMethod]
        public void PrepareBillingFileSendToFSS_Invalid()
        {
            ControllerContext();
            var task = controller.PrepareBillingFileSendToFSS(-1);
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
            var run = response.Value as CalculatorRunBillingDto;
            Assert.IsNotNull(run);
            Assert.AreEqual(3, run.RunId);
            Assert.AreEqual("INITIAL RUN COMPLETED", run.RunClassificationStatus);
            Assert.AreEqual(7, run.RunClassificationId);
            Assert.IsNull(run.UpdatedAt);
            Assert.IsNull(run.UpdatedBy);
            Assert.AreEqual("test.json", run.BillingJsonFileName);
            Assert.AreEqual("test.csv", run.BillingCsvFileName);
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
                .Setup(x => x.MoveBillingJsonFile(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            mockBillingFileService
               .Setup(x => x.IsBillingFileGeneratedLatest(1, It.IsAny<CancellationToken>()))
               .ReturnsAsync(true);

            context.CalculatorRunBillingFileMetadata.Add(new CalculatorRunBillingFileMetadata
            {
                BillingCsvFileName = "test2.csv",
                BillingJsonFileName = "test2.json",
                BillingFileCreatedBy = "testUser",
                BillingFileCreatedDate = DateTime.UtcNow,
                CalculatorRunId = 1,
            });

            context.SaveChanges();

            // Act
            var task = controller.PrepareBillingFileSendToFSS(1, CancellationToken.None);
            task.Wait(TestContext.CancellationTokenSource.Token);

            // Assert
            var result = task.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(422, result.StatusCode);
            Assert.AreEqual("Unable to move billing json file for Run Id 1", result.Value);

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
        [DataRow(RunClassification.INITIAL_RUN, RunClassification.INITIAL_RUN_COMPLETED)]
        [DataRow(RunClassification.INTERIM_RECALCULATION_RUN, RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED)]
        [DataRow(RunClassification.FINAL_RECALCULATION_RUN, RunClassification.FINAL_RECALCULATION_RUN_COMPLETED)]
        [DataRow(RunClassification.FINAL_RUN, RunClassification.FINAL_RUN_COMPLETED)]
        public async Task PrepareBillingFileSendToFSS_UpdateToCompleted(
            RunClassification initialValue,
            RunClassification expectedNewValue)
        {
            // Arrange
            ControllerContext();

            var calculatorRunId = 1;
            var calculatorRun = context.CalculatorRuns
                .Single(run => run.Id == calculatorRunId);

            calculatorRun.CalculatorRunClassificationId = (int)initialValue;

            // Set up the billings service to report that the file was successfully transfered.
            mockBillingFileService
                .Setup(x => x.MoveBillingJsonFile(calculatorRunId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            mockBillingFileService
               .Setup(x => x.IsBillingFileGeneratedLatest(1, It.IsAny<CancellationToken>()))
               .ReturnsAsync(true);

            context.CalculatorRunBillingFileMetadata.Add(new CalculatorRunBillingFileMetadata
            {
                BillingCsvFileName = "test2.csv",
                BillingJsonFileName = "test2.json",
                BillingFileCreatedBy = "testUser",
                BillingFileCreatedDate = DateTime.UtcNow,
                CalculatorRunId = calculatorRunId,
            });

            context.SaveChanges();

            // Act
            var result = (StatusCodeResult)await controller.PrepareBillingFileSendToFSS(calculatorRunId, CancellationToken.None);
            var newClassification = (RunClassification)context.CalculatorRuns
                .Single(run => run.Id == calculatorRunId).CalculatorRunClassificationId;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.Accepted, (HttpStatusCode)result.StatusCode);
            Assert.AreEqual(expectedNewValue, newClassification);

            context.CalculatorRunBillingFileMetadata.RemoveRange(context.CalculatorRunBillingFileMetadata);
            context.SaveChanges();
        }

        /// <summary>
        /// Checks that when the initial CalculatorRunClassificationId is not one of the valid initial
        /// values, an error is returned, and the classification is not updated.
        /// </summary>
        /// <param name="initialValue">The initial value of the run status.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        [DataRow(RunClassification.RUNNING)]
        [DataRow(RunClassification.TEST_RUN)]
        [DataRow(RunClassification.DELETED)]
        [DataRow(RunClassification.INTHEQUEUE)]
        public async Task PrepareBillingFileSendToFSS_FailWhenInvalidClassification(RunClassification initialValue)
        {
            ControllerContext();

            var calculatorRunId = 1;
            var calculatorRun = context.CalculatorRuns
                .Single(run => run.Id == calculatorRunId);

            calculatorRun.CalculatorRunClassificationId = (int)initialValue;

            mockBillingFileService
               .Setup(x => x.IsBillingFileGeneratedLatest(1, It.IsAny<CancellationToken>()))
               .ReturnsAsync(true);

            // Act
            var result = (ObjectResult)await controller.PrepareBillingFileSendToFSS(1, CancellationToken.None);
            var newClassification = (RunClassification)context.CalculatorRuns
                    .Single(run => run.Id == calculatorRunId).CalculatorRunClassificationId;

            // Assert
            Assert.AreEqual(HttpStatusCode.UnprocessableContent, (HttpStatusCode)result.StatusCode!);
            Assert.AreEqual(initialValue, newClassification);
            var expectedMessage = $"Classification {(RunClassification)calculatorRun.CalculatorRunClassificationId} is not valid to be completed.";
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
