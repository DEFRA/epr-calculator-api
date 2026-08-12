using System.Security.Claims;
using System.Security.Principal;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class LapcapDataControllerUploadTest : BaseControllerTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Test_With_Multiple_RelativeYears()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // unique DB per test
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();

            // Add relative years
            dbContext.AddRange(
                new CalculatorRunRelativeYear { Value = new RelativeYear(2029) },
                new CalculatorRunRelativeYear { Value = new RelativeYear(2030) });

            dbContext.SaveChanges();

            // Add existing Lapcap data for 2029 and 2030
            var lapcapMaster29 = new LapcapDataMaster
            {
                EffectiveFrom = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Local),
                EffectiveTo = null,
                RelativeYear = new RelativeYear(2029),
            };
            var lapcapDetail29 = new LapcapDataDetail
            {
                LapcapDataMaster = lapcapMaster29,
                UniqueReference = "ENG-AL"
            };

            var lapcapMaster30 = new LapcapDataMaster
            {
                EffectiveFrom = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Local),
                EffectiveTo = null,
                RelativeYear = new RelativeYear(2030),
            };
            var lapcapDetail30 = new LapcapDataDetail
            {
                LapcapDataMaster = lapcapMaster30,
                UniqueReference = "ENG-AL"
            };

            dbContext.LapcapDataMaster.AddRange(lapcapMaster29, lapcapMaster30);
            dbContext.LapcapDataDetail.AddRange(lapcapDetail29, lapcapDetail30);
            dbContext.SaveChanges();

            // -----------------------------
            // Arrange Controller with Authorized User
            // -----------------------------
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            identity.AddClaim(new Claim(ClaimTypes.Role, "Admin")); // ensure authorization passes
            var principal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext { User = principal };

            var controller = new LapcapDataController(dbContext)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext }
            };

            // -----------------------------
            // Act: create new Lapcap data for 2029
            // -----------------------------
            var request = new CreateLapcapDataRequest
            {
                RelativeYear = new RelativeYear(2029),
                Filename = "Test File",
                Values = []
            };

            var result = controller.Create(request).Result;

            // -----------------------------
            // Assert
            // -----------------------------
            Assert.IsNotNull(result);

            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(201, objectResult.StatusCode); // success

            var activeLapcap = dbContext.LapcapDataMaster.Where(x => x.EffectiveTo == null).ToList();
            Assert.HasCount(2, activeLapcap); // only 2029 and 2030 active

            Assert.IsNotNull(dbContext.LapcapDataMaster.Single(x => x.RelativeYear == 2029 && x.EffectiveTo == null));
            Assert.IsNotNull(dbContext.LapcapDataMaster.Single(x => x.RelativeYear == 2030 && x.EffectiveTo == null));
        }
    }
}
