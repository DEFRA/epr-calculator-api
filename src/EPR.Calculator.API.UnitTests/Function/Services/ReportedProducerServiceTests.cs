using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.Builder;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class ReportedProducerServiceTests
    {
        private ApplicationDBContext dbContext;
        private ReportedProducerService reportedProducerService;

        public ReportedProducerServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            dbContext = new ApplicationDBContext(options);

            reportedProducerService = new ReportedProducerService(dbContext);
        }

        [TestCleanup]
        public void Cleanup()
        {
            dbContext.Dispose();
        }

        [TestMethod]
        public async Task ShouldReturnProducers_ForGivenRunId()
        {
            TestDataHelper.SeedDatabaseForInitialRun(dbContext);
            var result = await reportedProducerService.GetProducers(1);

            Assert.AreEqual(3, result.Count);

            var firstProducer = result.Single(p => p.OrganisationId == 1);

            var firstPd = firstProducer.Producers.Single();
            Assert.AreEqual(2, firstPd.ProducerReportedMaterials.Count);
            CollectionAssert.AreEquivalent(
                new List<string> { "2025-H1",  "2025-H2" },
                firstPd.ProducerReportedMaterials
                    .Select(m => m.SubmissionPeriod)
                    .ToList()
            );
        }
    }
}