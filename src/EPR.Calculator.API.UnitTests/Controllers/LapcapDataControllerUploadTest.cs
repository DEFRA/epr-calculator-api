using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Validators;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Security.Claims;
using System.Security.Principal;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class LapcapDataControllerUploadTest
    {
        private LapcapDataController LapcapDataController { get; set; }

        [TestMethod]
        public void Test_With_Multiple_Financial_Years()
        {
            ApplicationDBContext dbContext;
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();

            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext()
            {
                User = principal,
            };

            var year24 = new CalculatorRunFinancialYear
            {
                Name = "2029-30",
                Description = ""
            };
            dbContext.Add(year24);

            var year25 = new CalculatorRunFinancialYear
            {
                Name = "2030-31",
                Description = ""
            };
            dbContext.Add(year25);

            var lapcapMaster25 = new LapcapDataMaster {
                ProjectionYearId = "2029-30",
                EffectiveFrom = new DateTime(2025, 1, 1),
                EffectiveTo = null,
                ProjectionYear = year24,
            };
            var lapcapDetail25 = new LapcapDataDetail
            {
                LapcapDataMaster = lapcapMaster25,
                UniqueReference = LapcapDataUniqueReferences.UniqueReferences.First(),
            };

            var lapcapMaster26 = new LapcapDataMaster
            {
                ProjectionYearId = "2030-31",
                EffectiveFrom = new DateTime(2025, 1, 1),
                EffectiveTo = null,
                ProjectionYear = year25,
            };
            var lapcapDetail26 = new LapcapDataDetail
            {
                LapcapDataMaster = lapcapMaster26,
                UniqueReference = LapcapDataUniqueReferences.UniqueReferences.First(),
            };
            dbContext.LapcapDataMaster.Add(lapcapMaster25);
            dbContext.LapcapDataDetail.Add(lapcapDetail25);
            dbContext.LapcapDataMaster.Add(lapcapMaster26);
            dbContext.LapcapDataDetail.Add(lapcapDetail26);
            dbContext.SaveChanges();

            var lapcapDataValidator = new Mock<ILapcapDataValidator>();
            lapcapDataValidator.Setup(x => x.Validate(It.IsAny<CreateLapcapDataDto>()))
                .Returns(new ValidationResultDto<CreateLapcapDataErrorDto> { IsInvalid = false });
            this.LapcapDataController = new LapcapDataController(
                dbContext,
                lapcapDataValidator.Object,
                new TelemetryClient())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = context,
                },
            };

            var request = new CreateLapcapDataDto
            {
                ParameterYear = "2029-30",
                LapcapFileName = "Some Name",
                LapcapDataTemplateValues = new List<LapcapDataTemplateValueDto>(),
            };
            var task = this.LapcapDataController.Create(request);
            task.Wait();
            var result = task.Result;
            Assert.IsNotNull(result);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(201, objectResult.StatusCode);

            var lapcapLatest = dbContext.LapcapDataMaster.Where(x => x.EffectiveTo == null).ToList();
            Assert.AreEqual(2, lapcapLatest.Count);
            Assert.IsNotNull(dbContext.LapcapDataMaster.Single(x => x.ProjectionYearId == "2029-30" && x.EffectiveTo == null));
            Assert.IsNotNull(dbContext.LapcapDataMaster.Single(x => x.ProjectionYearId == "2030-31" && x.EffectiveTo == null));

            var lapcapDataList = dbContext.LapcapDataDetail.ToList();
            foreach (var lapcapData in lapcapDataList)
            {
                dbContext.LapcapDataDetail.Remove(lapcapData);
            }

            var lapcapDataMasterList = dbContext.LapcapDataMaster.ToList();
            foreach (var lapcapDataMaster in lapcapDataMasterList)
            {
                dbContext.LapcapDataMaster.Remove(lapcapDataMaster);
            }

            dbContext.SaveChanges();
        }

        [TestMethod]
        public void Test_With_Incorrect_Financial_Years()
        {
            ApplicationDBContext dbContext;
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();

            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext()
            {
                User = principal,
            };

            var year24 = new CalculatorRunFinancialYear
            {
                Name = "202930",
                Description = ""
            };
            dbContext.Add(year24);

            var year25 = new CalculatorRunFinancialYear
            {
                Name = "203031",
                Description = ""
            };
            dbContext.Add(year25);

            var lapcapMaster25 = new LapcapDataMaster
            {
                ProjectionYearId = "202930",
                EffectiveFrom = new DateTime(2025, 1, 1),
                EffectiveTo = null,
                ProjectionYear = year24,
            };
            var lapcapDetail25 = new LapcapDataDetail
            {
                LapcapDataMaster = lapcapMaster25,
                UniqueReference = LapcapDataUniqueReferences.UniqueReferences.First(),
            };

            var lapcapMaster26 = new LapcapDataMaster
            {
                ProjectionYearId = "203031",
                EffectiveFrom = new DateTime(2025, 1, 1),
                EffectiveTo = null,
                ProjectionYear = year25,
            };
            var lapcapDetail26 = new LapcapDataDetail
            {
                LapcapDataMaster = lapcapMaster26,
                UniqueReference = LapcapDataUniqueReferences.UniqueReferences.First(),
            };
            dbContext.LapcapDataMaster.Add(lapcapMaster25);
            dbContext.LapcapDataDetail.Add(lapcapDetail25);
            dbContext.LapcapDataMaster.Add(lapcapMaster26);
            dbContext.LapcapDataDetail.Add(lapcapDetail26);
            dbContext.SaveChanges();

            var lapcapDataValidator = new Mock<ILapcapDataValidator>();
            lapcapDataValidator.Setup(x => x.Validate(It.IsAny<CreateLapcapDataDto>()))
                .Returns(new ValidationResultDto<CreateLapcapDataErrorDto> { IsInvalid = false });
            this.LapcapDataController = new LapcapDataController(
                dbContext,
                lapcapDataValidator.Object,
                new TelemetryClient())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = context,
                },
            };

            var request = new CreateLapcapDataDto
            {
                ParameterYear = "203334",
                LapcapFileName = "Some Name",
                LapcapDataTemplateValues = new List<LapcapDataTemplateValueDto>(),
            };
            var task = this.LapcapDataController.Create(request);
            task.Wait();
            var result = task.Result;
            Assert.IsNotNull(result);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(400, objectResult.StatusCode);
            Assert.AreEqual("No data available for the specified year. Please check the year and try again.",
                objectResult?.Value?.ToString());

            var lapcapLatest = dbContext.LapcapDataMaster.Where(x => x.EffectiveTo == null).ToList();
            Assert.AreEqual(2, lapcapLatest.Count);
            Assert.IsNotNull(dbContext.LapcapDataMaster.Single(x => x.ProjectionYearId == "202930" && x.EffectiveTo == null));
            Assert.IsNotNull(dbContext.LapcapDataMaster.Single(x => x.ProjectionYearId == "203031" && x.EffectiveTo == null));

            var lapcapDataList = dbContext.LapcapDataDetail.ToList();
            foreach (var lapcapData in lapcapDataList)
            {
                dbContext.LapcapDataDetail.Remove(lapcapData);
            }

            var lapcapDataMasterList = dbContext.LapcapDataMaster.ToList();
            foreach (var lapcapDataMaster in lapcapDataMasterList)
            {
                dbContext.LapcapDataMaster.Remove(lapcapDataMaster);
            }

            dbContext.SaveChanges();
        }
    }
}
