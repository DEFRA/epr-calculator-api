using EPR.Calculator.API.BackgroundService.Enums;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers;
using EPR.Calculator.API.Data.DataModels;
using FluentValidation.TestHelper;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Features.Calculator.Contexts;

[TestCategory(TestCategories.CalculatorRuns)]
[TestClass]
public class CalculatorRunValidatorTests : TestsFor<CalculatorRunValidator>
{
    [DataRow(RunClassification.INTHEQUEUE)]
    [DataRow(RunClassification.RUNNING)]
    [TestMethod]
    public void Should_not_error_when_run_is_valid(RunClassification classification)
    {
        var run = new CalculatorRun
        {
            CalculatorRunClassificationId = (int)classification,
            Name = "TestRun",
            DefaultParameterSettingMasterId = 1,
            LapcapDataMasterId = 1
        };

        var result = testSubject.TestValidate(run);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [DataRow(RunClassification.UNCLASSIFIED)]
    [DataRow(RunClassification.ERROR)]
    [DataRow(RunClassification.DELETED)]
    [TestMethod]
    public void Should_error_when_classification_is_invalid(RunClassification classification)
    {
        var run = new CalculatorRun
        {
            CalculatorRunClassificationId = (int)classification,
            Name = "TestRun",
            DefaultParameterSettingMasterId = 1,
            LapcapDataMasterId = 1
        };

        var result = testSubject.TestValidate(run);

        result.ShouldHaveValidationErrorFor(r => r.CalculatorRunClassificationId);
    }

    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [TestMethod]
    public void Should_error_for_empty_Name(string? name)
    {
        var run = new CalculatorRun
        {
            CalculatorRunClassificationId = (int)RunClassification.INTHEQUEUE,
            Name = name!,
            DefaultParameterSettingMasterId = 1,
            LapcapDataMasterId = 1
        };

        var result = testSubject.TestValidate(run);

        result.ShouldHaveValidationErrorFor(r => r.Name);
    }

    [DataRow(null)]
    [DataRow(0)]
    [TestMethod]
    public void Should_error_for_empty_DefaultParameterSettingMasterId(int? id)
    {
        var run = new CalculatorRun
        {
            CalculatorRunClassificationId = (int)RunClassification.INTHEQUEUE,
            Name = "TestRun",
            DefaultParameterSettingMasterId = id,
            LapcapDataMasterId = 1
        };

        var result = testSubject.TestValidate(run);

        result.ShouldHaveValidationErrorFor(r => r.DefaultParameterSettingMasterId);
    }

    [DataRow(null)]
    [DataRow(0)]
    [TestMethod]
    public void Should_error_for_empty_LapcapDataMasterId(int? id)
    {
        var run = new CalculatorRun
        {
            CalculatorRunClassificationId = (int)RunClassification.INTHEQUEUE,
            Name = "TestRun",
            DefaultParameterSettingMasterId = 1,
            LapcapDataMasterId = id
        };

        var result = testSubject.TestValidate(run);

        result.ShouldHaveValidationErrorFor(r => r.LapcapDataMasterId);
    }

    [TestMethod]
    public void Should_error_for_existing_OrgPomDataLoadedAt()
    {
        var run = new CalculatorRun
        {
            CalculatorRunClassificationId = (int)RunClassification.INTHEQUEUE,
            Name = "TestRun",
            DefaultParameterSettingMasterId = 1,
            LapcapDataMasterId = 1,
            OrgPomDataLoadedAt = DateTime.UtcNow
        };

        var result = testSubject.TestValidate(run);

        result.ShouldHaveValidationErrorFor(r => r.OrgPomDataLoadedAt);
    }
}
