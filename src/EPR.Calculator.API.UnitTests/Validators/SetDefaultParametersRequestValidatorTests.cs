using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Validators;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.API.UnitTests.Validators;

[TestClass]
public class SetDefaultParametersRequestValidatorTests
{
    private static readonly RelativeYear ValidRelativeYear = new(2024);

    // One reference ID per ParameterUnit branch (see DefaultParameterTemplateMasterExtensions.Unit) so every
    // value-parsing/range rule in the validator has master data to exercise it.
    private const string DateId = "DATE-1";
    private const string CurrencyId = "CURR-1";
    private const string PercentageIncreaseId = "PCT-1";
    private const string PercentageDecreaseId = "PCT-2";
    private const string TonnageId = "TON-1";
    private const string FactorId = "FCT-1";

    private ApplicationDBContext dbContext = null!;
    private List<DefaultParameterTemplateMaster> masterData = null!;
    private SetDefaultParametersRequestValidator validator = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        dbContext = new ApplicationDBContext(options);

        masterData =
        [
            // ParameterCategory containing "date" selects the Date unit.
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = DateId,
                ParameterType = "Date",
                ParameterCategory = "Reporting Date",
                ValidRangeFrom = 0m,
                ValidRangeTo = 0m
            },
            // ParameterType containing "costs" selects the Currency unit.
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = CurrencyId,
                ParameterType = "Fixed Costs",
                ParameterCategory = "Amount",
                ValidRangeFrom = 0m,
                ValidRangeTo = 10000m
            },
            // ParameterCategory containing "percent" selects the Percentage unit; a non-negative ValidRangeFrom
            // exercises the "increase" range-formatting branch of ValidatePercentage.
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = PercentageIncreaseId,
                ParameterType = "Percentage Increase",
                ParameterCategory = "Percentage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 100m
            },
            // A negative ValidRangeFrom exercises the "decrease" range-formatting branch of ValidatePercentage.
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = PercentageDecreaseId,
                ParameterType = "Percentage Decrease",
                ParameterCategory = "Percentage",
                ValidRangeFrom = -50m,
                ValidRangeTo = 0m
            },
            // ParameterType containing "tonnage" (without matching percentage/currency) selects the Tonnage unit.
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = TonnageId,
                ParameterType = "Tonnage",
                ParameterCategory = "Weight",
                ValidRangeFrom = 0m,
                ValidRangeTo = 1000m
            },
            // Type and category both containing "factor" select the Factor unit, which falls back to plain
            // decimal validation (there is no dedicated ValidateFactor branch).
            new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = FactorId,
                ParameterType = "Factor",
                ParameterCategory = "Factor",
                ValidRangeFrom = 0m,
                ValidRangeTo = 10m
            }
        ];

        dbContext.DefaultParameterTemplateMasterList.AddRange(masterData);
        dbContext.CalculatorRunRelativeYears.Add(new CalculatorRunRelativeYear { Value = ValidRelativeYear });
        dbContext.SaveChanges();

        validator = new SetDefaultParametersRequestValidator(dbContext);
    }

    [TestCleanup]
    public void TearDown()
    {
        dbContext.Database.EnsureDeleted();
        dbContext.Dispose();
    }

    [TestMethod]
    public async Task Validate_ReturnsValid_WhenRequestIsCompleteAndWithinRanges()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenRelativeYearIsNull()
    {
        // Arrange
        var request = CreateValidRequest() with { RelativeYear = null };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RelativeYear);
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenRelativeYearNotInDatabase()
    {
        // Arrange
        var request = CreateValidRequest() with { RelativeYear = new RelativeYear(2099) };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RelativeYear)
            .WithErrorMessage(CommonResources.NoDataForSpecifiedYear);
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenFilenameIsEmpty()
    {
        // Arrange
        var request = CreateValidRequest() with { Filename = string.Empty };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Filename)
            .WithErrorMessage(CommonResources.FileNameRequired);
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenFilenameIsNull()
    {
        // Arrange
        var request = CreateValidRequest() with { Filename = null };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Filename)
            .WithErrorMessage(CommonResources.FileNameRequired);
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenFilenameExceedsMaxLength()
    {
        // Arrange
        var request = CreateValidRequest() with { Filename = new string(c: 'a', count: 257) };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Filename)
            .WithErrorMessage(CommonResources.MaxFileNameLength);
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenParametersIsEmpty()
    {
        // Arrange
        var request = CreateValidRequest() with { Parameters = [] };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Parameters);
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenParametersIsNull()
    {
        // Arrange
        var request = CreateValidRequest() with { Parameters = null };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Parameters);
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenDuplicateParameterIdsExist()
    {
        // Arrange
        var parameters = CreateValidParameters();
        var duplicateIndex = parameters.Count;
        // Different casing confirms duplicates are detected case-insensitively.
        parameters.Add(new SetDefaultParametersRequest.ParameterValue { Id = "date-1", Value = "01/01/2024" });
        var request = CreateValidRequest() with { Parameters = [..parameters] };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor($"{nameof(SetDefaultParametersRequest.Parameters)}[{duplicateIndex}]")
            .WithErrorMessage("The parameter date-1 is duplicated. Remove any duplicated rows in the file.");
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenTemplateParameterIsMissingFromRequest()
    {
        // Arrange
        var parameters = CreateValidParameters();
        parameters.RemoveAll(p => p.Id == FactorId);
        var request = CreateValidRequest() with { Parameters = [..parameters] };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Parameters)
            .WithErrorMessage($"The parameter {FactorId} is missing. Add the parameter to the file.");
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenParameterIdDoesNotExistInTemplate()
    {
        // Arrange
        var parameters = CreateValidParameters();
        var unexpectedIndex = parameters.Count;
        parameters.Add(new SetDefaultParametersRequest.ParameterValue { Id = "UNKNOWN-1", Value = "100" });
        var request = CreateValidRequest() with { Parameters = [..parameters] };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(
                $"{nameof(SetDefaultParametersRequest.Parameters)}[{unexpectedIndex}].{nameof(SetDefaultParametersRequest.ParameterValue.Id)}")
            .WithErrorMessage("The parameter UNKNOWN-1 is an unexpected parameter. Remove it from the file.");
    }

    [TestMethod]
    public async Task Validate_AcceptsParameterIdMatchingTemplateIgnoringCase()
    {
        // Arrange
        var parameters = CreateValidParameters();
        var index = parameters.FindIndex(p => p.Id == DateId);
        parameters[index] = parameters[index] with { Id = DateId.ToLowerInvariant() };
        var request = CreateValidRequest() with { Parameters = [..parameters] };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenParameterIdIsEmpty()
    {
        // Arrange
        var parameters = CreateValidParameters();
        var index = parameters.FindIndex(p => p.Id == DateId);
        parameters[index] = parameters[index] with { Id = string.Empty };
        var request = CreateValidRequest() with { Parameters = [..parameters] };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(
            $"{nameof(SetDefaultParametersRequest.Parameters)}[{index}].{nameof(SetDefaultParametersRequest.ParameterValue.Id)}");
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenParameterIdExceedsMaxLength()
    {
        // Arrange
        var parameters = CreateValidParameters();
        var index = parameters.FindIndex(p => p.Id == DateId);
        parameters[index] = parameters[index] with { Id = new string(c: 'a', count: 401) };
        var request = CreateValidRequest() with { Parameters = [..parameters] };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(
            $"{nameof(SetDefaultParametersRequest.Parameters)}[{index}].{nameof(SetDefaultParametersRequest.ParameterValue.Id)}");
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenParameterValueIsBlank()
    {
        // Arrange
        var parameters = CreateValidParameters();
        var index = parameters.FindIndex(p => p.Id == DateId);
        parameters[index] = parameters[index] with { Value = string.Empty };
        var request = CreateValidRequest() with { Parameters = [..parameters] };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        // Value is covered by both an unconditional rule and a template-aware dependent rule, so a blank value
        // can raise more than one failure here; only the template-aware message is asserted.
        result.ShouldHaveValidationErrorFor(
                $"{nameof(SetDefaultParametersRequest.Parameters)}[{index}].{nameof(SetDefaultParametersRequest.ParameterValue.Value)}")
            .WithErrorMessage($"The parameter {DateId} is blank. Add a value for it.");
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenParameterValueExceedsMaxLength()
    {
        // Arrange
        var parameters = CreateValidParameters();
        var index = parameters.FindIndex(p => p.Id == DateId);
        parameters[index] = parameters[index] with { Value = new string(c: 'a', count: 401) };
        var request = CreateValidRequest() with { Parameters = [..parameters] };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(
            $"{nameof(SetDefaultParametersRequest.Parameters)}[{index}].{nameof(SetDefaultParametersRequest.ParameterValue.Value)}");
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenDateValueIsInvalidFormat()
    {
        // Arrange
        var parameters = CreateValidParameters();
        var index = parameters.FindIndex(p => p.Id == DateId);
        parameters[index] = parameters[index] with { Value = "not-a-date" };
        var request = CreateValidRequest() with { Parameters = [..parameters] };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(
                $"{nameof(SetDefaultParametersRequest.Parameters)}[{index}].{nameof(SetDefaultParametersRequest.ParameterValue.Value)}")
            .WithErrorMessage($"The parameter {DateId} is not a valid date value. Enter a valid date (dd/MM/yyyy) or 'NA'.");
    }

    [TestMethod]
    public async Task Validate_ReturnsValid_WhenDateValueIsNA()
    {
        // Arrange
        var parameters = CreateValidParameters();
        var index = parameters.FindIndex(p => p.Id == DateId);
        // Lowercase confirms the "NA" sentinel is matched case-insensitively.
        parameters[index] = parameters[index] with { Value = "na" };
        var request = CreateValidRequest() with { Parameters = [..parameters] };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenCurrencyValueIsInvalidFormat()
    {
        // Arrange
        var parameters = CreateValidParameters();
        var index = parameters.FindIndex(p => p.Id == CurrencyId);
        parameters[index] = parameters[index] with { Value = "abc" };
        var request = CreateValidRequest() with { Parameters = [..parameters] };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(
                $"{nameof(SetDefaultParametersRequest.Parameters)}[{index}].{nameof(SetDefaultParametersRequest.ParameterValue.Value)}")
            .WithErrorMessage($"The parameter {CurrencyId} is not a valid currency value. It can only include numbers, decimal points and a pound symbol (£).");
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenCurrencyValueIsOutsideRange()
    {
        // Arrange
        var parameters = CreateValidParameters();
        var index = parameters.FindIndex(p => p.Id == CurrencyId);
        parameters[index] = parameters[index] with { Value = "£10,000.01" };
        var request = CreateValidRequest() with { Parameters = [..parameters] };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(
                $"{nameof(SetDefaultParametersRequest.Parameters)}[{index}].{nameof(SetDefaultParametersRequest.ParameterValue.Value)}")
            .WithErrorMessage($"The parameter {CurrencyId} must be between £0.00 and £10,000.00.");
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenPercentageValueIsInvalidFormat()
    {
        // Arrange
        var parameters = CreateValidParameters();
        var index = parameters.FindIndex(p => p.Id == PercentageIncreaseId);
        parameters[index] = parameters[index] with { Value = "abc%" };
        var request = CreateValidRequest() with { Parameters = [..parameters] };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(
                $"{nameof(SetDefaultParametersRequest.Parameters)}[{index}].{nameof(SetDefaultParametersRequest.ParameterValue.Value)}")
            .WithErrorMessage($"The parameter {PercentageIncreaseId} is not a valid percentage value. It can only include numbers, commas, decimal points and a percentage symbol (%).");
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenPercentageIncreaseValueIsOutsideRange()
    {
        // Arrange
        var parameters = CreateValidParameters();
        var index = parameters.FindIndex(p => p.Id == PercentageIncreaseId);
        parameters[index] = parameters[index] with { Value = "150" };
        var request = CreateValidRequest() with { Parameters = [..parameters] };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(
                $"{nameof(SetDefaultParametersRequest.Parameters)}[{index}].{nameof(SetDefaultParametersRequest.ParameterValue.Value)}")
            .WithErrorMessage($"The parameter {PercentageIncreaseId} must be between 0% and 100%.");
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenPercentageDecreaseValueIsOutsideRange()
    {
        // Arrange
        var parameters = CreateValidParameters();
        var index = parameters.FindIndex(p => p.Id == PercentageDecreaseId);
        parameters[index] = parameters[index] with { Value = "-60" };
        var request = CreateValidRequest() with { Parameters = [..parameters] };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(
                $"{nameof(SetDefaultParametersRequest.Parameters)}[{index}].{nameof(SetDefaultParametersRequest.ParameterValue.Value)}")
            .WithErrorMessage($"The parameter {PercentageDecreaseId} must be between -50% and 0%.");
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenTonnageValueIsInvalidFormat()
    {
        // Arrange
        var parameters = CreateValidParameters();
        var index = parameters.FindIndex(p => p.Id == TonnageId);
        parameters[index] = parameters[index] with { Value = "abcT" };
        var request = CreateValidRequest() with { Parameters = [..parameters] };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(
                $"{nameof(SetDefaultParametersRequest.Parameters)}[{index}].{nameof(SetDefaultParametersRequest.ParameterValue.Value)}")
            .WithErrorMessage($"The parameter {TonnageId} is not a valid tonnage value. It can only include numbers, decimal points and a tonnage symbol (t).");
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenTonnageValueIsOutsideRange()
    {
        // Arrange
        var parameters = CreateValidParameters();
        var index = parameters.FindIndex(p => p.Id == TonnageId);
        parameters[index] = parameters[index] with { Value = "1500 t" };
        var request = CreateValidRequest() with { Parameters = [..parameters] };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(
                $"{nameof(SetDefaultParametersRequest.Parameters)}[{index}].{nameof(SetDefaultParametersRequest.ParameterValue.Value)}")
            .WithErrorMessage($"The parameter {TonnageId} must be between 0 t and 1,000 t.");
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenDecimalValueIsInvalidFormat()
    {
        // Arrange
        var parameters = CreateValidParameters();
        var index = parameters.FindIndex(p => p.Id == FactorId);
        parameters[index] = parameters[index] with { Value = "abc" };
        var request = CreateValidRequest() with { Parameters = [..parameters] };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(
                $"{nameof(SetDefaultParametersRequest.Parameters)}[{index}].{nameof(SetDefaultParametersRequest.ParameterValue.Value)}")
            .WithErrorMessage($"The parameter {FactorId} is not a valid decimal value. It can only include numbers and decimal points.");
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenDecimalValueIsOutsideRange()
    {
        // Arrange
        var parameters = CreateValidParameters();
        var index = parameters.FindIndex(p => p.Id == FactorId);
        parameters[index] = parameters[index] with { Value = "15" };
        var request = CreateValidRequest() with { Parameters = [..parameters] };

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(
                $"{nameof(SetDefaultParametersRequest.Parameters)}[{index}].{nameof(SetDefaultParametersRequest.ParameterValue.Value)}")
            .WithErrorMessage($"The parameter {FactorId} must be between 0 and 10.");
    }

    private SetDefaultParametersRequest CreateValidRequest()
    {
        return new SetDefaultParametersRequest
        {
            RelativeYear = ValidRelativeYear,
            Filename = "default-parameters.csv",
            Parameters = CreateValidParameters().ToImmutableList()
        };
    }

    private List<SetDefaultParametersRequest.ParameterValue> CreateValidParameters()
    {
        return masterData
            .Select(m => new SetDefaultParametersRequest.ParameterValue
            {
                Id = m.ParameterUniqueReferenceId,
                Value = ValidValueFor(m)
            })
            .ToList();
    }

    private static string ValidValueFor(DefaultParameterTemplateMaster master) => master.Unit switch
    {
        ParameterUnit.Date => "01/01/2024",
        ParameterUnit.Currency => "£1,500.50",
        ParameterUnit.Percentage => master.ValidRangeFrom >= 0 ? "50" : "-25",
        ParameterUnit.Tonnage => "500 t",
        _ => "5"
    };
}
