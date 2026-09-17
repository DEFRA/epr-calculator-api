using System.Security.Claims;
using System.Security.Principal;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class PutCalculatorRunStatusNewTest
    {
        private readonly Mock<IRunClassificationValidator> mockValidator;
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

            this.mockValidator = new Mock<IRunClassificationValidator>();

            this.calculatorNewControllerUnderTest = new CalculatorNewController(
                this.context,
                this.mockValidator.Object,
                Mock.Of<IBillingFileService>(),
                Mock.Of<IInvoiceDetailsService>(),
                Mock.Of<ILogger<CalculatorNewController>>());

            this.context.CalculatorRuns.Add(new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                Id = 1,
            });
            this.context.SaveChanges();
        }

        public TestContext TestContext { get; set; }

        [TestCleanup]
        public void CleanUp()
        {
            this.context.Database.EnsureDeleted();
        }

        [TestMethod]
        public void CallingPutCalculatorRunStatusMethod_ShouldReturn201SuccessCode_WhenAllValidationPassed()
        {
            // Act
            var request = new SetRunClassificationRequest
            {
                ClassificationId = RunClassification.Initial,
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
                x => x.ValidateAsync(
                    It.IsAny<CalculatorRun>(),
                    It.IsAny<RunClassification>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GenericValidationResultDto());

            // Act
            var task = this.calculatorNewControllerUnderTest.PutCalculatorRunStatus(request, CancellationToken.None);
            task.Wait(TestContext.CancellationTokenSource.Token);

            // Assert
            var result = task.Result as StatusCodeResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(201, result.StatusCode);

            this.mockValidator.Verify(
                x => x.ValidateAsync(
                    It.IsAny<CalculatorRun>(),
                    request.ClassificationId,
                    It.IsAny<CancellationToken>()),
                Times.Once());
        }

        [TestMethod]
        public void CallingPutCalculatorRunStatusMethod_ShouldReturn422FailureCode_WhenClassificationRunValidationFailed()
        {
            // Arrange
            var request = new SetRunClassificationRequest
            {
                ClassificationId = RunClassification.Initial,
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
                x => x.ValidateAsync(
                    It.IsAny<CalculatorRun>(),
                    It.IsAny<RunClassification>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GenericValidationResultDto
                {
                    Errors = [ "Some error" ],
                });

            // Act
            var task = this.calculatorNewControllerUnderTest.PutCalculatorRunStatus(request, CancellationToken.None);
            task.Wait(TestContext.CancellationTokenSource.Token);

            // Assert
            var result = task.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(422, result.StatusCode);
            Assert.IsNotNull(result.Value);
            var errors = result.Value as IEnumerable<string>;
            Assert.IsNotNull(errors);
            Assert.AreEqual("Some error", errors.First());

            this.mockValidator.Verify(
                x => x.ValidateAsync(
                    It.IsAny<CalculatorRun>(),
                    request.ClassificationId,
                    It.IsAny<CancellationToken>()),
                Times.Once());
        }
    }
}
