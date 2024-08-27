using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class CalculatorControllerTests
    {
        private ApplicationDBContext _dbContext;
        private CalculatorController _controller;

        private static DbContextOptions _dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

        [TestInitialize]
        public void Setup()
        {
            _dbContext = new ApplicationDBContext(_dbContextOptions);
            _dbContext.Database.EnsureCreated();

            _controller = new CalculatorController(_dbContext);
        }

        [TestMethod]
        public void Get_Calculator_Runs_Return_Results_Test()
        {
            var actionResult = _controller.GetCalculatorRuns("2024-25") as ObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(200, actionResult.StatusCode);
        }

        [TestMethod]
        public void Get_Calculator_Runs_Return_Not_Found_Test()
        {
            var actionResult = _controller.GetCalculatorRuns("2022-23") as ObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(404, actionResult.StatusCode);
        }

        [TestMethod]
        public void Get_Calculator_Runs_Return_Bad_Request_Test()
        {
            var actionResult = _controller.GetCalculatorRuns(null) as ObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(400, actionResult.StatusCode);
        }

        [TestCleanup]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
        }
    }
}
