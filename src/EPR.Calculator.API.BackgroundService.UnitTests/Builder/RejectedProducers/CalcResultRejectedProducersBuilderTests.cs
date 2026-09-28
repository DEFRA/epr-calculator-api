using EPR.Calculator.API.BackgroundService.Builder.RejectedProducers;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Constants;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.TestData;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Builder.RejectedProducers;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class CalcResultRejectedProducersBuilderTests : TestsFor<CalcResultRejectedProducersBuilder>
{
    [TestMethod]
    public async Task Construct_ReturnsRejectedProducers_WithLatestOrganisationDetails()
    {
        // Arrange
        var runContextOld = TestDataHelper.CalculatorRun2025;
        var runContextLatest = runContextOld with { RunId = runContextOld.RunId + 1 };

        const int organisationId = 100;
        dbContext.CalculatorRunRelativeYears.Add(new CalculatorRunRelativeYear { Value = runContextOld.RelativeYear });

        var runOld = new CalculatorRun
        {
            Id = runContextOld.RunId,
            Name = runContextOld.RunName,
            RelativeYear = runContextOld.RelativeYear
        };
        var runLatest = new CalculatorRun
        {
            Id = runContextLatest.RunId,
            Name = runContextLatest.RunName,
            RelativeYear = runContextLatest.RelativeYear
        };
        dbContext.CalculatorRuns.AddRange(runOld, runLatest);

        var orgOld = new CalculatorRunOrganisation
        {
            Id = 1,
            CalculatorRunId = runOld.Id,
            OrganisationId = organisationId,
            OrganisationName = "Old Org Name",
            TradingName = "Old Trading Name"
        };
        var orgLatest = new CalculatorRunOrganisation
        {
            Id = 2,
            CalculatorRunId = runLatest.Id,
            OrganisationId = organisationId,
            OrganisationName = "Latest Org Name",
            TradingName = "Latest Trading Name"
        };
        dbContext.CalculatorRunOrganisations.AddRange(orgOld, orgLatest);

        // Producer detail for the new run
        dbContext.ProducerDetail.Add(new ProducerDetail
        {
            CalculatorRunId = runOld.Id,
            ProducerId = organisationId,
            ProducerName = "Producer Name",
            TradingName = "Trading Name",
            SubsidiaryId = null
        });

        // Rejected billing instruction for the new run
        var confirmedDate = new DateTime(2024, 1, 1);
        dbContext.ProducerResultFileSuggestedBillingInstruction.Add(new ProducerResultFileSuggestedBillingInstruction
        {
            CalculatorRunId = runOld.Id,
            ProducerId = organisationId,
            SuggestedBillingInstruction = "Instruction A",
            SuggestedInvoiceAmount = 123.45m,
            BillingInstructionAcceptReject = BillingConstants.Action.Rejected,
            ReasonForRejection = "Invalid data",
            LastModifiedAcceptReject = confirmedDate,
            LastModifiedAcceptRejectBy = "User A"
        });

        await dbContext.SaveChangesAsync();

        // Act
        var result = (await testSubject.ConstructAsync(runContextOld, CancellationToken.None)).ToList();

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

        Assert.AreEqual(runLatest.Id, rejected.RunId);
    }
}
