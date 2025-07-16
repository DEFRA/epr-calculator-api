using System.Security.Claims;
using System.Security.Principal;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class PutCalculatorRunStatusTest
    {
        private readonly ApplicationDBContext context;
        private readonly Mock<IConfiguration> mockConfig;
        private readonly Mock<IStorageService> mockStorageService;
        private readonly Mock<IServiceBusService> mockServiceBusService;
        private readonly Mock<ICalcFinancialYearRequestDtoDataValidator> mockValidator;

        public PutCalculatorRunStatusTest()
        {
            this.mockStorageService = new Mock<IStorageService>();
            this.mockConfig = new Mock<IConfiguration>();
            this.mockServiceBusService = new Mock<IServiceBusService>();
            this.mockValidator = new Mock<ICalcFinancialYearRequestDtoDataValidator>();
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            this.context = new ApplicationDBContext(dbContextOptions);
            this.context.Database.EnsureCreated();

            this.FinancialYear24_25 = new CalculatorRunFinancialYear { Name = "2024-25" };
            this.context.FinancialYears.Add(this.FinancialYear24_25);
            this.context.SaveChanges();

            context.CalculatorRuns.AddRange(GetCalculatorRuns());
            context.SaveChanges();
        }

        private CalculatorRunFinancialYear FinancialYear24_25 { get; init; }

        [TestCleanup]
        public void CleanUp()
        {
            this.context.Database.EnsureDeleted();
        }

        [TestMethod]
        public void PutCalculatorRunStatusTest_422()
        {
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);
            var defaultContext = new DefaultHttpContext()
            {
                User = principal,
            };
            var controller =
                new CalculatorController(
                    this.context,
                    this.mockConfig.Object,
                    this.mockStorageService.Object,
                    this.mockServiceBusService.Object,
                    this.mockValidator.Object,
                    Mock.Of<IAvailableClassificationsService>());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = defaultContext,
            };
            var runId = 0;
            var task = controller.PutCalculatorRunStatus(new CalculatorRunStatusUpdateDto
                { ClassificationId = 5, RunId = runId });
            task.Wait();
            var result = task.Result as ObjectResult;
            Assert.IsNotNull(result);

            Assert.AreEqual(422, result.StatusCode);
            Assert.AreEqual($"Unable to find Run Id {runId}", result.Value);
        }

        [TestMethod]
        public void PutCalculatorRunStatusTest_Invalid_Classification_Id()
        {
            var runId = 1;
            var invalidClassificationId = 10;
            var date = DateTime.Now;
            this.context.CalculatorRuns.Add(new CalculatorRun
            {
                Name = "Calc RunName",
                CalculatorRunClassificationId = 2,
                CreatedAt = date,
                CreatedBy = "User23",
                LapcapDataMasterId = 1,
                DefaultParameterSettingMasterId = 1,
                Financial_Year = FinancialYear24_25,
            });
            this.context.SaveChanges();

            var controller =
                new CalculatorController(
                    this.context,
                    this.mockConfig.Object,
                    this.mockStorageService.Object,
                    this.mockServiceBusService.Object,
                    this.mockValidator.Object,
                    Mock.Of<IAvailableClassificationsService>());

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
            var task = controller.PutCalculatorRunStatus(new CalculatorRunStatusUpdateDto
                { ClassificationId = invalidClassificationId, RunId = runId });
            task.Wait();
            var result = task.Result as ObjectResult;
            Assert.IsNotNull(result);

            Assert.AreEqual(422, result.StatusCode);
            Assert.AreEqual($"Unable to find Classification Id {invalidClassificationId}", result.Value);
        }

        [TestMethod]
        public void PutCalculatorRunStatusTest_Valid_Run_Classification_Id()
        {
            var runId = 1;
            var validClassificationId = 5;
            var date = DateTime.Now;
            this.context.CalculatorRuns.Add(new CalculatorRun
            {
                Name = "Calc RunName",
                CalculatorRunClassificationId = 2,
                CreatedAt = date,
                CreatedBy = "User23",
                LapcapDataMasterId = 1,
                DefaultParameterSettingMasterId = 1,
                Financial_Year = FinancialYear24_25,
            });
            this.context.SaveChanges();

            var controller =
                new CalculatorController(
                    this.context,
                    this.mockConfig.Object,
                    this.mockStorageService.Object,
                    this.mockServiceBusService.Object,
                    this.mockValidator.Object,
                    Mock.Of<IAvailableClassificationsService>());

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
            var task = controller.PutCalculatorRunStatus(new CalculatorRunStatusUpdateDto
            { ClassificationId = validClassificationId, RunId = runId });
            task.Wait();
            var result = task.Result as StatusCodeResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(201, result.StatusCode);

            var run = this.context.CalculatorRuns.Single(x => x.Id == 1);
            Assert.IsNotNull(run);

            Assert.AreEqual(5, run.CalculatorRunClassificationId);
        }

        [TestMethod]
        public async Task PutCalculatorRunStatusTest_Unable_To_Change_Classification_Id()
        {
            var runId = 2;
            var classificationId = 5;
            var date = DateTime.Now;
            this.context.CalculatorRuns.Add(new CalculatorRun
            {
                Name = "Calc RunName",
                CalculatorRunClassificationId = classificationId,
                CreatedAt = date,
                CreatedBy = "User23",
                LapcapDataMasterId = 1,
                DefaultParameterSettingMasterId = 1,
                Financial_Year = FinancialYear24_25,
            });
            this.context.SaveChanges();

            var controller =
                new CalculatorController(
                    this.context,
                    this.mockConfig.Object,
                    this.mockStorageService.Object,
                    this.mockServiceBusService.Object,
                    this.mockValidator.Object,
                    Mock.Of<IAvailableClassificationsService>());

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

            var task = await controller.PutCalculatorRunStatus(new CalculatorRunStatusUpdateDto
            { ClassificationId = classificationId, RunId = runId });
            var result = task as ObjectResult;
            Assert.IsNotNull(result);

            Assert.AreEqual(422, result.StatusCode);
            Assert.AreEqual($"RunId {runId} cannot be changed to classification {classificationId}", result.Value);
        }

        private IEnumerable<CalculatorRun> GetCalculatorRuns()
        {
            return new List<CalculatorRun>()
            {
                new CalculatorRun
                {
                    Id = 1,
                    CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                    Name = "Test Run",
                    Financial_Year = FinancialYear24_25,
                    CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                    CreatedBy = "Test User",
                },
                new CalculatorRun
                {
                    Id = 2,
                    CalculatorRunClassificationId = (int)RunClassification.ERROR,
                    Name = "Test Calculated Result",
                    Financial_Year = FinancialYear24_25,
                    CreatedAt = new DateTime(2024, 8, 21, 14, 16, 27, DateTimeKind.Utc),
                    CreatedBy = "Test User",
                },
                new CalculatorRun
                {
                    Id = 3,
                    CalculatorRunClassificationId = (int)RunClassification.UNCLASSIFIED,
                    Name = "Test Run",
                    Financial_Year = FinancialYear24_25,
                    CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                    CreatedBy = "Test User",
                    CalculatorRunOrganisationDataMasterId = 1,
                    CalculatorRunPomDataMasterId = 1,
                },
                new CalculatorRun
                {
                    Id = 4,
                    CalculatorRunClassificationId = (int)RunClassification.DELETED,
                    Name = "Test Calculated Result",
                    Financial_Year = FinancialYear24_25,
                    CreatedAt = new DateTime(2024, 8, 21, 14, 16, 27, DateTimeKind.Utc),
                    CreatedBy = "Test User",
                    CalculatorRunOrganisationDataMasterId = 2,
                    CalculatorRunPomDataMasterId = 2,
                },
            };
        }
    }
}
