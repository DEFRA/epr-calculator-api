using api.Controllers;
using api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework;

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

        [SetUp]
        public void SetUp()
        {
            _dbContext = new ApplicationDBContext(_dbContextOptions);
            _dbContext.Database.EnsureCreated();

            _controller = new DefaultParameterSettingController(_dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
        }
    }
}
