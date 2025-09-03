using System.Security.Claims;
using System.Security.Principal;
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class PutCalculatorRunStatusNewTest
    {
        private readonly Mock<ICalculatorRunStatusDataValidator> mockValidator;
        private readonly Mock<IBillingFileService> mockBillingFileService;
        private readonly Mock<IOrgAndPomWrapper> mockOrgAndPomWrapper;
        private readonly Mock<ICalculationRunService> mockCalculationRunService;
        private readonly ApplicationDBContext context;

        private readonly CalculatorNewController calculatorNewControllerUnderTest;

        public PutCalculatorRunStatusNewTest()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            this.context = new ApplicationDBContext(dbContextOptions);
            this.context.Database.EnsureCreated();

            this.mockValidator = new Mock<ICalculatorRunStatusDataValidator>();
            this.mockBillingFileService = new Mock<IBillingFileService>();
            this.mockOrgAndPomWrapper = new Mock<IOrgAndPomWrapper>();
            this.mockCalculationRunService = new Mock<ICalculationRunService>();

            var config = TelemetryConfiguration.CreateDefault();
            var telemetryClient = new TelemetryClient(config);

            this.calculatorNewControllerUnderTest = new CalculatorNewController(
                this.context,
                this.mockValidator.Object,
                this.mockBillingFileService.Object,
                this.mockOrgAndPomWrapper.Object,
                telemetryClient,
                this.mockCalculationRunService.Object);

            this.context.CalculatorRuns.Add(new CalculatorRun
            {
                Financial_Year = new CalculatorRunFinancialYear { Name = "2024-25" },
                Name = "Name",
                Id = 1,
            });
            this.context.SaveChanges();
        }

        [TestCleanup]
        public void CleanUp()
        {
            this.context.Database.EnsureDeleted();
        }

        [TestMethod]
        public void CallingPutCalculatorRunStatusMethod_ShouldReturn201SuccessCode_WhenAllValidationPassed()
        {
            // Act
            var runStatusUpdateDto = new API.Dtos.CalculatorRunStatusUpdateDto
            {
                ClassificationId = 6,
                RunId = 1,
            };

            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            var userContext = new DefaultHttpContext()
            {
                User = principal,
            };

            List<ClassifiedCalculatorRunDto> designatedRuns = [];

            this.calculatorNewControllerUnderTest.ControllerContext = new ControllerContext
            {
                HttpContext = userContext,
            };

            // Setup
            this.mockValidator.Setup(
                x => x.Validate(
                    It.IsAny<CalculatorRun>(),
                    It.IsAny<CalculatorRunStatusUpdateDto>()))
                .Returns(new GenericValidationResultDto { IsInvalid = false });
            this.mockCalculationRunService.Setup(
                x => x.GetDesignatedRunsByFinanialYear(
                    It.IsAny<string>(),
                    default)).ReturnsAsync(designatedRuns);
            this.mockValidator.Setup(
                x => x.Validate(
                    designatedRuns,
                    It.IsAny<CalculatorRun>(),
                    It.IsAny<CalculatorRunStatusUpdateDto>()))
                .Returns(new GenericValidationResultDto { IsInvalid = false });

            // Act
            var task = this.calculatorNewControllerUnderTest.PutCalculatorRunStatus(runStatusUpdateDto);
            task.Wait();

            // Assert
            var result = task.Result as StatusCodeResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(201, result.StatusCode);

            // Veify
            this.mockValidator.Verify(
                x => x.Validate(
                    It.IsAny<CalculatorRun>(),
                    It.IsAny<CalculatorRunStatusUpdateDto>()),
                Times.Once());
            this.mockCalculationRunService.Verify(
                x => x.GetDesignatedRunsByFinanialYear(
                    It.IsAny<string>(),
                    default),
                Times.Once());
            this.mockValidator.Verify(
                x => x.Validate(
                    designatedRuns,
                    It.IsAny<CalculatorRun>(),
                    It.IsAny<CalculatorRunStatusUpdateDto>()),
                Times.Once());
        }

        [TestMethod]
        public void CallingPutCalculatorRunStatusMethod_ShouldReturn422FailureCode_WhenClassificationRunValidationFailed()
        {
            // Arrange
            var runStatusUpdateDto = new API.Dtos.CalculatorRunStatusUpdateDto
            {
                ClassificationId = 6,
                RunId = 1,
            };

            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            var userContext = new DefaultHttpContext()
            {
                User = principal,
            };

            this.calculatorNewControllerUnderTest.ControllerContext = new ControllerContext
            {
                HttpContext = userContext,
            };

            // Setup
            this.mockValidator.Setup(
                x => x.Validate(
                    It.IsAny<CalculatorRun>(),
                    It.IsAny<CalculatorRunStatusUpdateDto>()))
                .Returns(new GenericValidationResultDto
                {
                    IsInvalid = true,
                    Errors =
                    [
                        "Some error",
                    ],
                });

            // Act
            var task = this.calculatorNewControllerUnderTest.PutCalculatorRunStatus(runStatusUpdateDto);
            task.Wait();

            // Assert
            var result = task.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(422, result.StatusCode);
            Assert.IsNotNull(result.Value);
            var errors = result.Value as IEnumerable<string>;
            Assert.IsNotNull(errors);
            Assert.AreEqual("Some error", errors.First());

            // Veify
            this.mockValidator.Verify(
                x => x.Validate(
                    It.IsAny<CalculatorRun>(),
                    It.IsAny<CalculatorRunStatusUpdateDto>()),
                Times.Once());
            this.mockCalculationRunService.Verify(
                x => x.GetDesignatedRunsByFinanialYear(
                    It.IsAny<string>(),
                    default),
                Times.Never());
            this.mockValidator.Verify(
                x => x.Validate(
                    It.IsAny<List<ClassifiedCalculatorRunDto>>(),
                    It.IsAny<CalculatorRun>(),
                    It.IsAny<CalculatorRunStatusUpdateDto>()),
                Times.Never());
        }

        [TestMethod]
        public void CallingPutCalculatorRunStatusMethod_ShouldReturn422FailureCode_WhenOtherRunsClassificationsStatusValidationFailed()
        {
            // Arrange
            var runStatusUpdateDto = new API.Dtos.CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)RunClassification.INITIAL_RUN,
                RunId = 1,
            };

            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            var userContext = new DefaultHttpContext()
            {
                User = principal,
            };

            List<ClassifiedCalculatorRunDto> designatedRuns = [];
            designatedRuns.Add(new ClassifiedCalculatorRunDto
            {
                RunId = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                RunName = "Run 2",
                RunClassificationId = (int)RunClassification.INITIAL_RUN,
                RunClassificationStatus = "INITIAL RUN",
                UpdatedAt = DateTime.UtcNow.AddMinutes(-1),
            });

            this.calculatorNewControllerUnderTest.ControllerContext = new ControllerContext
            {
                HttpContext = userContext,
            };

            // Setup
            this.mockValidator.Setup(
                x => x.Validate(
                    It.IsAny<CalculatorRun>(),
                    It.IsAny<CalculatorRunStatusUpdateDto>()))
                .Returns(new GenericValidationResultDto { IsInvalid = false });
            this.mockCalculationRunService.Setup(
                x => x.GetDesignatedRunsByFinanialYear(
                    It.IsAny<string>(),
                    default)).ReturnsAsync(designatedRuns);
            this.mockValidator.Setup(
                x => x.Validate(
                    designatedRuns,
                    It.IsAny<CalculatorRun>(),
                    It.IsAny<CalculatorRunStatusUpdateDto>()))
                .Returns(new GenericValidationResultDto
                {
                    IsInvalid = true,
                    Errors =
                    [
                        "Some error",
                    ],
                });

            // Act
            var task = this.calculatorNewControllerUnderTest.PutCalculatorRunStatus(runStatusUpdateDto);
            task.Wait();

            // Assert
            var result = task.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(422, result.StatusCode);
            Assert.IsNotNull(result.Value);
            var errors = result.Value as IEnumerable<string>;
            Assert.IsNotNull(errors);
            Assert.AreEqual("Some error", errors.First());

            // Veify
            this.mockValidator.Verify(
                x => x.Validate(
                    It.IsAny<CalculatorRun>(),
                    It.IsAny<CalculatorRunStatusUpdateDto>()),
                Times.Once());
            this.mockCalculationRunService.Verify(
                x => x.GetDesignatedRunsByFinanialYear(
                    It.IsAny<string>(),
                    default),
                Times.Once());
            this.mockValidator.Verify(
                x => x.Validate(
                    designatedRuns,
                    It.IsAny<CalculatorRun>(),
                    It.IsAny<CalculatorRunStatusUpdateDto>()),
                Times.Once());
        }
    }
}
