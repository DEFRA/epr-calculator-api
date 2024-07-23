using api.Controllers;
using api.Data;
using api.Dtos;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace api.Tests.Controllers
{
    public class DefaultParameterSettingControllerTest
    {
        private ApplicationDBContext _dbContext;
        private DefaultParameterSettingController _controller;

        private static DbContextOptions _dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: "PayCal")
            .Options;

        [OneTimeSetUp]
        public void SetUp()
        {
            _dbContext = new ApplicationDBContext(_dbContextOptions);
            _dbContext.Database.EnsureCreated();

            _controller = new DefaultParameterSettingController(_dbContext);
        }

        [Test]
        public void CreateTest()
        {
            var createDefaultParameterDto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = "2024-25",
                SchemeParameterTemplateValues = new List<SchemeParameterTemplateValue>()
            };
            var actionResult = _controller.Create(createDefaultParameterDto);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
        }
    }
}
