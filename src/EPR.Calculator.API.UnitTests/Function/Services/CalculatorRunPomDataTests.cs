using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class CalculatorRunPomDataTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ApplicationDBContext context;

        public CalculatorRunPomDataTests()
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

        private async Task<(RelativeYear, CalculatorRunClassification, PomData)> SeedData()
        {
            var calculatorRunRelativeYear = new CalculatorRunRelativeYear { Value  = 2024 };
            context.CalculatorRunRelativeYears.Add(calculatorRunRelativeYear);

            var classification = new CalculatorRunClassification { Status = "Test Classification" };
            context.CalculatorRunClassifications.Add(classification);

            var pomData = new PomData
            {
                OrganisationId = 1,
                PackagingActivity = "Test Activity",
                PackagingType = "Test Type",
                PackagingClass = "Test Class",
                PackagingMaterial = "Test Material",
                PackagingMaterialWeight = 100,
                LoadTimeStamp = DateTime.Now,
                SubmissionPeriod = "2024-Q1",
                SubmissionPeriodDesc = "Q1 2024",
                SubsidiaryId = "Test Subsidiary"
            };

            await context.Database.ExecuteSqlInterpolatedAsync(
                $@"
                INSERT INTO pom_data(organisation_id, packaging_activity, packaging_type, packaging_class, packaging_material, packaging_material_weight, load_ts, submission_period, submission_period_desc, subsidiary_id)
                VALUES ({pomData.OrganisationId}, {pomData.PackagingActivity}, {pomData.PackagingType}, {pomData.PackagingClass}, {pomData.PackagingMaterial}, {pomData.PackagingMaterialWeight}, {pomData.LoadTimeStamp}, {pomData.SubmissionPeriod}, {pomData.SubmissionPeriodDesc}, {pomData.SubsidiaryId})");

            await context.SaveChangesAsync();
            return (new RelativeYear(calculatorRunRelativeYear.Value), classification, pomData);

        }

        [TestMethod]
        public async Task LoadPomDataForCalcRun()
        {
            int runId = 1;
            int runId2 = 2;
            string createdBy = "TestUser";
            var cancellationToken = CancellationToken.None;
            var service = new CalculatorRunPomData(context);
            var (relativeYear, classification, pomData) = await SeedData();

            //Run 1
            var run = new CalculatorRun { Id = runId, RelativeYear = relativeYear, Name = "CalculatorRunTest1", CalculatorRunClassificationId = classification.Id };
            context.CalculatorRuns.Add(run);
            await context.SaveChangesAsync();

            await service.LoadPomDataForCalcRun(runId, relativeYear, createdBy, cancellationToken);

            var masterRecords = await context.CalculatorRunPomDataMaster.ToListAsync();
            Assert.AreEqual(1, masterRecords.Count);
            var pomMasterRun1 = masterRecords[0];
            Assert.AreEqual(relativeYear, pomMasterRun1.RelativeYear);
            Assert.AreEqual(createdBy, pomMasterRun1.CreatedBy);
            Assert.IsNull(pomMasterRun1.EffectiveTo);

            var detailRecords = await context.CalculatorRunPomDataDetails.ToListAsync();
            Assert.AreEqual(1, detailRecords.Count);
            var pomDataRun1 = detailRecords[0];
            Assert.AreEqual(pomData.OrganisationId, pomDataRun1.OrganisationId);
            Assert.AreEqual(pomData.SubsidiaryId, pomDataRun1.SubsidiaryId);
            Assert.AreEqual(pomMasterRun1.Id, pomDataRun1.CalculatorRunPomDataMasterId);

            var calculatorRun1 = await context.CalculatorRuns.FirstOrDefaultAsync(c => c.Id == runId);
            Assert.AreEqual(pomMasterRun1.Id, calculatorRun1!.CalculatorRunPomDataMasterId);

            //Run 2
            var run2 = new CalculatorRun { Id = runId2, RelativeYear = relativeYear, Name = "CalculatorRunTest2", CalculatorRunClassificationId = classification.Id };
            context.CalculatorRuns.Add(run2);
            await context.SaveChangesAsync();

            await service.LoadPomDataForCalcRun(runId2, relativeYear, createdBy, cancellationToken);

            var updatedMasterRecords = await context.CalculatorRunPomDataMaster.ToListAsync();
            Assert.AreEqual(2, updatedMasterRecords.Count);
            var pomMasterRun2 = updatedMasterRecords[1];
            Assert.IsNotNull(updatedMasterRecords[0].EffectiveTo, "EffectiveTo should be set for the master record for run 1");
            Assert.IsNull(pomMasterRun2.EffectiveTo, "EffectiveTo should be null for the master record for run 2");

            var updatedDetailRecords = await context.CalculatorRunPomDataDetails.ToListAsync();
            Assert.AreEqual(2, updatedDetailRecords.Count);
            var pomDataRun2 = updatedDetailRecords[1];
            Assert.AreEqual(pomData.OrganisationId, pomDataRun2.OrganisationId);
            Assert.AreEqual(pomData.PackagingActivity, pomDataRun2.PackagingActivity);
            Assert.AreEqual(pomMasterRun2.Id, pomDataRun2.CalculatorRunPomDataMasterId);

            var calculatorRun2 = await context.CalculatorRuns.FirstOrDefaultAsync(c => c.Id == runId2);
            Assert.AreEqual(pomMasterRun2.Id, calculatorRun2!.CalculatorRunPomDataMasterId);
        }
    }
}