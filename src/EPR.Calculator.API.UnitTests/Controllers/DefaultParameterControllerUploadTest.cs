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
    public class DefaultParameterControllerUploadTest : BaseControllerTest
    {
        public TestContext TestContext { get; set; }

        private DefaultParameterSettingController DefaultParameterController { get; set; } = null!;

        [TestInitialize]
        public void SetUp()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

            this.DbContext = new ApplicationDBContext(dbContextOptions);
            this.DbContext.Database.EnsureCreated();
        }

        [TestMethod]
        public void Test_With_Multiple_RelativeYears()
        {
            // Arrange user identity
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext()
            {
                User = principal,
            };

            // Add existing relative years
            var year29 = new CalculatorRunRelativeYear { Value = 2029, Description = string.Empty };
            var year30 = new CalculatorRunRelativeYear { Value = 2030, Description = string.Empty };
            var year31 = new CalculatorRunRelativeYear { Value = 2031, Description = string.Empty };
            DbContext.AddRange(year29, year30, year31);
            DbContext.SaveChanges();

            // Existing default parameter settings
            var defaultParameterSettingMaster29 = new DefaultParameterSettingMaster
            {
                EffectiveFrom = new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Local),
                EffectiveTo = null,
                RelativeYear = new RelativeYear(2029),
            };
            var defaultParameterDetail29 = new DefaultParameterSettingDetail
            {
                DefaultParameterSettingMaster = defaultParameterSettingMaster29,
                ParameterUniqueReferenceId = CommonResources.DefaultParameterUniqueReferences.Split(',')[0],
            };

            var defaultParameterSettingMaster30 = new DefaultParameterSettingMaster
            {
                EffectiveFrom = new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Local),
                EffectiveTo = null,
                RelativeYear = new RelativeYear(2030),
            };
            var defaultParameterDetail30 = new DefaultParameterSettingDetail
            {
                DefaultParameterSettingMaster = defaultParameterSettingMaster30,
                ParameterUniqueReferenceId = CommonResources.DefaultParameterUniqueReferences.Split(',')[0],
            };

            DbContext.DefaultParameterSettings.AddRange(defaultParameterSettingMaster29, defaultParameterSettingMaster30);
            DbContext.DefaultParameterSettingDetail.AddRange(defaultParameterDetail29, defaultParameterDetail30);
            DbContext.SaveChanges();

            // Mock validator
            var defaultParameterValidator = new Mock<ICreateDefaultParameterDataValidator>();
            defaultParameterValidator.Setup(x => x.Validate(It.IsAny<CreateDefaultParameterSettingDto>()))
                .Returns(new ValidationResultDto<CreateDefaultParameterSettingErrorDto> { IsInvalid = false });

            // Controller
            this.DefaultParameterController = new DefaultParameterSettingController(DbContext, defaultParameterValidator.Object, TelemetryClient)
            {
                ControllerContext = new ControllerContext { HttpContext = context },
            };

            // Act: create for the new year
            var request = new CreateDefaultParameterSettingDto()
            {
                RelativeYear = new RelativeYear(2031),
                ParameterFileName = "Test File",
                SchemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>(),
            };

            var task = this.DefaultParameterController.Create(request);
            task.Wait(TestContext.CancellationTokenSource.Token);
            var result = task.Result;

            // Assert
            Assert.IsNotNull(result);

            var defaultParameterLatest = DbContext.DefaultParameterSettings.Where(x => x.EffectiveTo == null).ToList();
            Assert.AreEqual(3, defaultParameterLatest.Count); // 29, 30, 31

            Assert.IsNotNull(DbContext.DefaultParameterSettings.Single(x => x.RelativeYearValue == 2029 && x.EffectiveTo == null));
            Assert.IsNotNull(DbContext.DefaultParameterSettings.Single(x => x.RelativeYearValue == 2030 && x.EffectiveTo == null));
            Assert.IsNotNull(DbContext.DefaultParameterSettings.Single(x => x.RelativeYearValue == 2031 && x.EffectiveTo == null));
        }
    }
}