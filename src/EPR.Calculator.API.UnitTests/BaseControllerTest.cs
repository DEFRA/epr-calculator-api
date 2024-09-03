using EPR.Calculator.API.CommandHandlers;
using EPR.Calculator.API.Commands;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Queries;
using EPR.Calculator.API.QueryHandlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace api.Tests.Controllers
{
    [TestClass]
    public class BaseControllerTest
    {
        protected ApplicationDBContext dbContext;
        protected DefaultParameterSettingController controller;

        protected static DbContextOptions _dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: "PayCal")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        [TestInitialize]
        public void SetUp()
        {
            this.dbContext = new ApplicationDBContext(_dbContextOptions);
            this.dbContext.Database.EnsureCreated();
            var percentDecreses = dbContext.DefaultParameterTemplateMasterList.Where(x => x.ValidRangeTo < 0).ToList();
            foreach (var percent in percentDecreses) 
            {
                percent.ValidRangeFrom = percent.ValidRangeTo;
                percent.ValidRangeTo = 0M;
            }
            var tontDI =dbContext.DefaultParameterTemplateMasterList.SingleOrDefault(x => x.ParameterUniqueReferenceId == "TONT-DI");

            var tontAd = new DefaultParameterTemplateMaster
            {
                ParameterCategory = "Tonnage change threshold",
                ParameterUniqueReferenceId = "TONT-AD",
                ParameterType = "Amount Decrease",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99M
            };
            tontAd.ParameterUniqueReferenceId = "TONT-AD";
            this.dbContext.Entry(tontDI).State = EntityState.Deleted;
            this.dbContext.DefaultParameterTemplateMasterList.Add(tontAd);
            this.dbContext.SaveChanges();

            ICommandHandler<CreateDefaultParameterCommand> commandHandler = new CreateDefaultParameterCommandHandler(dbContext);
            IQueryHandler<DefaultParameterSettingDetailQuery, IEnumerable<DefaultSchemeParametersDto>> queryHandler = new DefaultParameterSettingDetailQueryHandler(dbContext);

            controller = new DefaultParameterSettingController(this.dbContext, commandHandler, queryHandler);
        }

        [TestMethod]
        public void CheckDbContext()
        {
            Assert.IsNotNull(this.dbContext);
            Assert.IsTrue(this.dbContext.Database.IsInMemory());
        }

        [TestCleanup]
        public void TearDown()
        {
            this.dbContext.Database.EnsureDeleted();
        }
    }
}
