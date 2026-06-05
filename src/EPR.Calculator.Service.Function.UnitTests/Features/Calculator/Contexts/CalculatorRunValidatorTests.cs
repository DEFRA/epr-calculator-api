using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Features.CalculatorRun.Contexts;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using FluentValidation.TestHelper;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Calculator.Contexts;

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
    public void Should_error_for_existing_CalculatorRunOrganisationDataMasterId()
    {
        var run = new CalculatorRun
        {
            CalculatorRunClassificationId = (int)RunClassification.INTHEQUEUE,
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
            CalculatorRunClassificationId = (int)RunClassification.INTHEQUEUE,
            Name = "TestRun",
            DefaultParameterSettingMasterId = 1,
            LapcapDataMasterId = 1,
            CalculatorRunPomDataMasterId = 1
        };

        var result = testSubject.TestValidate(run);

        result.ShouldHaveValidationErrorFor(r => r.CalculatorRunPomDataMasterId);
    }
}
