using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using FluentValidation.TestHelper;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Features.Calculator.Contexts;

[TestCategory(TestCategories.CalculatorRuns)]
[TestClass]
public class CalculatorRunValidatorTests : TestsFor<CalculatorRunValidator>
{
    [DataRow(RunClassification.Running)]
    [TestMethod]
    public void Should_not_error_when_run_is_valid(RunClassification classification)
    {
        var run = new CalculatorRun
        {
            Classification = classification,
            Name = "TestRun",
            DefaultParameterSettingMasterId = 1,
            LapcapDataMasterId = 1
        };

        var result = testSubject.TestValidate(run);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [DataRow(RunClassification.Unclassified)]
    [DataRow(RunClassification.Errored)]
    [DataRow(RunClassification.Deleted)]
    [TestMethod]
    public void Should_error_when_classification_is_invalid(RunClassification classification)
    {
        var run = new CalculatorRun
        {
            Classification = classification,
            Name = "TestRun",
            DefaultParameterSettingMasterId = 1,
            LapcapDataMasterId = 1
        };

        var result = testSubject.TestValidate(run);

        result.ShouldHaveValidationErrorFor(r => r.Classification);
    }

    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [TestMethod]
    public void Should_error_for_empty_Name(string? name)
    {
        var run = new CalculatorRun
        {
            Classification = RunClassification.Running,
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
            Classification = RunClassification.Running,
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
            Classification = RunClassification.Running,
            Name = "TestRun",
            DefaultParameterSettingMasterId = 1,
            LapcapDataMasterId = id
        };

        var result = testSubject.TestValidate(run);

        result.ShouldHaveValidationErrorFor(r => r.LapcapDataMasterId);
    }

    [TestMethod]
    public void Should_error_for_existing_CalculatorRunOrganisationDataMasterId()
    {
        var run = new CalculatorRun
        {
            Classification = RunClassification.Running,
            Name = "TestRun",
            DefaultParameterSettingMasterId = 1,
            LapcapDataMasterId = 1,
            CalculatorRunOrganisationDataMasterId = 1
        };

        var result = testSubject.TestValidate(run);

        result.ShouldHaveValidationErrorFor(r => r.CalculatorRunOrganisationDataMasterId);
    }

    [TestMethod]
    public void Should_error_for_existing_CalculatorRunPomDataMasterId()
    {
        var run = new CalculatorRun
        {
            Classification = RunClassification.Running,
            Name = "TestRun",
            DefaultParameterSettingMasterId = 1,
            LapcapDataMasterId = 1,
            CalculatorRunPomDataMasterId = 1
        };

        var result = testSubject.TestValidate(run);

        result.ShouldHaveValidationErrorFor(r => r.CalculatorRunPomDataMasterId);
    }
}
