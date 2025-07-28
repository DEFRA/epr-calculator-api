using System.Security.Claims;
using System.Security.Principal;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Services.Abstractions;
using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Wrapper;
using Microsoft.ApplicationInsights;
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
        private readonly Mock<TelemetryClient> mockTelemetryClient;

        private ApplicationDBContext context;
        private CalculatorNewController controller;

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
            this.mockTelemetryClient = new Mock<TelemetryClient>();

            this.controller = new CalculatorNewController(
                this.context,
                this.mockValidator.Object,
                this.mockBillingFileService.Object,
                this.mockOrgAndPomWrapper.Object,
                this.mockTelemetryClient.Object);

            this.context.CalculatorRunClassifications.Add(new CalculatorRunClassification
            {
                Status = "DELETED",
                Id = 6,
                CreatedBy = "SomeUser",
            });
            this.context.CalculatorRunClassifications.Add(new CalculatorRunClassification
            {
                Status = "INITIAL RUN COMPLETED",
                Id = 7,
                CreatedBy = "SomeUser",
            });
            this.context.CalculatorRunClassifications.Add(new CalculatorRunClassification
            {
                Status = "INITIAL RUN",
                Id = 8,
                CreatedBy = "SomeUser",
            });
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
        public void PutCalculatorRunStatus()
        {
            this.mockValidator.Setup(x => x.Validate(
                It.IsAny<CalculatorRun>(),
                It.IsAny<CalculatorRunStatusUpdateDto>())).Returns(new GenericValidationResultDto { IsInvalid = false });

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

            this.controller.ControllerContext = new ControllerContext
            {
                HttpContext = userContext,
            };

            var task = this.controller.PutCalculatorRunStatus(runStatusUpdateDto);
            task.Wait();
            var result = task.Result as StatusCodeResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(201, result.StatusCode);
        }

        [TestMethod]
        public void PutCalculatorRunStatus_Invalid()
        {
            this.mockValidator.Setup(x => x.Validate(
                It.IsAny<CalculatorRun>(),
                It.IsAny<CalculatorRunStatusUpdateDto>()))
                .Returns(new GenericValidationResultDto
                {
                    IsInvalid = true,
                    Errors = new List<string>
                    {
                        "Some error",
                    },
                });

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

            this.controller.ControllerContext = new ControllerContext
            {
                HttpContext = userContext,
            };

            var task = this.controller.PutCalculatorRunStatus(runStatusUpdateDto);
            task.Wait();
            var result = task.Result as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(422, result.StatusCode);
            Assert.IsNotNull(result.Value);
            var errors = result.Value as IEnumerable<string>;
            Assert.IsNotNull(errors);
            Assert.AreEqual("Some error", errors.First());
        }
    }
}
