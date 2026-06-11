using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.API.UnitTests.Validator;

[TestClass]
public class LapcapDataValidatorTest
{
    private readonly LapcapDataValidator validator;

    public LapcapDataValidatorTest()
    {
        var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var context = new ApplicationDBContext(dbContextOptions);
        context.LapcapDataTemplateMaster.AddRange(new List<LapcapDataTemplateMaster>{
            new LapcapDataTemplateMaster()
            {
                UniqueReference = "ENG-AL",
                Country = "England",
                Material = "Aluminium",
                TotalCostFrom = -1000m,
                TotalCostTo = 1000m,
            },
            new LapcapDataTemplateMaster()
            {
                UniqueReference = "ENG-GL",
                Country = "England",
                Material = "Glass",
                TotalCostFrom = -1000m,
                TotalCostTo = 1000m,
            },
            new LapcapDataTemplateMaster()
            {
                UniqueReference = "WLS-AL",
                Country = "Wales",
                Material = "Aluminium",
                TotalCostFrom = -1000m,
                TotalCostTo = 1000m,
            },
            new LapcapDataTemplateMaster()
            {
                UniqueReference = "WLS-GL",
                Country = "Wales",
                Material = "Glass",
                TotalCostFrom = -1000m,
                TotalCostTo = 1000m,
            }
        });
        context.SaveChanges();
        validator = new LapcapDataValidator(context);
    }

    [TestMethod]
    public void Validate_ShouldReturnIsInvalidAsTrue_WhenTotalCostIsEmpty()
    {
        var dto = CreateDto(engAlm: string.Empty);

        var result = validator.Validate(dto);

        Assert.IsTrue(result.IsInvalid);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual("England", result.Errors[0].Country);
        Assert.AreEqual("Aluminium", result.Errors[0].Material);
    }

    [TestMethod]
    public void Validate_ShouldReturnIsInvalidAsFalse_WhenTotalCostIsWithinRange()
    {
        // Arrange
        var dto = CreateDto(engAlm: "500");

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.IsFalse(result.IsInvalid);
        Assert.IsEmpty(result.Errors);
    }

    [TestMethod]
    public void Validate_ShouldReturnIsInvalidAsTrue_WhenTotalCostExceedsRange()
    {
        // Arrange
        var dto = CreateDto(engAlm: "9999");

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.IsTrue(result.IsInvalid);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual("England", result.Errors[0].Country);
        Assert.AreEqual("Aluminium", result.Errors[0].Material);
    }

    [TestMethod]
    public void Validate_ShouldReturnIsInvalidAsTrue_WhenTotalCostIsNotNumeric()
    {
        // Arrange
        var dto = CreateDto(engAlm: "not-a-number");

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.IsTrue(result.IsInvalid);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual("Aluminium", result.Errors[0].Material);
    }

    [TestMethod]
    public void Validate_ShouldReturnIsInvalidAsFalse_WhenTotalCostIsWithinRange_Negative()
    {
        // Arrange
        var dto = CreateDto(engAlm: "600", engGlass: "-300", wlsAlm: "-100", wlsGlass: "300");

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.IsFalse(result.IsInvalid);
        Assert.IsEmpty(result.Errors);
    }

    [TestMethod]
    public void Validate_ShouldReturnIsInvalidAsTrue_WhenTotalCountryCostsAreNegative()
    {
        // Arrange
        var dto = CreateDto(engAlm: "200", engGlass: "-250", wlsAlm: "-100", wlsGlass: "300");

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.IsTrue(result.IsInvalid);
        Assert.HasCount(1, result.Errors);
    }

    [TestMethod]
    public void Validate_ShouldReturnIsInvalidAsTrue_WhenTotalMaterialCostsAreNegative()
    {
        // Arrange
        var dto = CreateDto(engAlm: "200", engGlass: "-100", wlsAlm: "300", wlsGlass: "-100");

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.IsTrue(result.IsInvalid);
        Assert.HasCount(1, result.Errors);
    }

    [TestMethod]
    public void Validate_ShouldReturnIsInvalidAsTrue_WhenBothTotalMaterialAndCountryCostsAreNegative()
    {
        // Arrange
        var dto = CreateDto(engAlm: "-200", engGlass: "-100", wlsAlm: "-300", wlsGlass: "-100");

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.IsTrue(result.IsInvalid);
        Assert.HasCount(2, result.Errors);
    }

    private static CreateLapcapDataDto CreateDto(string engAlm = "100", string engGlass = "200", string wlsAlm = "300", string wlsGlass = "400") => new()
    {
        RelativeYear = new RelativeYear(2024),
        LapcapFileName = "test.csv",
        LapcapDataTemplateValues =
        [
            new LapcapDataTemplateValueDto
            {
                CountryName = "England",
                Material = "Aluminium",
                TotalCost = engAlm,
            },
            new LapcapDataTemplateValueDto
            {
                CountryName = "England",
                Material = "Glass",
                TotalCost = engGlass,
            },
            new LapcapDataTemplateValueDto
            {
                CountryName = "Wales",
                Material = "Aluminium",
                TotalCost = wlsAlm,
            },
            new LapcapDataTemplateValueDto
            {
                CountryName = "Wales",
                Material = "Glass",
                TotalCost = wlsGlass,
            },
        ],
    };
}
