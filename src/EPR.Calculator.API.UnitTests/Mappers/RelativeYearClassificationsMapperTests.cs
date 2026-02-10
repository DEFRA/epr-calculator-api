using AutoFixture;
using EPR.Calculator.API.Data.DataModels;
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
    public void Map_WhenNotGivenCalculatorRuns_ReturnsExpectedClassifications()
    {
        // Arrange
        List<CalculatorRunClassification> classifications =
        [
            new() { Id = 1, Status = "RUNNING" },
            new() { Id = 2, Status = "UNCLASSIFIED" }
        ];

        // Act
        var result = RelativeYearClassificationsMapper.Map(new RelativeYear(2024), classifications);

        // Assert
        Assert.IsInstanceOfType<RelativeYearClassificationResponseDto>(result);
        result.RelativeYear.Should().Be(new RelativeYear(2024));
        result.Classifications.Count.Should().Be(classifications.Count);
        result.ClassifiedRuns.Should().BeEmpty();

        result.Classifications.Should().SatisfyRespectively(
            first =>
            {
                first.Id.Should().Be(1);
                first.Status.Should().Be("RUNNING");
            },
            second =>
            {
                second.Id.Should().Be(2);
                second.Status.Should().Be("UNCLASSIFIED");
            });
    }

    [TestMethod]
    public void Map_WhenGivenNullCalculatorRuns_ReturnsExpectedClassifications()
    {
        // Arrange
        var classifications = this.Fixture.Create<List<CalculatorRunClassification>>();

        // Act
        var result = RelativeYearClassificationsMapper.Map(new RelativeYear(2024), classifications, null);

        // Assert
        Assert.IsInstanceOfType<RelativeYearClassificationResponseDto>(result);
        result.RelativeYear.Should().Be(new RelativeYear(2024));
        result.Classifications.Count.Should().Be(classifications.Count);
        result.ClassifiedRuns.Should().BeEmpty();
    }

    [TestMethod]
    public void Map_WhenGivenCalculatorRuns_ReturnsExpectedClassificationsAndRuns()
    {
        // Arrange
        var classifications = this.Fixture.Create<List<CalculatorRunClassification>>();
        var runs = this.Fixture.Create<List<ClassifiedCalculatorRunDto>>();

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