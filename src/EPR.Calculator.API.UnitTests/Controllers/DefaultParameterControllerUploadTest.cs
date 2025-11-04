using System.Security.Claims;
using System.Security.Principal;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
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
    public class DefaultParameterControllerUploadTest : BaseControllerTest
    {
        public TestContext TestContext { get; set; }

        private DefaultParameterSettingController DefaultParameterController { get; set; } = null!;

        [TestInitialize]
        public void SetUp()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
           .UseInMemoryDatabase(databaseName: "PayCalDefaultParams")
           .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
           .Options;

            this.DbContext = new ApplicationDBContext(dbContextOptions);
            this.DbContext.Database.EnsureCreated();
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

            var year29 = new CalculatorRunFinancialYear
            {
                Name = "2029-30",
                Description = string.Empty,
            };
            DbContext.Add(year29);

            var year30 = new CalculatorRunFinancialYear
            {
                Name = "2030-31",
                Description = string.Empty,
            };
            DbContext.Add(year30);

            var defaultParameterSettingMaster25 = new DefaultParameterSettingMaster
            {
                ParameterYearId = "2029-30",
                EffectiveFrom = new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Local),
                EffectiveTo = null,
                ParameterYear = year29,
            };
            var defaultParameterDetail25 = new DefaultParameterSettingDetail
            {
                DefaultParameterSettingMaster = defaultParameterSettingMaster25,
                ParameterUniqueReferenceId = CommonResources.DefaultParameterUniqueReferences.Split(',')[0],
            };

            var defaultParameterSettingMaster26 = new DefaultParameterSettingMaster
            {
                ParameterYearId = "2030-31",
                EffectiveFrom = new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Local),
                EffectiveTo = null,
                ParameterYear = year30,
            };
            var defaultParameterDetail26 = new DefaultParameterSettingDetail
            {
                DefaultParameterSettingMaster = defaultParameterSettingMaster26,
                ParameterUniqueReferenceId = CommonResources.DefaultParameterUniqueReferences.Split(',')[0],
            };
            this.DbContext.DefaultParameterSettings.Add(defaultParameterSettingMaster25);
            this.DbContext.DefaultParameterSettingDetail.Add(defaultParameterDetail25);
            this.DbContext.DefaultParameterSettings.Add(defaultParameterSettingMaster26);
            this.DbContext.DefaultParameterSettingDetail.Add(defaultParameterDetail26);
            this.DbContext.SaveChanges();

            var defaultParameterValidator = new Mock<ICreateDefaultParameterDataValidator>();
            defaultParameterValidator.Setup(x => x.Validate(It.IsAny<CreateDefaultParameterSettingDto>()))
                .Returns(new ValidationResultDto<CreateDefaultParameterSettingErrorDto> { IsInvalid = false });
            this.DefaultParameterController = new DefaultParameterSettingController(this.DbContext, defaultParameterValidator.Object, TelemetryClient)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = context,
                },
            };

            var request = new CreateDefaultParameterSettingDto()
            {
                ParameterYear = "2029-30",
                ParameterFileName = "test Name",
                SchemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>(),
            };
            var task = this.DefaultParameterController.Create(request);
            task.Wait(TestContext.CancellationTokenSource.Token);
            var result = task.Result;
            Assert.IsNotNull(result);

            var defaultParameterLatest = DbContext.DefaultParameterSettings.Where(x => x.EffectiveTo == null).ToList();
            Assert.HasCount(2, defaultParameterLatest);
            Assert.IsNotNull(DbContext.DefaultParameterSettings.Single(x => x.ParameterYearId == "2029-30" && x.EffectiveTo == null));
            Assert.IsNotNull(DbContext.DefaultParameterSettings.Single(x => x.ParameterYearId == "2030-31" && x.EffectiveTo == null));
        }
    }
}