using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace api.Tests.Controllers
{
    public class BaseControllerTest
    {
        protected ApplicationDBContext _dbContext;
        protected DefaultParameterSettingController _controller;

        protected static DbContextOptions _dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: "PayCal")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        [TestInitialize]
        public void SetUp()
        {
            _dbContext = new ApplicationDBContext(_dbContextOptions);
            _dbContext.Database.EnsureCreated();
            var percentDecreses = _dbContext.DefaultParameterTemplateMasterList.Where(x => x.ValidRangeTo < 0).ToList();
            foreach (var percent in percentDecreses) 
            {
                percent.ValidRangeFrom = percent.ValidRangeTo;
                percent.ValidRangeTo = 0M;
            }
            var tontDI =_dbContext.DefaultParameterTemplateMasterList.SingleOrDefault(x => x.ParameterUniqueReferenceId == "TONT-DI");

            var tontAd = new DefaultParameterTemplateMaster
            {
                ParameterCategory = "Tonnage change threshold",
                ParameterUniqueReferenceId = "TONT-AD",
                ParameterType = "Amount Decrease",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99M
            };
            tontAd.ParameterUniqueReferenceId = "TONT-AD";
            _dbContext.Entry(tontDI).State = EntityState.Deleted;
            _dbContext.DefaultParameterTemplateMasterList.Add(tontAd);
            _dbContext.SaveChanges();

            _controller = new DefaultParameterSettingController(_dbContext);
        }

        [TestCleanup]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
        }
    }
}
