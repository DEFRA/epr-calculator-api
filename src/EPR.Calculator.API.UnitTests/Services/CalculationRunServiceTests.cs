using EPR.Calculator.API.Data.Enums;

namespace EPR.Calculator.API.UnitTests.Services;

using EnumsNET;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Exceptions;
using EPR.Calculator.API.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public class CalculationRunServiceTests
{
    private ApplicationDBContext dbContext = null!;
    private CalculationRunService service = null!;
    private Mock<ILogger<CalculationRunService>> loggerMock = null!;

    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        this.dbContext = new ApplicationDBContext(options);

        this.dbContext.CalculatorRunRelativeYears.Add(new CalculatorRunRelativeYear
        {
            Value = 2024,
        });

        this.dbContext.SaveChanges();

        this.loggerMock = new Mock<ILogger<CalculationRunService>>();
        this.service = new CalculationRunService(this.dbContext);
    }

    [TestCleanup]
    public void Cleanup()
    {
        this.dbContext.Dispose();
    }

    [TestMethod]
    [DataRow(RunClassification.None)]
    [DataRow(RunClassification.Running)]
    [DataRow(RunClassification.Unclassified)]
    [DataRow(RunClassification.TestRun)]
    [DataRow(RunClassification.Errored)]
    [DataRow(RunClassification.Deleted)]
    [DataRow(RunClassification.InitialRunCompleted)]
    [DataRow(RunClassification.InitialRun)]
    [DataRow(RunClassification.InterimRecalculationRun)]
    [DataRow(RunClassification.FinalRun)]
    [DataRow(RunClassification.FinalRecalculationRun)]
    [DataRow(RunClassification.InterimRecalculationRunCompleted)]
    [DataRow(RunClassification.FinalRecalculationRunCompleted)]
    [DataRow(RunClassification.FinalRunCompleted)]
    public async Task GetDesignatedRunsByFinanialYear_ExcludesRunsInWrongRelativeYear(RunClassification classification)
    {
        // Arrange
        this.AddRunToDb(classification, requestId: 1, 1923);

        // Act
        var result = await this.service.GetDesignatedRunsByFinanialYear(new RelativeYear(2024), TestContext.CancellationTokenSource.Token);

        // Assert
        result.Should().HaveCount(0);
    }

    [TestMethod]
    [DataRow(RunClassification.None, 0)]
    [DataRow(RunClassification.Running, 0)]
    [DataRow(RunClassification.Unclassified, 0)]
    [DataRow(RunClassification.TestRun, 0)]
    [DataRow(RunClassification.Errored, 0)]
    [DataRow(RunClassification.Deleted, 0)]
    [DataRow(RunClassification.InitialRunCompleted, 1)]
    [DataRow(RunClassification.InitialRun, 1)]
    [DataRow(RunClassification.InterimRecalculationRun, 1)]
    [DataRow(RunClassification.FinalRun, 1)]
    [DataRow(RunClassification.FinalRecalculationRun, 1)]
    [DataRow(RunClassification.InterimRecalculationRunCompleted, 1)]
    [DataRow(RunClassification.FinalRecalculationRunCompleted, 1)]
    [DataRow(RunClassification.FinalRunCompleted, 1)]
    public async Task GetDesignatedRunsByFinanialYear_ReturnsRunsWithValidClassifications(
        RunClassification classification,
        int expectedRowCount)
    {
        // Arrange
        this.AddRunToDb(classification, requestId: 1, 2024);

        // Act
        var result = await this.service.GetDesignatedRunsByFinanialYear(new RelativeYear(2024), TestContext.CancellationTokenSource.Token);

        // Assert
        result.Should().HaveCount(expectedRowCount);
    }

    [TestMethod]
    public async Task GetDesignatedRunsByFinanialYear_ExcludesRunsInWrongRelativeYearOrClassification()
    {
        // Arrange
        this.AddRunToDb(RunClassification.InitialRun, requestId: 1, 1923);
        this.AddRunToDb(RunClassification.InitialRunCompleted, requestId: 2, 2024);
        this.AddRunToDb(RunClassification.InterimRecalculationRun, requestId: 3, 2024);
        this.AddRunToDb(RunClassification.TestRun, requestId: 4, 2024);

        // Act
        var result = await this.service.GetDesignatedRunsByFinanialYear(new RelativeYear(2024), TestContext.CancellationTokenSource.Token);

        // Assert
        result.Should().HaveCount(2);

        result.Should().SatisfyRespectively(
            first =>
            {
                first.RunId.Should().Be(2);
                first.RunClassification.Should().Be(RunClassification.InitialRunCompleted);
            },
            second =>
            {
                second.RunId.Should().Be(3);
                second.RunClassification.Should().Be(RunClassification.InterimRecalculationRun);
            });
    }

    private void AddRunToDb(RunClassification classification, int requestId, int relativeYearValue)
    {
        this.dbContext.CalculatorRuns.Add(new CalculatorRun
        {
            Id = requestId,
            Classification = classification,
            Name = "Test",
            RelativeYear = new RelativeYear(relativeYearValue),
            CreatedBy = "TestUser",
            CreatedAt = DateTime.UtcNow,
        });
        this.dbContext.SaveChanges();
    }
}
