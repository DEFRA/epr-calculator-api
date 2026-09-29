using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Constants;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Contexts;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using FluentValidation.TestHelper;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Features.Billing.Contexts;

[TestCategory(TestCategories.BillingRuns)]
[TestClass]
public class BillingRunContextValidatorTests : TestsFor<BillingRunContextValidator>
{
    private static readonly DateTimeOffset Now = new(2026, 9, 29, 12, 0, 0, TimeSpan.Zero);

    [DataRow(RunClassification.Initial)]
    [DataRow(RunClassification.Recalculation)]
    [TestMethod]
    public void Should_not_error_when_run_is_valid(RunClassification classification)
    {
        var preValidationContext = CreatePreValidationContext(classification: classification);
        var result = testSubject.TestValidate(preValidationContext);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [TestMethod]
    public void Should_error_for_empty_User(string? user)
    {
        var preValidationContext = CreatePreValidationContext(user);
        var result = testSubject.TestValidate(preValidationContext);
        result.ShouldHaveValidationErrorFor(ctx => ctx.User);
    }

    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [TestMethod]
    public void Should_error_for_empty_run_Name(string? name)
    {
        var preValidationContext = CreatePreValidationContext(runName: name);
        var result = testSubject.TestValidate(preValidationContext);
        result.ShouldHaveValidationErrorFor(ctx => ctx.Run.Name);
    }

    [DataRow(null)]
    [DataRow(0)]
    [TestMethod]
    public void Should_error_for_empty_DefaultParameterSettingMasterId(int? id)
    {
        var preValidationContext = CreatePreValidationContext(paramMasterId: id);
        var result = testSubject.TestValidate(preValidationContext);
        result.ShouldHaveValidationErrorFor(ctx => ctx.Run.DefaultParameterSettingMasterId);
    }

    [DataRow(null)]
    [DataRow(0)]
    [TestMethod]
    public void Should_error_for_empty_LapcapDataMasterId(int? id)
    {
        var preValidationContext = CreatePreValidationContext(lapcapMasterId: id);
        var result = testSubject.TestValidate(preValidationContext);
        result.ShouldHaveValidationErrorFor(ctx => ctx.Run.LapcapDataMasterId);
    }

    [DataRow(null)]
    [DataRow(0)]
    [TestMethod]
    public void Should_error_for_empty_CalculatorRunOrganisationDataMasterId(int? id)
    {
        var preValidationContext = CreatePreValidationContext(orgMasterId: id);
        var result = testSubject.TestValidate(preValidationContext);
        result.ShouldHaveValidationErrorFor(ctx => ctx.Run.CalculatorRunOrganisationDataMasterId);
    }

    [DataRow(null)]
    [DataRow(0)]
    [TestMethod]
    public void Should_error_for_empty_CalculatorRunPomDataMasterId(int? id)
    {
        var preValidationContext = CreatePreValidationContext(pomMasterId: id);
        var result = testSubject.TestValidate(preValidationContext);
        result.ShouldHaveValidationErrorFor(ctx => ctx.Run.CalculatorRunPomDataMasterId);
    }

    [DataRow(BillingRunStatus.None)]
    [DataRow(BillingRunStatus.Completed)] // Regenerating the billing file
    [DataRow(BillingRunStatus.Errored)]   // Retrying the billing run
    [TestMethod]
    public void Should_not_error_when_billing_status_allows_a_billing_run(BillingRunStatus status)
    {
        var preValidationContext = CreatePreValidationContext(billingRunStatus: status);
        var result = testSubject.TestValidate(preValidationContext);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [DataRow(61)]
    [DataRow(24 * 60)]
    [TestMethod]
    public void Should_not_error_when_billing_run_is_stuck(int startedMinutesAgo)
    {
        var preValidationContext = CreatePreValidationContext(
            billingRunStatus: BillingRunStatus.Started,
            billingRunStartedAt: Now.UtcDateTime.AddMinutes(-startedMinutesAgo));

        var result = testSubject.TestValidate(preValidationContext);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [DataRow(null)] // Without a start time, it can never be considered stuck
    [DataRow(0)]
    [DataRow(60)]
    [TestMethod]
    public void Should_error_when_billing_run_is_underway(int? startedMinutesAgo)
    {
        var preValidationContext = CreatePreValidationContext(
            billingRunStatus: BillingRunStatus.Started,
            billingRunStartedAt: startedMinutesAgo is { } minutes ? Now.UtcDateTime.AddMinutes(-minutes) : null);

        var result = testSubject.TestValidate(preValidationContext);
        result.ShouldHaveValidationErrorFor(ctx => ctx.Run.BillingRunStatus);
    }

    [TestMethod]
    public void Should_error_when_billing_status_is_unknown()
    {
        var preValidationContext = CreatePreValidationContext(billingRunStatus: BillingRunStatus.Unknown);
        var result = testSubject.TestValidate(preValidationContext);
        result.ShouldHaveValidationErrorFor(ctx => ctx.Run.BillingRunStatus);
    }

    [DynamicData(nameof(NoAcceptedProducersCases))]
    [TestMethod]
    public void Should_error_when_no_accepted_producers(ICollection<ProducerResultFileSuggestedBillingInstruction> instructions)
    {
        var preValidationContext = CreatePreValidationContext(instructions: instructions);
        var result = testSubject.TestValidate(preValidationContext);
        result.ShouldHaveValidationErrorFor(ctx => ctx.AcceptedProducerIds);
    }

    private static BillingRunContextBuilder.PreValidationContext CreatePreValidationContext(
        string? user = "Test User",
        string? runName = "TestRun",
        RunClassification classification = RunClassification.Initial,
        CalculationRunStatus calculationRunStatus = CalculationRunStatus.Completed,
        BillingRunStatus billingRunStatus = BillingRunStatus.None,
        DateTime? billingRunStartedAt = null,
        int? paramMasterId = 1,
        int? lapcapMasterId = 1,
        int? orgMasterId = 1,
        int? pomMasterId = 1,
        ICollection<ProducerResultFileSuggestedBillingInstruction>? instructions = null)
    {
        var ctx = new BillingRunContextBuilder.PreValidationContext
        {
            User = user,
            StartedAt = Now,
            Run = new CalculatorRun
            {
                Name = runName!,
                Classification = classification,
                CalculationRunStatus = calculationRunStatus,
                BillingRunStatus = billingRunStatus,
                BillingRunStartedAt = billingRunStartedAt,
                DefaultParameterSettingMasterId = paramMasterId,
                LapcapDataMasterId = lapcapMasterId,
                CalculatorRunOrganisationDataMasterId = orgMasterId,
                CalculatorRunPomDataMasterId = pomMasterId,
            }
        };

        instructions ??=
        [
            new ProducerResultFileSuggestedBillingInstruction { BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
        ];

        foreach (var instruction in instructions)
            ctx.Run.ProducerResultFileSuggestedBillingInstruction.Add(instruction);

        return ctx;
    }

    public static IEnumerable<TestDataRow<ICollection<ProducerResultFileSuggestedBillingInstruction>>> NoAcceptedProducersCases()
    {
        yield return new TestDataRow<ICollection<ProducerResultFileSuggestedBillingInstruction>>([])
        {
            DisplayName = "No instructions"
        };

        yield return new TestDataRow<ICollection<ProducerResultFileSuggestedBillingInstruction>>([
            new ProducerResultFileSuggestedBillingInstruction
            {
                BillingInstructionAcceptReject = BillingConstants.Action.Accepted,
                SuggestedBillingInstruction = BillingConstants.Suggestion.Cancel
            }
        ])
        {
            DisplayName = "Only Accepted/Cancel instructions"
        };

        yield return new TestDataRow<ICollection<ProducerResultFileSuggestedBillingInstruction>>([
            new ProducerResultFileSuggestedBillingInstruction
            {
                BillingInstructionAcceptReject = BillingConstants.Action.Rejected
            }
        ])
        {
            DisplayName = "Only Rejected instructions"
        };
    }
}
