using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Services;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.UnitTests.Services;

[TestClass]
public class CalculationRunServiceTests
{
    private ApplicationDBContext dbContext = null!;
    private CalculationRunService service = null!;

    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        this.dbContext = new ApplicationDBContext(options);

        // Add all possible classifications
        foreach (RunClassification value in Enum.GetValues(typeof(RunClassification)))
        {
            this.dbContext.CalculatorRunClassifications.Add(new CalculatorRunClassification
            {
                Id = value,
                Status = value.ToString()
            });
        }

        this.dbContext.CalculatorRunRelativeYears.Add(new CalculatorRunRelativeYear
        {
            Value = new RelativeYear(2024)
        });

        this.dbContext.SaveChanges();

        this.service = new CalculationRunService(this.dbContext);
    }

    [TestCleanup]
    public void Cleanup()
    {
        this.dbContext.Dispose();
    }

    [TestMethod]
    [DataRow(RunClassification.Running)]
    [DataRow(RunClassification.Unclassified)]
    [DataRow(RunClassification.Test)]
    [DataRow(RunClassification.Errored)]
    [DataRow(RunClassification.Deleted)]
    [DataRow(RunClassification.InitialCompleted)]
    [DataRow(RunClassification.Initial)]
    [DataRow(RunClassification.Recalculation)]
    [DataRow(RunClassification.RecalculationCompleted)]
    public async Task GetDesignatedRunsByFinanialYear_ExcludesRunsInWrongRelativeYear(RunClassification classification)
    {
        // Arrange
        this.AddRunToDb(classification, requestId: 1, 1923);

        // Act
        var result = await this.service.GetDesignatedRunsByFinancialYear(new RelativeYear(2024), TestContext.CancellationTokenSource.Token);

        // Assert
        result.Count.ShouldBe(0);
    }

    [TestMethod]
    [DataRow(RunClassification.Running, 0)]
    [DataRow(RunClassification.Unclassified, 0)]
    [DataRow(RunClassification.Test, 0)]
    [DataRow(RunClassification.Errored, 0)]
    [DataRow(RunClassification.Deleted, 0)]
    [DataRow(RunClassification.InitialCompleted, 1)]
    [DataRow(RunClassification.Initial, 1)]
    [DataRow(RunClassification.Recalculation, 1)]
    [DataRow(RunClassification.RecalculationCompleted, 1)]
    public async Task GetDesignatedRunsByFinanialYear_ReturnsRunsWithValidClassifications(
        RunClassification classification,
        int expectedRowCount)
    {
        // Arrange
        this.AddRunToDb(classification, requestId: 1, 2024);

        // Act
        var result = await this.service.GetDesignatedRunsByFinancialYear(new RelativeYear(2024), TestContext.CancellationTokenSource.Token);

        // Assert
        result.Count.ShouldBe(expectedRowCount);
    }

    [TestMethod]
    public async Task GetDesignatedRunsByFinanialYear_ExcludesRunsInWrongRelativeYearOrClassification()
    {
        // Arrange
        this.AddRunToDb(RunClassification.Initial, requestId: 1, 1923);
        this.AddRunToDb(RunClassification.InitialCompleted, requestId: 2, 2024);
        this.AddRunToDb(RunClassification.Recalculation, requestId: 3, 2024);
        this.AddRunToDb(RunClassification.Test, requestId: 4, 2024);

        // Act
        var result = await this.service.GetDesignatedRunsByFinancialYear(new RelativeYear(2024), TestContext.CancellationTokenSource.Token);

        // Assert
        result.Count.ShouldBe(2);
        result[0].RunId.ShouldBe(2);
        result[0].RunClassification.ShouldBe(RunClassification.InitialCompleted);
        result[1].RunId.ShouldBe(3);
        result[1].RunClassification.ShouldBe(RunClassification.Recalculation);
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
