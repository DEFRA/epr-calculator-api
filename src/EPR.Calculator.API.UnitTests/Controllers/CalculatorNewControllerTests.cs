using EPR.Calculator.API.Services.Abstractions;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    using System.Security.Claims;
    using System.Security.Principal;
    using EPR.Calculator.API.Controllers;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.API.Dtos;
    using EPR.Calculator.API.Validators;
    using EPR.Calculator.API.Wrapper;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CalculatorNewControllerTests
    {
        private readonly Mock<IBillingFileService> mockBillingFileService;
        private readonly Mock<ICalculatorRunStatusDataValidator> mockValidator;
        private readonly Mock<IInvoiceDetailsWrapper> mockInvoiceDetailsWrapper;
        private ApplicationDBContext context;
        private CalculatorNewController controller;

        public CalculatorNewControllerTests()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            this.context = new ApplicationDBContext(dbContextOptions);
            this.context.Database.EnsureCreated();

            this.mockValidator = new Mock<ICalculatorRunStatusDataValidator>();
            this.mockBillingFileService = new Mock<IBillingFileService>();
            this.mockInvoiceDetailsWrapper = new Mock<IInvoiceDetailsWrapper>();

            this.controller = new CalculatorNewController(this.context, this.mockValidator.Object, this.mockBillingFileService.Object, this.mockInvoiceDetailsWrapper.Object);
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
                CalculatorRunClassificationId = 8,
                Financial_Year = new CalculatorRunFinancialYear { Name = "2024-25" },
                Name = "Name",
                Id = 1,
            });
            this.context.CalculatorRuns.Add(new CalculatorRun
            {
                CalculatorRunClassificationId = 3,
                Financial_Year = new CalculatorRunFinancialYear { Name = "2025-26" },
                Name = "Second run",
                Id = 2,
            });
            this.context.CalculatorRuns.Add(new CalculatorRun
            {
                CalculatorRunClassificationId = 7,
                Financial_Year = new CalculatorRunFinancialYear { Name = "2023-24" },
                Name = "Calc Billing Run Test",
                Id = 3,
            });
            this.context.CalculatorRunBillingFileMetadata.Add(new CalculatorRunBillingFileMetadata
            {
                Id = 1,
                BillingCsvFileName = "test.csv",
                BillingJsonFileName = "test.json",
                BillingFileCreatedBy = "testUser",
                BillingFileAuthorisedDate = DateTime.Now,
                BillingFileAuthorisedBy = "testUser",
                BillingFileCreatedDate = DateTime.Now,
                CalculatorRunId = 3,
            });
            this.context.SaveChanges();
        }

        [TestCleanup]
        public void CleanUp()
        {
            this.context.Database.EnsureDeleted();
        }

        [TestMethod]
        public void PrepareBillingFileSendToFSS_SendFile_Successfully()
        {
            this.ControllerContext();

            // Set up the mock to return a value
            this.mockBillingFileService
                .Setup(x => x.MoveBillingJsonFile(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            this.context.CalculatorRunBillingFileMetadata.Add(new CalculatorRunBillingFileMetadata
            {
                BillingCsvFileName = "test2.csv",
                BillingJsonFileName = "test2.json",
                BillingFileCreatedBy = "testUser",
                BillingFileCreatedDate = DateTime.Now,
                CalculatorRunId = 1,
            });
            this.context.SaveChanges();
            var task = this.controller.PrepareBillingFileSendToFSS(1);
            task.Wait();

            var result = task.Result as StatusCodeResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(202, result.StatusCode);
        }

        [TestMethod]
        public void PrepareBillingFileSendToFSS_Invalid()
        {
            this.ControllerContext();
            var task = this.controller.PrepareBillingFileSendToFSS(-1);
            task.Wait();

            var result = task.Result as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual("Invalid Run Id -1", result.Value);
        }

        [TestMethod]
        public async Task GetCalculatorRunWithBillingDetails_Get_Valid_Run()
        {
            this.ControllerContext();
            var response = await this.controller.GetCalculatorRun(3) as ObjectResult;

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
            this.ControllerContext();
            var response = await this.controller.GetCalculatorRun(5) as ObjectResult;
            Assert.IsNotNull(response);
            Assert.AreEqual(404, response.StatusCode);
            Assert.AreEqual("Unable to find Run Id 5", response.Value);
        }

        [TestMethod]
        public async Task GetCalculatorRunWithBillingDetails_Get_InValid_Run()
        {
            this.ControllerContext();
            var response = await this.controller.GetCalculatorRun(-1) as ObjectResult;
            Assert.IsNotNull(response);
            Assert.AreEqual(400, response.StatusCode);
            Assert.AreEqual("Invalid Run Id -1", response.Value);
        }

        [TestMethod]
        public void PrepareBillingFileSendToFSS_MoveBillingJsonFileFails_Returns422()
        {
            this.ControllerContext();

            // Arrange: runId 1 is valid and meets preconditions
            this.mockBillingFileService
                .Setup(x => x.MoveBillingJsonFile(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            this.context.CalculatorRunBillingFileMetadata.Add(new CalculatorRunBillingFileMetadata
            {
                BillingCsvFileName = "test2.csv",
                BillingJsonFileName = "test2.json",
                BillingFileCreatedBy = "testUser",
                BillingFileCreatedDate = DateTime.Now,
                CalculatorRunId = 1,
            });

            this.context.SaveChanges();

            // Act
            var task = this.controller.PrepareBillingFileSendToFSS(1);
            task.Wait();

            // Assert
            var result = task.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(422, result.StatusCode);
            Assert.AreEqual("Unable to move billing json file for Run Id 1", result.Value);

            this.context.CalculatorRunBillingFileMetadata.RemoveRange(this.context.CalculatorRunBillingFileMetadata);
            this.context.SaveChanges();
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

            this.controller.ControllerContext = new ControllerContext
            {
                HttpContext = userContext,
            };
        }
    }
}
