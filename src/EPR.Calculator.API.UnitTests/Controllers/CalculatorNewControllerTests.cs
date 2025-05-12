namespace EPR.Calculator.API.UnitTests.Controllers
{
    using System.Security.Claims;
    using System.Security.Principal;
    using EPR.Calculator.API.Controllers;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.API.Dtos;
    using EPR.Calculator.API.Enums;
    using EPR.Calculator.API.UnitTests.Helpers;
    using EPR.Calculator.API.Validators;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CalculatorNewControllerTests
    {
        private Mock<ICalculatorRunStatusDataValidator> mockValidator;
        private ApplicationDBContext context;
        private CalculatorNewController controller;

        [TestInitialize]
        public void SetUp()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: "PayCal")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
            this.context = new ApplicationDBContext(dbContextOptions);
            this.context.Database.EnsureCreated();

            this.mockValidator = new Mock<ICalculatorRunStatusDataValidator>();
            var configs = ConfigurationItems.GetConfigurationValues();
            configs.GetSection("BillingJsonFileName").Value = "Example_sample_message_Producer_billing_file_1.0.json";
            this.controller = new CalculatorNewController(this.context, configs, this.mockValidator.Object);
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
                HasBillingFileGenerated = true,
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
            this.context.SaveChanges();
        }

        [TestCleanup]
        public void CleanUp()
        {
            this.context.Database.EnsureDeleted();
        }

        [TestMethod]
        public void PrepareBillingFileSendToFSS()
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

            var task = this.controller.PrepareBillingFileSendToFSS(1);
            task.Wait();

            var result = task.Result as StatusCodeResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(202, result.StatusCode);
        }

        [TestMethod]
        public void PrepareBillingFileSendToFSS_Invalid()
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

            var task = this.controller.PrepareBillingFileSendToFSS(-1);
            task.Wait();

            var result = task.Result as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(422, result.StatusCode);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual("Unable to find Run Id -1", result.Value);
        }

        [TestMethod]
        public void PrepareBillingFileSendToFSS_NotInitialRun()
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

            var task = this.controller.PrepareBillingFileSendToFSS(2);
            task.Wait();

            var result = task.Result as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(422, result.StatusCode);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual("Run Id 2 classification status is not an 'INITIAL RUN' or 'HasBillingFileGenerated' column is not set to true", result.Value);
        }
    }
}