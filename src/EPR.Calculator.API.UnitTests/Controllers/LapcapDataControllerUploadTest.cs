using System.Security.Claims;
using System.Security.Principal;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

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
                new CalculatorRunRelativeYear { Value = 2029 },
                new CalculatorRunRelativeYear { Value = 2030 });

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
                UniqueReference = CommonResources.LapcapDataUniqueReferences.Split(',')[0]
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
                UniqueReference = CommonResources.LapcapDataUniqueReferences.Split(',')[0]
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

            var lapcapDataValidator = new Mock<ILapcapDataValidator>();
            lapcapDataValidator.Setup(x => x.Validate(It.IsAny<CreateLapcapDataDto>()))
                .Returns(new ValidationResultDto<CreateLapcapDataErrorDto> { IsInvalid = false });

            var controller = new LapcapDataController(dbContext, lapcapDataValidator.Object, TelemetryClient)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext }
            };

            // -----------------------------
            // Act: create new Lapcap data for 2029
            // -----------------------------
            var request = new CreateLapcapDataDto
            {
                RelativeYear = new RelativeYear(2029),
                LapcapFileName = "Test File",
                LapcapDataTemplateValues = new List<LapcapDataTemplateValueDto>()
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
            Assert.AreEqual(2, activeLapcap.Count); // only 2029 and 2030 active

            Assert.IsNotNull(dbContext.LapcapDataMaster.Single(x => x.RelativeYearValue == 2029 && x.EffectiveTo == null));
            Assert.IsNotNull(dbContext.LapcapDataMaster.Single(x => x.RelativeYearValue == 2030 && x.EffectiveTo == null));
        }

        [TestMethod]
        public void Test_With_Incorrect_RelativeYears()
        {
            // Arrange: in-memory DB
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            using var dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();

            // Arrange: user identity
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);
            var context = new DefaultHttpContext { User = principal };

            // Seed relative years (realistic values)
            dbContext.AddRange(
                new CalculatorRunRelativeYear { Value = 2029 },
                new CalculatorRunRelativeYear { Value = 2030 });

            dbContext.SaveChanges();

            // Seed minimal LapcapDataTemplateMaster so controller Single() won't throw
            dbContext.LapcapDataTemplateMaster.Add(new LapcapDataTemplateMaster
            {
                Material = "M1",
                Country = "C1",
                UniqueReference = "REF1"
            });
            dbContext.SaveChanges();

            // Seed existing LapcapDataMasters
            var lapcapMaster29 = new LapcapDataMaster
            {
                EffectiveFrom = DateTime.UtcNow.AddYears(-1),
                EffectiveTo = null,
                RelativeYear = new RelativeYear(2029),
                LapcapFileName = "Existing29"
            };
            var lapcapDetail29 = new LapcapDataDetail
            {
                LapcapDataMaster = lapcapMaster29,
                UniqueReference = "REF1",
                TotalCost = 100
            };

            var lapcapMaster30 = new LapcapDataMaster
            {
                EffectiveFrom = DateTime.UtcNow.AddYears(-1),
                EffectiveTo = null,
                RelativeYear = new RelativeYear(2030),
                LapcapFileName = "Existing30"
            };
            var lapcapDetail30 = new LapcapDataDetail
            {
                LapcapDataMaster = lapcapMaster30,
                UniqueReference = "REF1",
                TotalCost = 100
            };

            dbContext.LapcapDataMaster.AddRange(lapcapMaster29, lapcapMaster30);
            dbContext.LapcapDataDetail.AddRange(lapcapDetail29, lapcapDetail30);
            dbContext.SaveChanges();

            // Mock validator
            var lapcapDataValidator = new Mock<ILapcapDataValidator>();
            lapcapDataValidator.Setup(x => x.Validate(It.IsAny<CreateLapcapDataDto>()))
                .Returns(new ValidationResultDto<CreateLapcapDataErrorDto> { IsInvalid = false });

            var controller = new LapcapDataController(dbContext, lapcapDataValidator.Object, TelemetryClient)
            {
                ControllerContext = new ControllerContext { HttpContext = context }
            };

            // Act: use a year that doesn't exist in DB
            var request = new CreateLapcapDataDto
            {
                RelativeYear = new RelativeYear(2035), // deliberately missing
                LapcapFileName = "Some Name",
                LapcapDataTemplateValues = new List<LapcapDataTemplateValueDto>
                {
                    new LapcapDataTemplateValueDto
                    {
                        Material = "M1",
                        CountryName = "C1",
                        TotalCost = "£1000"
                    }
                }
            };

            var result = controller.Create(request).Result;

            // Assert
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(400, objectResult.StatusCode);
            Assert.AreEqual(CommonResources.NoDataForSpecifiedYear, objectResult.Value);

            // Ensure existing data is untouched
            var lapcapLatest = dbContext.LapcapDataMaster.Where(x => x.EffectiveTo == null).ToList();
            Assert.HasCount(2, lapcapLatest);
            Assert.IsNotNull(dbContext.LapcapDataMaster.Single(x => x.RelativeYearValue == 2029 && x.EffectiveTo == null));
            Assert.IsNotNull(dbContext.LapcapDataMaster.Single(x => x.RelativeYearValue == 2030 && x.EffectiveTo == null));
        }
    }
}
