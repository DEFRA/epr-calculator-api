namespace EPR.Calculator.API.UnitTests.Services;

using EnumsNET;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public class CalculationRunServiceTests
{
    private const string FinancialYear = "2024-25";
    private const string WrongFinancialYear = "1923-24";

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

        // Add all possible classifications
        foreach (RunClassification value in Enum.GetValues(typeof(RunClassification)))
        {
            this.dbContext.CalculatorRunClassifications.Add(new CalculatorRunClassification
            {
                Id = (int)value,
                Status = value.AsString(EnumFormat.Description)!, // not null by contract
            });
        }

        this.dbContext.FinancialYears.Add(new CalculatorRunFinancialYear
        {
            Name = FinancialYear,
        });

        this.dbContext.FinancialYears.Add(new CalculatorRunFinancialYear
        {
            Name = WrongFinancialYear,
        });

        this.dbContext.SaveChanges();

        this.loggerMock = new Mock<ILogger<CalculationRunService>>();
        this.service = new CalculationRunService(this.dbContext, this.loggerMock.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        this.dbContext.Dispose();
    }

    [TestMethod]
    [DataRow(RunClassification.INTHEQUEUE)]
    [DataRow(RunClassification.RUNNING)]
    [DataRow(RunClassification.UNCLASSIFIED)]
    [DataRow(RunClassification.TEST_RUN)]
    [DataRow(RunClassification.ERROR)]
    [DataRow(RunClassification.DELETED)]
    [DataRow(RunClassification.INITIAL_RUN_COMPLETED)]
    [DataRow(RunClassification.INITIAL_RUN)]
    [DataRow(RunClassification.INTERIM_RECALCULATION_RUN)]
    [DataRow(RunClassification.FINAL_RUN)]
    [DataRow(RunClassification.FINAL_RECALCULATION_RUN)]
    [DataRow(RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED)]
    [DataRow(RunClassification.FINAL_RECALCULATION_RUN_COMPLETED)]
    [DataRow(RunClassification.FINAL_RUN_COMPLETED)]
    public async Task GetDesignatedRunsByFinanialYear_ExcludesRunsInWrongFinancialYear(RunClassification classification)
    {
        // Arrange
        this.AddRunToDb(classification, requestId: 1, WrongFinancialYear);

        // Act
        var result = await this.service.GetDesignatedRunsByFinanialYear(FinancialYear, TestContext.CancellationTokenSource.Token);

        // Assert
        result.Should().HaveCount(0);
    }

    [TestMethod]
    [DataRow(RunClassification.INTHEQUEUE, 0)]
    [DataRow(RunClassification.RUNNING, 0)]
    [DataRow(RunClassification.UNCLASSIFIED, 0)]
    [DataRow(RunClassification.TEST_RUN, 0)]
    [DataRow(RunClassification.ERROR, 0)]
    [DataRow(RunClassification.DELETED, 0)]
    [DataRow(RunClassification.INITIAL_RUN_COMPLETED, 1)]
    [DataRow(RunClassification.INITIAL_RUN, 1)]
    [DataRow(RunClassification.INTERIM_RECALCULATION_RUN, 1)]
    [DataRow(RunClassification.FINAL_RUN, 1)]
    [DataRow(RunClassification.FINAL_RECALCULATION_RUN, 1)]
    [DataRow(RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED, 1)]
    [DataRow(RunClassification.FINAL_RECALCULATION_RUN_COMPLETED, 1)]
    [DataRow(RunClassification.FINAL_RUN_COMPLETED, 1)]
    public async Task GetDesignatedRunsByFinanialYear_ReturnsRunsWithValidClassifications(
        RunClassification classification,
        int expectedRowCount)
    {
        // Arrange
        this.AddRunToDb(classification, requestId: 1, FinancialYear);

        // Act
        var result = await this.service.GetDesignatedRunsByFinanialYear(FinancialYear, TestContext.CancellationTokenSource.Token);

        // Assert
        result.Should().HaveCount(expectedRowCount);
    }

    [TestMethod]
    public async Task GetDesignatedRunsByFinanialYear_ExcludesRunsInWrongFinancialYearOrClassification()
    {
        // Arrange
        this.AddRunToDb(RunClassification.INITIAL_RUN, requestId: 1, WrongFinancialYear);
        this.AddRunToDb(RunClassification.INITIAL_RUN_COMPLETED, requestId: 2, FinancialYear);
        this.AddRunToDb(RunClassification.INTERIM_RECALCULATION_RUN, requestId: 3, FinancialYear);
        this.AddRunToDb(RunClassification.TEST_RUN, requestId: 4, FinancialYear);

        // Act
        var result = await this.service.GetDesignatedRunsByFinanialYear(FinancialYear, TestContext.CancellationTokenSource.Token);

        // Assert
        result.Should().HaveCount(2);

        result.Should().SatisfyRespectively(
            first =>
            {
                first.RunId.Should().Be(2);
                first.RunClassificationId.Should().Be((int)RunClassification.INITIAL_RUN_COMPLETED);
            },
            second =>
            {
                second.RunId.Should().Be(3);
                second.RunClassificationId.Should().Be((int)RunClassification.INTERIM_RECALCULATION_RUN);
            });
    }

    private void AddRunToDb(RunClassification classification, int requestId, string financialYearId)
    {
        this.dbContext.CalculatorRuns.Add(new CalculatorRun
        {
            Id = requestId,
            CalculatorRunClassificationId = (int)classification,
            Name = "Test",
            FinancialYearId = financialYearId,
            Financial_Year = this.dbContext.FinancialYears.First(x => x.Name.Equals(financialYearId)),
            CreatedBy = "TestUser",
            CreatedAt = DateTime.UtcNow,
        });
        this.dbContext.SaveChanges();
    }
}