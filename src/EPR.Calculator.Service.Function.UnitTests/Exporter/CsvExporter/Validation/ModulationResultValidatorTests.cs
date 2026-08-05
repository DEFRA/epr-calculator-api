using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Validation;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using FluentValidation;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.Validation;

[TestClass]
public class ModulationResultValidatorTests : TestsFor<ModulationResultValidator>
{
    public static IEnumerable<object[]> PositiveCases =>
    [
        [ 0m ],
        [ 0.000001m ],
        [ 2m ]
    ];

    [DynamicData(nameof(PositiveCases))]
    [TestMethod]
    public void Should_be_valid_when_green_factor_is_positive(decimal greenFactor)
    {
        // Arrange
        var modulation = CreateModulationResult(greenFactor);

        // Act
        var result = testSubject.Validate(modulation);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    public static IEnumerable<object[]> NegativeCases =>
    [
        [ -0.000001m ],
        [ -2m ]
    ];

    [DynamicData(nameof(NegativeCases))]
    [TestMethod]
    public void Should_have_warning_when_green_factor_is_negative(decimal greenFactor)
    {
        // Arrange - negative green factor is a warning, not an error
        var modulation = CreateModulationResult(greenFactor);

        // Act
        var result = testSubject.Validate(modulation);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldHaveSingleItem();
        result.Errors[0].Severity.ShouldBe(Severity.Warning);
    }

    private static ModulationResult CreateModulationResult(decimal greenFactor)
    {
        return new ModulationResult
        {
            CalculatorRunId = 1,
            GreenFactor = greenFactor,
            RedFactor = 1.5m,
            ModulationByMaterial = new Dictionary<MaterialDetail, ModulationDetail>()
        };
    }
}
