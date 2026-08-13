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
public class CreateLapcapDataRequestValidatorTests
{
    private static readonly RelativeYear ValidRelativeYear = new(2024);

    private ApplicationDBContext dbContext = null!;
    private List<LapcapDataTemplateMaster> masterData = null!;
    private CreateLapcapDataRequestValidator validator = null!;

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
            new LapcapDataTemplateMaster
            {
                UniqueReference = "ENG-AL",
                Country = "England",
                Material = "Aluminium",
                TotalCostFrom = -999999999.99m,
                TotalCostTo = 999999999.99m
            },
            new LapcapDataTemplateMaster
            {
                UniqueReference = "ENG-PL",
                Country = "England",
                Material = "Plastic",
                TotalCostFrom = -999999999.99m,
                TotalCostTo = 999999999.99m
            },
            new LapcapDataTemplateMaster
            {
                UniqueReference = "WLS-AL",
                Country = "Wales",
                Material = "Aluminium",
                TotalCostFrom = -999999999.99m,
                TotalCostTo = 999999999.99m
            },
            new LapcapDataTemplateMaster
            {
                UniqueReference = "WLS-PL",
                Country = "Wales",
                Material = "Plastic",
                TotalCostFrom = -999999999.99m,
                TotalCostTo = 999999999.99m
            }
        ];

        dbContext.LapcapDataTemplateMaster.AddRange(masterData);
        dbContext.CalculatorRunRelativeYears.Add(new CalculatorRunRelativeYear { Value = ValidRelativeYear });
        dbContext.SaveChanges();

        validator = new CreateLapcapDataRequestValidator(dbContext);
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
        var request = CreateValidRequest();

        var result = await validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenRelativeYearIsNull()
    {
        var request = CreateValidRequest() with { RelativeYear = null };

        var result = await validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.RelativeYear);
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenRelativeYearNotInDatabase()
    {
        var request = CreateValidRequest() with { RelativeYear = new RelativeYear(2099) };

        var result = await validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.RelativeYear)
            .WithErrorMessage(CommonResources.NoDataForSpecifiedYear);
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenLapcapFileNameIsEmpty()
    {
        var request = CreateValidRequest() with { Filename = string.Empty };

        var result = await validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Filename)
            .WithErrorMessage(CommonResources.FileNameRequired);
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenLapcapFileNameIsNull()
    {
        var request = CreateValidRequest() with { Filename = null };

        var result = await validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Filename)
            .WithErrorMessage(CommonResources.FileNameRequired);
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenLapcapFileNameExceedsMaxLength()
    {
        var request = CreateValidRequest() with { Filename = new string(c: 'a', count: 257) };

        var result = await validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Filename)
            .WithErrorMessage(CommonResources.MaxFileNameLength);
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenValuesIsEmpty()
    {
        var request = CreateValidRequest() with { Values = [] };

        var result = await validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Values);
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenValuesIsNull()
    {
        var request = CreateValidRequest() with { Values = null };

        var result = await validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Values);
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenValueCountryIsEmpty()
    {
        var values = CreateValidValues();
        values[0] = values[0] with { Country = string.Empty };
        var request = CreateValidRequest() with { Values = [..values] };

        var result = await validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor($"{nameof(CreateLapcapDataRequest.Values)}[0].{nameof(CreateLapcapDataRequest.LapcapValue.Country)}");
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenValueMaterialIsEmpty()
    {
        var values = CreateValidValues();
        values[0] = values[0] with { Material = string.Empty };
        var request = CreateValidRequest() with { Values = [..values] };

        var result = await validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor($"{nameof(CreateLapcapDataRequest.Values)}[0].{nameof(CreateLapcapDataRequest.LapcapValue.Material)}");
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenValueTotalCostIsNull()
    {
        var values = CreateValidValues();
        values[0] = values[0] with { TotalCost = null };
        var request = CreateValidRequest() with { Values = [..values] };

        var result = await validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor($"{nameof(CreateLapcapDataRequest.Values)}[0].{nameof(CreateLapcapDataRequest.LapcapValue.TotalCost)}");
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenValueCountryExceedsMaxLength()
    {
        var values = CreateValidValues();
        values[0] = values[0] with { Country = new string(c: 'c', count: 401) };
        var request = CreateValidRequest() with { Values = [..values] };

        var result = await validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor($"{nameof(CreateLapcapDataRequest.Values)}[0].{nameof(CreateLapcapDataRequest.LapcapValue.Country)}");
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenValueMaterialExceedsMaxLength()
    {
        var values = CreateValidValues();
        values[0] = values[0] with { Material = new string(c: 'm', count: 401) };
        var request = CreateValidRequest() with { Values = [..values] };

        var result = await validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor($"{nameof(CreateLapcapDataRequest.Values)}[0].{nameof(CreateLapcapDataRequest.LapcapValue.Material)}");
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenDuplicateCountryMaterialCombinationsExist()
    {
        var request = CreateValidRequest() with
        {
            Values =
            [
                ..CreateValidValues(),
                new CreateLapcapDataRequest.LapcapValue
                {
                    Country = "England",
                    Material = "Aluminium",
                    TotalCost = 5m
                }
            ]
        };

        var result = await validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor($"{nameof(CreateLapcapDataRequest.Values)}[4]")
            .WithErrorMessage("You have entered the total cost for Aluminium in England more than once." +
                              " Make sure there is only one entry for Aluminium in England.");
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenNationTotalIsNegative()
    {
        var values = CreateValidValues();
        // England Aluminium + Plastic = 10 + (-120) = -110
        // Keep material totals non-negative so NationTotals is the rule under test
        // (CascadeMode.Stop would otherwise skip later Values rules).
        values[0] = values[0] with { TotalCost = 10m };   // England Aluminium
        values[1] = values[1] with { TotalCost = -120m }; // England Plastic
        values[2] = values[2] with { TotalCost = 10m };   // Wales Aluminium
        values[3] = values[3] with { TotalCost = 130m };  // Wales Plastic
        var request = CreateValidRequest() with { Values = [..values] };

        var result = await validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Values)
            .WithErrorMessage("The overall total disposal cost for England is negative (-£110.00).");
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenMaterialTotalIsNegative()
    {
        var values = CreateValidValues();
        // Aluminium England + Wales = 10 + (-120) = -110
        // Keep nation totals non-negative so MaterialTotals is reached under CascadeMode.Stop.
        values[0] = values[0] with { TotalCost = 10m };   // England Aluminium
        values[1] = values[1] with { TotalCost = 10m };   // England Plastic
        values[2] = values[2] with { TotalCost = -120m }; // Wales Aluminium
        values[3] = values[3] with { TotalCost = 130m };  // Wales Plastic
        var request = CreateValidRequest() with { Values = [..values] };

        var result = await validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Values)
            .WithErrorMessage("The overall total disposal cost for Aluminium is negative (-£110.00).");
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenMasterCountryMaterialIsMissing()
    {
        var values = CreateValidValues().Where(v => v is not { Country: "England", Material: "Aluminium" });
        var request = CreateValidRequest() with { Values = [..values] };

        var result = await validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Values)
            .WithErrorMessage("The total cost for Aluminium in England is missing. Enter the total cost for Aluminium in England.");
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenCountryMaterialDoesNotExistInMaster()
    {
        var values = CreateValidValues();
        values.Add(new CreateLapcapDataRequest.LapcapValue
        {
            Country = "France",
            Material = "Aluminium",
            TotalCost = 10m
        });
        var request = CreateValidRequest() with { Values = values.ToImmutableList() };

        var result = await validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrors();
        result.Errors[0].ErrorMessage.ShouldBe("The country and material combination France/Aluminium does not exist in the master template.");
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenTotalCostIsBelowMasterRange()
    {
        var values = CreateValidValues();
        values[0] = values[0] with { TotalCost = -1000000000m }; // England Aluminium (below min)
        var request = CreateValidRequest() with { Values = [..values] };

        var result = await validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor($"{nameof(CreateLapcapDataRequest.Values)}[0].{nameof(CreateLapcapDataRequest.LapcapValue.TotalCost)}")
            .WithErrorMessage("The total cost for Aluminium in England is invalid. Enter a total cost between -£999,999,999.99 and £999,999,999.99.");
    }

    [TestMethod]
    public async Task Validate_ReturnsError_WhenTotalCostIsAboveMasterRange()
    {
        var values = CreateValidValues();
        values[0] = values[0] with { TotalCost = 1000000000m }; // England Aluminium (above max)
        var request = CreateValidRequest() with { Values = [..values] };

        var result = await validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor($"{nameof(CreateLapcapDataRequest.Values)}[0].{nameof(CreateLapcapDataRequest.LapcapValue.TotalCost)}")
            .WithErrorMessage("The total cost for Aluminium in England is invalid. Enter a total cost between -£999,999,999.99 and £999,999,999.99.");
    }

    [TestMethod]
    public async Task Validate_ReturnsValid_WhenTotalCostIsAtMasterRangeBoundaries()
    {
        var values = CreateValidValues();
        values[0] = values[0] with { TotalCost = -999999999.99m }; // England Aluminium (min)
        values[1] = values[1] with { TotalCost = 999999999.99m };  // England Plastic (max)
        values[2] = values[2] with { TotalCost = 999999999.99m };  // Wales Aluminium
        values[3] = values[3] with { TotalCost = 0m };             // Wales Plastic
        var request = CreateValidRequest() with { Values = [..values] };

        var result = await validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public async Task Validate_AcceptsCountryMaterialMatchingMasterIgnoringCase()
    {
        var values = CreateValidValues();
        values[0] = values[0] with { Country = "ENGLAND", Material = "ALUMINIUM" };
        var request = CreateValidRequest() with { Values = [..values] };

        var result = await validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private CreateLapcapDataRequest CreateValidRequest()
    {
        return new CreateLapcapDataRequest
        {
            RelativeYear = ValidRelativeYear,
            Filename = "lapcap-data.csv",
            Values = CreateValidValues().ToImmutableList()
        };
    }

    private List<CreateLapcapDataRequest.LapcapValue> CreateValidValues()
    {
        return masterData
            .Select(m => new CreateLapcapDataRequest.LapcapValue
            {
                Country = m.Country,
                Material = m.Material,
                TotalCost = 10m
            })
            .ToList();
    }
}
