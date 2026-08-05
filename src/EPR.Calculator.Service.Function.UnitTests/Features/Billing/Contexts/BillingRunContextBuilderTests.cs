using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Features.BillingRuns.Constants;
using EPR.Calculator.Service.Function.Features.BillingRuns.Contexts;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using Microsoft.Extensions.Time.Testing;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Billing.Contexts;

[TestCategory(TestCategories.BillingRuns)]
[TestClass]
public class BillingRunContextBuilderTests : TestsFor<BillingRunContextBuilder>
{
    private FakeTimeProvider timeProvider = null!;

    protected override void TestInitialize()
    {
        dbContext.CalculatorRuns.Add(new CalculatorRun
        {
            Id = 1,
            Name = "Valid test run",
            CalculatorRunClassificationId = RunClassificationStatusIds.INITIALRUNID,
            DefaultParameterSettingMasterId = 1,
            LapcapDataMasterId = 1,
            CalculatorRunOrganisationDataMasterId = 1,
            CalculatorRunPomDataMasterId = 1,
            BillingRunStatus = BillingRunStatus.Running,
            ProducerResultFileSuggestedBillingInstruction =
            {
                new ProducerResultFileSuggestedBillingInstruction
                {
                    BillingInstructionAcceptReject = BillingConstants.Action.Accepted,
                    SuggestedBillingInstruction = BillingConstants.Suggestion.Initial
                }
            }
        });

        dbContext.SaveChanges();

        timeProvider = (FakeTimeProvider) fixture.Freeze<TimeProvider>();
    }

    [TestMethod]
    public async Task Should_build()
    {
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        timeProvider.SetUtcNow(now);

        var result = await testSubject.Build(1, "Test User", CancellationToken.None);

        result.RunId.ShouldBe(1);
        result.User.ShouldBe("Test User");
        result.ProcessingStartedAt.ShouldBe(now);
    }

    [TestMethod]
    public async Task Should_handle_no_run()
    {
        await Should.ThrowAsync<RunContextException>(async () => await testSubject.Build(2, "Test User", CancellationToken.None));
    }

    [TestMethod]
    public async Task Should_handle_validation_failure()
    {
        await Should.ThrowAsync<RunContextException>(async () => await testSubject.Build(1, null, CancellationToken.None));

        var run = dbContext.CalculatorRuns
            .Single(x => x.Id == 1);

        run.BillingRunStatus.ShouldBe(BillingRunStatus.Errored);
    }
}
