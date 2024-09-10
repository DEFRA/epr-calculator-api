using api.Validators;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace api.Tests.Controllers
{
    [TestClass]
    public class BaseControllerTest
    {
        protected ApplicationDBContext? dbContext;
        protected DefaultParameterSettingController? defaultParameterSettingController;
        protected LapcapDataController? lapcapDataController;

        [TestInitialize]
        public void SetUp()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: "PayCal")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

            dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();
            var percentDecreses = dbContext.DefaultParameterTemplateMasterList.Where(x => x.ValidRangeTo < 0).ToList();
            foreach (var percent in percentDecreses) 
            {
                percent.ValidRangeFrom = percent.ValidRangeTo;
                percent.ValidRangeTo = 0M;
            }
            var tontDI = dbContext.DefaultParameterTemplateMasterList.SingleOrDefault(x => x.ParameterUniqueReferenceId == "TONT-DI");

            var tontAd = new DefaultParameterTemplateMaster
            {
                ParameterCategory = "Tonnage change threshold",
                ParameterUniqueReferenceId = "TONT-AD",
                ParameterType = "Amount Decrease",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99M
            };
            tontAd.ParameterUniqueReferenceId = "TONT-AD";
#pragma warning disable CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.
            dbContext.Entry(tontDI).State = EntityState.Deleted;
#pragma warning restore CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.
            dbContext.DefaultParameterTemplateMasterList.Add(tontAd);
            dbContext.SaveChanges();

            var validator = new CreateDefaultParameterDataValidator(dbContext);
            defaultParameterSettingController = new DefaultParameterSettingController(dbContext, validator);
            ILapcapDataValidator lapcapDataValidator = new LapcapDataValidator(dbContext);
            lapcapDataController = new LapcapDataController(dbContext, lapcapDataValidator);
        }

        [TestMethod]
        public void CheckDbContext()
        {
            Assert.IsNotNull(dbContext);
            Assert.IsTrue(dbContext.Database.IsInMemory());
        }

        [TestCleanup]
        public void TearDown()
        {
            dbContext?.Database.EnsureDeleted();
        }
    }
}
