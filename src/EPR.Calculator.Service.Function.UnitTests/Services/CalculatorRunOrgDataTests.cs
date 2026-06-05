using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class CalculatorRunOrgDataTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ApplicationDBContext context;

        public CalculatorRunOrgDataTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseSqlite(_connection)
                .Options;

            context = new ApplicationDBContext(options);
            context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            context.Dispose();
            _connection.Close();
        }

        private async Task<(CalculatorRunClassification, OrganisationData)> SeedData()
        {
            var calculatorRunRelativeYear = new CalculatorRunRelativeYear { Value  = 2024 };
            context.CalculatorRunRelativeYears.Add(calculatorRunRelativeYear);

            var classification = new CalculatorRunClassification { Status = "Test Classification" };
            context.CalculatorRunClassifications.Add(classification);

            var orgData = new OrganisationData
            {
                OrganisationId = 1,
                OrganisationName = "Test Org",
                LoadTimestamp = DateTime.Now,
                ObligationStatus = "O",
                SubmitterId = new Guid("11111111-1111-1111-1111-111111111111"),
                HasH1 = false,
                HasH2 = false,
            };

            // Raw SQL used because `organisation_data` is a keyless temp table; all NOT NULL columns must be included.
            await context.Database.ExecuteSqlInterpolatedAsync(
                $@"
                INSERT INTO organisation_data(organisation_id, organisation_name, load_ts, obligation_status, submitter_id, has_h1, has_h2)
                VALUES ({orgData.OrganisationId}, {orgData.OrganisationName}, {orgData.LoadTimestamp}, {orgData.ObligationStatus}, {orgData.SubmitterId}, {orgData.HasH1}, {orgData.HasH2} )");

            await context.SaveChangesAsync();
            return (classification, orgData);
        }

        [TestMethod]
        public async Task LoadOrgDataForCalcRun()
        {
            var runContext1 = TestDataHelper.CalculatorRun2024;
            var runContext2 = runContext1 with { RunId = runContext1.RunId + 1 };
            var cancellationToken = CancellationToken.None;
            var service = new CalculatorRunOrgData(context, new Mock<ILogger<CalculatorRunOrgData>>().Object);
            var (classification, orgData) = await SeedData();

            //Run 1
            var run = new CalculatorRun { Id = runContext1.RunId, RelativeYear = runContext1.RelativeYear, Name = "CalculatorRunTest1", CalculatorRunClassificationId = classification.Id };
            context.CalculatorRuns.Add(run);
            await context.SaveChangesAsync();

            await service.LoadOrgDataForCalcRun(runContext1, cancellationToken);

            var masterRecords = await context.CalculatorRunOrganisationDataMaster.ToListAsync();
            Assert.AreEqual(1, masterRecords.Count);
            var orgMasterRun1 = masterRecords[0];
            Assert.AreEqual(runContext1.RelativeYear, orgMasterRun1.RelativeYear);
            Assert.AreEqual(runContext1.User, orgMasterRun1.CreatedBy);
            Assert.IsNull(orgMasterRun1.EffectiveTo);

            var detailRecords = await context.CalculatorRunOrganisationDataDetails.ToListAsync();
            Assert.AreEqual(1, detailRecords.Count);
            var orgDataRun1 = detailRecords[0];
            Assert.AreEqual(orgData.OrganisationId, orgDataRun1.OrganisationId);
            Assert.AreEqual(orgData.OrganisationName, orgDataRun1.OrganisationName);
            Assert.AreEqual(orgMasterRun1.Id, orgDataRun1.CalculatorRunOrganisationDataMasterId);

            var calculatorRun1 = await context.CalculatorRuns.FirstOrDefaultAsync(c => c.Id == runContext1.RunId);
            Assert.AreEqual(orgMasterRun1.Id, calculatorRun1!.CalculatorRunOrganisationDataMasterId);

            //Run 2
            var run2 = new CalculatorRun { Id = runContext2.RunId, RelativeYear = runContext2.RelativeYear, Name = "CalculatorRunTest2", CalculatorRunClassificationId = classification.Id };
            context.CalculatorRuns.Add(run2);
            await context.SaveChangesAsync();

            await service.LoadOrgDataForCalcRun(runContext2, cancellationToken);

            var updatedMasterRecords = await context.CalculatorRunOrganisationDataMaster.ToListAsync();
            Assert.AreEqual(2, updatedMasterRecords.Count);
            var orgMasterRun2 = updatedMasterRecords[1];
            Assert.IsNotNull(updatedMasterRecords[0].EffectiveTo, "EffectiveTo should be set for the master record for run 1");
            Assert.IsNull(orgMasterRun2.EffectiveTo, "EffectiveTo should be null for the master record for run 2");

            var updatedDetailRecords = await context.CalculatorRunOrganisationDataDetails.ToListAsync();
            Assert.AreEqual(2, updatedDetailRecords.Count);
            var orgDataRun2 = updatedDetailRecords[1];
            Assert.AreEqual(orgData.OrganisationId, orgDataRun2.OrganisationId);
            Assert.AreEqual(orgData.OrganisationName, orgDataRun1.OrganisationName);
            Assert.AreEqual(orgMasterRun2.Id, orgDataRun2.CalculatorRunOrganisationDataMasterId);

            var calculatorRun2 = await context.CalculatorRuns.FirstOrDefaultAsync(c => c.Id == runContext2.RunId);
            Assert.AreEqual(orgMasterRun2.Id, calculatorRun2!.CalculatorRunOrganisationDataMasterId);
        }
    }
}
