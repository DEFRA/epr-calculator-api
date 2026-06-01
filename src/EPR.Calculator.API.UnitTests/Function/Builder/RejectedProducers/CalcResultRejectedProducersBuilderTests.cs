using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.RejectedProducers;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Misc;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.RejectedProducers
{
    [TestClass]
    public class CalcResultRejectedProducersBuilderTests
    {
        private ApplicationDBContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDBContext(options);
        }

        private CalcResultRejectedProducersBuilder CreateBuilder(ApplicationDBContext context)
        {
            return new CalcResultRejectedProducersBuilder(context);
        }

        [TestMethod]
        public async Task Construct_ReturnsRejectedProducers_WithLatestOrganisationDetails()
        {
            // Arrange
            var context = CreateDbContext();

            const int organisationId = 100;
            context.CalculatorRunRelativeYears.Add(new CalculatorRunRelativeYear { Value = 2025 });

            var masterOld = new CalculatorRunOrganisationDataMaster
            {
                Id = 1,
                RelativeYear = new RelativeYear(2025),
                EffectiveFrom = DateTime.UtcNow.AddDays(-10),
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "testsuperuser.paycal"
            };
            var masterLatest = new CalculatorRunOrganisationDataMaster
            {
                Id = 2,
                RelativeYear = new RelativeYear(2025),
                EffectiveFrom = DateTime.UtcNow.AddDays(-5),
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                CreatedBy = "testsuperuser.paycal"
            };
            context.CalculatorRunOrganisationDataMaster.AddRange(masterOld, masterLatest);


            var runOld = new CalculatorRun
            {
                Id = 1,
                Name = "Run 1",
                RelativeYear = new RelativeYear(2025),
                CalculatorRunOrganisationDataMasterId = masterOld.Id
            };
            var runLatest = new CalculatorRun
            {
                Id = 2,
                Name = "Run 2",
                RelativeYear = new RelativeYear(2025),
                CalculatorRunOrganisationDataMasterId = masterLatest.Id
            };
            context.CalculatorRuns.AddRange(runOld, runLatest);

            var orgOld = new CalculatorRunOrganisationDataDetail
            {
                Id = 1,
                CalculatorRunOrganisationDataMasterId = masterOld.Id,
                OrganisationId = organisationId,
                OrganisationName = "Old Org Name",
                TradingName = "Old Trading Name"
            };
            var orgLatest = new CalculatorRunOrganisationDataDetail
            {
                Id = 2,
                CalculatorRunOrganisationDataMasterId = masterLatest.Id,
                OrganisationId = organisationId,
                OrganisationName = "Latest Org Name",
                TradingName = "Latest Trading Name"
            };
            context.CalculatorRunOrganisationDataDetails.AddRange(orgOld, orgLatest);

            // Producer detail for the current run
            context.ProducerDetail.Add(new ProducerDetail
            {
                CalculatorRunId = runOld.Id,
                ProducerId = organisationId,
                ProducerName = "Producer Name",
                TradingName = "Trading Name",
                SubsidiaryId = null
            });

            // Rejected billing instruction for the current run
            var confirmedDate = new DateTime(2024, 1, 1);
            context.ProducerResultFileSuggestedBillingInstruction.Add(new ProducerResultFileSuggestedBillingInstruction
            {
                CalculatorRunId = runOld.Id,
                ProducerId = organisationId,
                SuggestedBillingInstruction = "Instruction A",
                SuggestedInvoiceAmount = 123.45m,
                BillingInstructionAcceptReject = CommonConstants.Rejected,
                ReasonForRejection = "Invalid data",
                LastModifiedAcceptReject = confirmedDate,
                LastModifiedAcceptRejectBy = "User A"
            });

            await context.SaveChangesAsync();

            var builder = CreateBuilder(context);
            var requestDto = new CalcResultsRequestDto
            {
                RunId = runOld.Id,
                RelativeYear = new RelativeYear(2025)
            };

            // Act
            var result = (await builder.ConstructAsync(requestDto)).ToList();

            // Assert
            Assert.AreEqual(1, result.Count);

            var rejected = result[0];

            // Organisation details should come from the latest run
            Assert.AreEqual(organisationId, rejected.ProducerId);
            Assert.AreEqual("Latest Org Name", rejected.ProducerName);
            Assert.AreEqual("Latest Trading Name", rejected.TradingName);

            Assert.AreEqual("Instruction A", rejected.SuggestedBillingInstruction);
            Assert.AreEqual(123.45m, rejected.SuggestedInvoiceAmount);
            Assert.AreEqual(confirmedDate, rejected.InstructionConfirmedDate);
            Assert.AreEqual("User A", rejected.InstructionConfirmedBy);
            Assert.AreEqual("Invalid data", rejected.ReasonForRejection);

            Assert.AreEqual(runLatest.Id, rejected.runId);
        }
    }
}
