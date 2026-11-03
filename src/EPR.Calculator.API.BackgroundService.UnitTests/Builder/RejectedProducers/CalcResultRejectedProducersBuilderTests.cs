using EPR.Calculator.API.BackgroundService.Builder.RejectedProducers;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Constants;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.TestData;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Builder.RejectedProducers;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class CalcResultRejectedProducersBuilderTests : TestsFor<CalcResultRejectedProducersBuilder>
{
    private const int ProducerId = 100;

    private static readonly RelativeYear Year = TestDataHelper.CalculatorRun2025.RelativeYear;

    // Runs in id order: an earlier billed run, the run being processed, and a later run
    private const int InvoicedRunId = 2025001;
    private const int CurrentRunId = 2025002;
    private const int LaterRunId = 2025003;

    private int nextOrgRowId = 1;

    protected override void TestInitialize()
    {
        dbContext.CalculatorRunRelativeYears.Add(new CalculatorRunRelativeYear { Value = Year });

        AddRun(InvoicedRunId, isCompleted: true);
        AddRun(CurrentRunId, isCompleted: false);
        AddRun(LaterRunId, isCompleted: true);
    }

    [TestMethod]
    public async Task Construct_UsesTheCurrentRunsProducerDetails_ForAProducerInTheRun()
    {
        // Arrange - the producer is in the current run, and a later run has different organisation details
        dbContext.ProducerDetail.Add(new ProducerDetail
        {
            CalculatorRunId = CurrentRunId,
            ProducerId = ProducerId,
            ProducerName = "Current Name",
            TradingName = "Current Trading Name"
        });
        AddOrganisation(LaterRunId, "Later Name", "Later Trading Name", ObligationStates.Obligated);

        var confirmedDate = new DateTime(2024, 1, 1);
        AddRejectedInstruction(CurrentRunId, BillingConstants.Suggestion.Initial, suggestedInvoiceAmount: 123.45m, confirmedDate: confirmedDate);

        await dbContext.SaveChangesAsync();

        // Act
        var result = (await testSubject.ConstructAsync(CurrentRunContext, CancellationToken.None)).ToList();

        // Assert
        result.Count.ShouldBe(1);
        var rejected = result[0];
        rejected.ProducerId.ShouldBe(ProducerId);
        rejected.ProducerName.ShouldBe("Current Name");
        rejected.TradingName.ShouldBe("Current Trading Name");
        rejected.RunId.ShouldBe(CurrentRunId);
        rejected.SuggestedBillingInstruction.ShouldBe(BillingConstants.Suggestion.Initial);
        rejected.SuggestedInvoiceAmount.ShouldBe(123.45m);
        rejected.InstructionConfirmedDate.ShouldBe(confirmedDate);
        rejected.InstructionConfirmedBy.ShouldBe("User A");
        rejected.ReasonForRejection.ShouldBe("Invalid data");
    }

    [TestMethod]
    public async Task Construct_UsesTheCurrentRunsObligatedOrganisation_ForAHoldingCompanyWithOnlySubsidiaryProducerDetails()
    {
        // Arrange - the holding company doesn't report its own packaging, so only its subsidiary has producer details
        dbContext.ProducerDetail.Add(new ProducerDetail
        {
            CalculatorRunId = CurrentRunId,
            ProducerId = ProducerId,
            SubsidiaryId = "SUB1",
            ProducerName = "Subsidiary Name",
            TradingName = "Subsidiary Trading Name"
        });
        AddOrganisation(CurrentRunId, "Not Obligated Name", null, ObligationStates.NotObligated);
        AddOrganisation(CurrentRunId, "Holding Company Name", "Holding Company Trading Name", ObligationStates.Obligated);
        AddOrganisation(LaterRunId, "Later Name", "Later Trading Name", ObligationStates.Obligated);

        AddRejectedInstruction(CurrentRunId, BillingConstants.Suggestion.Initial, suggestedInvoiceAmount: 50m);

        await dbContext.SaveChangesAsync();

        // Act
        var result = (await testSubject.ConstructAsync(CurrentRunContext, CancellationToken.None)).ToList();

        // Assert
        result.Count.ShouldBe(1);
        result[0].ProducerName.ShouldBe("Holding Company Name");
        result[0].TradingName.ShouldBe("Holding Company Trading Name");
        result[0].RunId.ShouldBe(CurrentRunId);
    }

    [TestMethod]
    public async Task Construct_UsesTheLatestInvoicedRunsObligatedDetails_ForARejectedCancellation()
    {
        // Arrange - invoiced in an earlier run (where obligated), now not in the current run, with two error
        // registrations in the current run's organisation data and different details in a later run
        AddOrganisation(InvoicedRunId, "Invoiced Name", "Invoiced Trading Name", ObligationStates.Obligated);
        AddOrganisation(CurrentRunId, "Error Name", null, ObligationStates.Error);
        AddOrganisation(CurrentRunId, "Error Name", "Error Trading Name", ObligationStates.Error);
        AddOrganisation(LaterRunId, "Later Name", "Later Trading Name", ObligationStates.Obligated);

        AddInstruction(InvoicedRunId, BillingConstants.Suggestion.Initial, BillingConstants.Action.Accepted);
        AddRejectedInstruction(CurrentRunId, BillingConstants.Suggestion.Cancel, currentYearInvoiceTotalToDate: 500m);

        await dbContext.SaveChangesAsync();

        // Act
        var result = (await testSubject.ConstructAsync(CurrentRunContext, CancellationToken.None)).ToList();

        // Assert - one row, with the details the producer was last billed under
        result.Count.ShouldBe(1);
        var rejected = result[0];
        rejected.ProducerName.ShouldBe("Invoiced Name");
        rejected.TradingName.ShouldBe("Invoiced Trading Name");
        rejected.RunId.ShouldBe(CurrentRunId);
        rejected.SuggestedBillingInstruction.ShouldBe(BillingConstants.Suggestion.Cancel);
        rejected.SuggestedInvoiceAmount.ShouldBe(500m);
    }

    [TestMethod]
    public async Task Construct_StillReportsTheRejectedInstruction_WhenThereAreNoDetailsToName()
    {
        // Arrange - not in the current run and never invoiced
        AddRejectedInstruction(CurrentRunId, BillingConstants.Suggestion.Cancel);

        await dbContext.SaveChangesAsync();

        // Act
        var result = (await testSubject.ConstructAsync(CurrentRunContext, CancellationToken.None)).ToList();

        // Assert
        result.Count.ShouldBe(1);
        result[0].ProducerId.ShouldBe(ProducerId);
        result[0].ProducerName.ShouldBe(string.Empty);
        result[0].TradingName.ShouldBe(string.Empty);
    }

    private static CalculatorRunContext CurrentRunContext =>
        TestDataHelper.CalculatorRun2025 with { RunId = CurrentRunId };

    private void AddRun(int runId, bool isCompleted)
    {
        var master = new CalculatorRunOrganisationDataMaster
        {
            Id = runId,
            RelativeYear = Year,
            EffectiveFrom = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "testsuperuser.paycal"
        };
        dbContext.CalculatorRunOrganisationDataMaster.Add(master);

        dbContext.CalculatorRuns.Add(new CalculatorRun
        {
            Id = runId,
            Name = $"Run {runId}",
            RelativeYear = Year,
            Classification = isCompleted ? RunClassification.InitialCompleted : RunClassification.Initial,
            CalculatorRunOrganisationDataMasterId = master.Id
        });
    }

    private void AddOrganisation(int runId, string name, string? tradingName, string obligationStatus) =>
        dbContext.CalculatorRunOrganisationDataDetails.Add(new CalculatorRunOrganisationDataDetail
        {
            Id = nextOrgRowId++,
            CalculatorRunOrganisationDataMasterId = runId,
            OrganisationId = ProducerId,
            OrganisationName = name,
            TradingName = tradingName,
            ObligationStatus = obligationStatus
        });

    private void AddInstruction(int runId, string suggestion, string acceptReject) =>
        dbContext.ProducerResultFileSuggestedBillingInstruction.Add(new ProducerResultFileSuggestedBillingInstruction
        {
            CalculatorRunId = runId,
            ProducerId = ProducerId,
            SuggestedBillingInstruction = suggestion,
            BillingInstructionAcceptReject = acceptReject
        });

    private void AddRejectedInstruction(
        int runId,
        string suggestion,
        decimal? suggestedInvoiceAmount = null,
        decimal? currentYearInvoiceTotalToDate = null,
        DateTime? confirmedDate = null) =>
        dbContext.ProducerResultFileSuggestedBillingInstruction.Add(new ProducerResultFileSuggestedBillingInstruction
        {
            CalculatorRunId = runId,
            ProducerId = ProducerId,
            SuggestedBillingInstruction = suggestion,
            SuggestedInvoiceAmount = suggestedInvoiceAmount,
            CurrentYearInvoiceTotalToDate = currentYearInvoiceTotalToDate,
            BillingInstructionAcceptReject = BillingConstants.Action.Rejected,
            ReasonForRejection = "Invalid data",
            LastModifiedAcceptReject = confirmedDate,
            LastModifiedAcceptRejectBy = "User A"
        });
}
