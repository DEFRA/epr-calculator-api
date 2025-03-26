using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Validators;
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
        private ApplicationDBContext DbContext { get; set; }
        private LapcapDataController LapcapDataController { get; set; }

        [TestInitialize]
        public void SetUp()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
           .UseInMemoryDatabase(databaseName: "PayCal")
           .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
           .Options;

            this.DbContext = new ApplicationDBContext(dbContextOptions);
            this.DbContext.Database.EnsureCreated();
        }

        public void TearDown()
        {
            this.DbContext.Database.EnsureDeleted();
        }

        [TestMethod]
        public void Test_With_Multiple_Financial_Years()
        {
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext()
            {
                User = principal,
            };

            var year24 = new CalculatorRunFinancialYear
            {
                Name = "2024-25",
                Description = ""
            };
            DbContext.Add(year24);

            var year25 = new CalculatorRunFinancialYear
            {
                Name = "2025-26",
                Description = ""
            };
            DbContext.Add(year25);

            var lapcapMaster25 = new LapcapDataMaster {
                ProjectionYearId = "2024-25",
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
                ProjectionYearId = "2025-26",
                EffectiveFrom = new DateTime(2025, 1, 1),
                EffectiveTo = null,
                ProjectionYear = year25,
            };
            var lapcapDetail26 = new LapcapDataDetail
            {
                LapcapDataMaster = lapcapMaster26,
                UniqueReference = LapcapDataUniqueReferences.UniqueReferences.First(),
            };
            this.DbContext.LapcapDataMaster.Add(lapcapMaster25);
            this.DbContext.LapcapDataDetail.Add(lapcapDetail25);
            this.DbContext.LapcapDataMaster.Add(lapcapMaster26);
            this.DbContext.LapcapDataDetail.Add(lapcapDetail26);
            this.DbContext.SaveChanges();

            var lapcapDataValidator = new Mock<ILapcapDataValidator>();
            lapcapDataValidator.Setup(x => x.Validate(It.IsAny<CreateLapcapDataDto>()))
                .Returns(new LapcapValidationResultDto { IsInvalid = false });
            this.LapcapDataController = new LapcapDataController(this.DbContext, lapcapDataValidator.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = context,
                },
            };

            var request = new CreateLapcapDataDto
            {
                ParameterYear = "2024-25",
                LapcapFileName = "Some Name",
                LapcapDataTemplateValues = new List<LapcapDataTemplateValueDto>(),
            };
            var task = this.LapcapDataController.Create(request);
            task.Wait();
            var result = task.Result;
            Assert.IsNotNull(result);

            var lapcapLatest = DbContext.LapcapDataMaster.Where(x => x.EffectiveTo == null).ToList();
            Assert.AreEqual(2, lapcapLatest.Count);
            Assert.IsNotNull(DbContext.LapcapDataMaster.Single(x => x.ProjectionYearId == "2024-25" && x.EffectiveTo == null));
            Assert.IsNotNull(DbContext.LapcapDataMaster.Single(x => x.ProjectionYearId == "2025-26" && x.EffectiveTo == null));
        }
    }
}
