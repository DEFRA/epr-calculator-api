using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Mappers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests.Mappers;

[TestClass]
public class FinancialYearClassificationsMapperTests
{
    [TestMethod]
    public void Map_ReturnsExpectedClassifications()
    {
        // Arrange
        List<CalculatorRunClassification> classifications =
        [
            new() { Id = 1, Status = "Ready" },
            new() { Id = 2, Status = "Willing" }
        ];

        // Act
        var result = FinancialYearClassificationsMapper.Map("2024-25", classifications);

        // Assert
        Assert.IsInstanceOfType<FinancialYearClassificationResponseDto>(result);
        result.FinancialYear.Should().Be("2024-25");
        result.Classifications.Count.Should().Be(classifications.Count);
        result.ClassifiedRuns.Should().BeNull();

        result.Classifications.Should().SatisfyRespectively(
            first =>
            {
                first.Id.Should().Be(1);
                first.Status.Should().Be("Ready");
            },
            second =>
            {
                second.Id.Should().Be(2);
                second.Status.Should().Be("Willing");
            });
    }
}