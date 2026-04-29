using AutoFixture;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Mappers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests.Mappers;

[TestClass]
public class RelativeYearClassificationsMapperTests
{
    private Fixture Fixture { get; } = new Fixture();

    [TestMethod]
    public void Map_WhenGivenCalculatorRuns_ReturnsExpectedClassificationsAndRuns()
    {
        // Arrange
        var classifications = this.Fixture.Create<List<RunClassification>>();
        var runs = this.Fixture.Create<List<CalculatorRunDto>>();

        // Act
        var result = RelativeYearClassificationsMapper.Map(new RelativeYear(2024), classifications, runs);

        // Assert
        Assert.IsInstanceOfType<RelativeYearClassificationResponseDto>(result);
        result.RelativeYear.Should().Be(new RelativeYear(2024));
        result.Classifications.Count.Should().Be(classifications.Count);
        result.ClassifiedRuns.Should().HaveCount(runs.Count);
        result.ClassifiedRuns.Should().BeEquivalentTo(runs);
    }
}
