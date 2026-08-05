using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.Service.Function.Features.BillingRuns.Constants;
using EPR.Calculator.Service.Function.Features.BillingRuns.Contexts;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using FluentValidation.TestHelper;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Billing.Contexts;

[TestCategory(TestCategories.BillingRuns)]
[TestClass]
public class BillingRunContextValidatorTests : TestsFor<BillingRunContextValidator>
{
    [DataRow(RunClassificationStatusIds.INITIALRUNID)]
    [DataRow(RunClassificationStatusIds.INTERIMRECALCULATIONRUNID)]
    [DataRow(RunClassificationStatusIds.FINALRECALCULATIONRUNID)]
    [DataRow(RunClassificationStatusIds.FINALRUNID)]
    [TestMethod]
    public void Should_not_error_when_run_is_valid(int classificationId)
    {
        var preValidationContext = CreatePreValidationContext(classificationId: classificationId);
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

    [DataRow(BillingRunStatus.Completed)]
    [DataRow(BillingRunStatus.Errored)]
    [DataRow(BillingRunStatus.None)]
    [DataRow(BillingRunStatus.Unknown)]
    [TestMethod]
    public void Should_error_when_billing_status_is_incorrect(BillingRunStatus status)
    {
        var preValidationContext = CreatePreValidationContext(billingRunStatus: status);
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
        int classificationId = RunClassificationStatusIds.INITIALRUNID,
        int? paramMasterId = 1,
        int? lapcapMasterId = 1,
        int? orgMasterId = 1,
        int? pomMasterId = 1,
        BillingRunStatus billingRunStatus = BillingRunStatus.Running,
        ICollection<ProducerResultFileSuggestedBillingInstruction>? instructions = null)
    {
        var ctx = new BillingRunContextBuilder.PreValidationContext
        {
            User = user,
            StartedAt = DateTime.UtcNow,
            Run = new CalculatorRun
            {
                Name = runName!,
                CalculatorRunClassificationId = classificationId,
                DefaultParameterSettingMasterId = paramMasterId,
                LapcapDataMasterId = lapcapMasterId,
                CalculatorRunOrganisationDataMasterId = orgMasterId,
                CalculatorRunPomDataMasterId = pomMasterId,
                BillingRunStatus = billingRunStatus
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
