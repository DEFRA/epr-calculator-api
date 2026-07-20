using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Mappers;
using EPR.Calculator.API.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.API.UnitTests.Mappers;

[TestClass]
public class RelativeYearClassificationsMapperTests
{
    private IFixture Fixture { get; } = TestFixtures.New();

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
        result.RelativeYear.ShouldBe(new RelativeYear(2024));
        result.Classifications.Count.ShouldBe(classifications.Count);
        result.ClassifiedRuns.ShouldBeEmpty();

        result.Classifications[0].Id.ShouldBe(1);
        result.Classifications[0].Status.ShouldBe("RUNNING");
        result.Classifications[1].Id.ShouldBe(2);
        result.Classifications[1].Status.ShouldBe("UNCLASSIFIED");
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
        result.RelativeYear.ShouldBe(new RelativeYear(2024));
        result.Classifications.Count.ShouldBe(classifications.Count);
        result.ClassifiedRuns.ShouldBeEmpty();
    }

    [TestMethod]
    public void Map_WhenGivenCalculatorRuns_ReturnsExpectedClassificationsAndRuns()
    {
        // Arrange
        var classifications = this.Fixture.Create<List<CalculatorRunClassification>>();
        var runs = this.Fixture.Create<List<CalculatorRunDto>>();

        // Act
        var result = RelativeYearClassificationsMapper.Map(new RelativeYear(2024), classifications, runs);

        // Assert
        Assert.IsInstanceOfType<RelativeYearClassificationResponseDto>(result);
        result.RelativeYear.ShouldBe(new RelativeYear(2024));
        result.Classifications.Count.ShouldBe(classifications.Count);
        result.ClassifiedRuns.Count.ShouldBe(runs.Count);
        result.ClassifiedRuns.ShouldBeEquivalentTo(runs);
    }
}
