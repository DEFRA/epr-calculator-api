using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Constants;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Services;

[TestClass]
public class InvoicedProducerServiceTests : TestsFor<InvoicedProducerService>
{
    [TestMethod]
    public async Task GetAcceptedCancelledProducerIdsForRun_ReturnsOnlyAcceptedCancelInstructions()
    {
        // Arrange
        var run = new CalculatorRun { Id = 1, Name = "Run 1" };
        var otherRun = new CalculatorRun { Id = 2, Name = "Run 2" };
        dbContext.CalculatorRuns.AddRange(run, otherRun);

        dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
            new ProducerResultFileSuggestedBillingInstruction { CalculatorRun = run, ProducerId = 1, BillingInstructionAcceptReject = BillingConstants.Action.Accepted, SuggestedBillingInstruction = BillingConstants.Suggestion.Cancel },
            new ProducerResultFileSuggestedBillingInstruction { CalculatorRun = run, ProducerId = 2, BillingInstructionAcceptReject = BillingConstants.Action.Accepted, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial },
            new ProducerResultFileSuggestedBillingInstruction { CalculatorRun = run, ProducerId = 3, BillingInstructionAcceptReject = BillingConstants.Action.Rejected, SuggestedBillingInstruction = BillingConstants.Suggestion.Cancel },
            new ProducerResultFileSuggestedBillingInstruction { CalculatorRun = otherRun, ProducerId = 4, BillingInstructionAcceptReject = BillingConstants.Action.Accepted, SuggestedBillingInstruction = BillingConstants.Suggestion.Cancel });

        dbContext.SaveChanges();

        // Act
        var result = await testSubject.GetAcceptedCancelledProducerIdsForRun(run.Id);

        // Assert
        result.ShouldBe([1]);
    }

    [TestMethod]
    public async Task GetAcceptedCancelledProducerIdsForRun_ReturnsEmpty_WhenNoneMatch()
    {
        // Arrange
        var run = new CalculatorRun { Id = 1, Name = "Run 1" };
        dbContext.CalculatorRuns.Add(run);
        dbContext.ProducerResultFileSuggestedBillingInstruction.Add(
            new ProducerResultFileSuggestedBillingInstruction { CalculatorRun = run, ProducerId = 1, BillingInstructionAcceptReject = BillingConstants.Action.Accepted, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial });

        dbContext.SaveChanges();

        // Act
        var result = await testSubject.GetAcceptedCancelledProducerIdsForRun(run.Id);

        // Assert
        result.ShouldBeEmpty();
    }
}
