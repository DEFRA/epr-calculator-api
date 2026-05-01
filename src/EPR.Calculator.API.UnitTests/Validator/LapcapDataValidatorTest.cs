using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
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
        context.LapcapDataTemplateMaster.Add(new LapcapDataTemplateMaster
        {
            UniqueReference = "ENG-AL",
            Country = "England",
            Material = "Aluminium",
            TotalCostFrom = 0m,
            TotalCostTo = 1000m,
        });
        context.SaveChanges();
        validator = new LapcapDataValidator(context);
    }

    [TestMethod]
    public void Validate_ShouldReturnIsInvalidAsTrue_WhenTotalCostIsEmpty()
    {
        var dto = CreateDto(totalCost: string.Empty);

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
        var dto = CreateDto(totalCost: "500");

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
        var dto = CreateDto(totalCost: "9999");

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
        var dto = CreateDto(totalCost: "not-a-number");

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.IsTrue(result.IsInvalid);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual("Aluminium", result.Errors[0].Material);
    }

    private static CreateLapcapDataDto CreateDto(string totalCost) => new()
    {
        RelativeYear = new RelativeYear(2024),
        LapcapFileName = "test.csv",
        LapcapDataTemplateValues =
        [
            new LapcapDataTemplateValueDto
            {
                CountryName = "England",
                Material = "Aluminium",
                TotalCost = totalCost,
            },
        ],
    };
}
