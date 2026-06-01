using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Services;

[TestClass]
public class PrepareBillingFileServiceTests
{
    private ApplicationDBContext dbContext = null!;
    private Mock<IPrepareCalcService> prepareCalcService = null!;
    private PrepareBillingFileService sut = null!;

    [TestInitialize]
    public void Init()
    {
        var fixture = TestFixtures.New();
        dbContext = fixture.Freeze<ApplicationDBContext>();

        prepareCalcService = fixture.Freeze<Mock<IPrepareCalcService>>();
        prepareCalcService
            .Setup(s => s.PrepareCalcResultsAsync(
                It.IsAny<CalcResultsRequestDto>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(PreparedResult.Success("some-results.csv"));

        prepareCalcService
            .Setup(s => s.PrepareBillingResultsAsync(
                It.IsAny<CalcResultsRequestDto>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(PreparedResult.Success((CsvFileName: "some-billing.csv", JsonFileName: "some-billing.json")));

        sut = fixture.Create<PrepareBillingFileService>();
    }

    [TestMethod]
    public async Task PrepareBillingFileAsync_ReturnsFalse_WhenRunNotFound()
    {
        // Arrange
        var calculatorRunId = 99854;
        var calculatorName = "Test";
        var approvedBy = "user";

        // Act
        var result = await sut.PrepareBillingFileAsync(calculatorRunId, calculatorName, approvedBy);

        // Assert
        Assert.AreEqual(false, result.IsSuccess);
    }

    [TestMethod]
    public async Task PrepareBillingFileAsync_ReturnsFalse_WhenNoBillingInstructions()
    {
        // Arrange
        var calculatorRunId = 99854;
        var calculatorName = "Test";
        dbContext.CalculatorRuns.Add(
            new CalculatorRun
            {
                Id = calculatorRunId,
                CalculatorRunClassificationId = 8,
                Name = calculatorName,
                RelativeYear = new RelativeYear(2025),
                CreatedBy = "user",
                CreatedAt = DateTime.UtcNow
            });
        await dbContext.SaveChangesAsync();
        var approvedBy = "user";

        // Act
        var result = await sut.PrepareBillingFileAsync(calculatorRunId, calculatorName, approvedBy);

        // Assert
        Assert.AreEqual(false, result.IsSuccess);
    }

    [TestMethod]
    public async Task PrepareBillingFileAsync_ReturnsTrue_WhenRunAndAcceptedBillingInstructionsExist()
    {
        // Arrange
        var calculatorRunId = 122444;
        var calculatorName = "TestRun";
        var acceptedProducerId = 999;

        // Add a CalculatorRun to the context
        dbContext.CalculatorRuns.Add(new CalculatorRun
        {
            Id = calculatorRunId,
            CalculatorRunClassificationId = 1,
            Name = calculatorName,
            RelativeYear = new RelativeYear(2025),
            CreatedBy = "user",
            CreatedAt = DateTime.UtcNow,
            IsBillingFileGenerating = true
        });

        // Add an accepted billing instruction
        dbContext.Add(new ProducerResultFileSuggestedBillingInstruction
        {
            CalculatorRunId = calculatorRunId,
            ProducerId = acceptedProducerId,
            BillingInstructionAcceptReject = PrepareBillingFileConstants.BillingInstructionAccepted,
            SuggestedBillingInstruction = "TestInstruction"
        });

        await dbContext.SaveChangesAsync();

        var approvedBy = "user";

        // Act
        var result = await sut.PrepareBillingFileAsync(calculatorRunId, calculatorName, approvedBy);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        prepareCalcService.Verify(s => s.PrepareBillingResultsAsync(
            It.Is<CalcResultsRequestDto>(dto =>
                dto.RunId == calculatorRunId &&
                dto.AcceptedProducerIds.Contains(acceptedProducerId) &&
                dto.IsBillingFile),
            calculatorName,
            CancellationToken.None), Times.Once);
    }

    [TestMethod]
    public async Task PrepareBillingFileAsync_ReturnsTrue_WhenRunAndNoAcceptedBillingInstructionsExist()
    {
        // Arrange
        var calculatorRunId = 122445;
        var calculatorName = "TestRun";
        var acceptedProducerId = 999;

        // Add a CalculatorRun to the context
        dbContext.CalculatorRuns.Add(new CalculatorRun
        {
            Id = calculatorRunId,
            CalculatorRunClassificationId = 1,
            Name = calculatorName,
            RelativeYear = new RelativeYear(2025),
            CreatedBy = "user",
            CreatedAt = DateTime.UtcNow,
            IsBillingFileGenerating = true
        });

        // Add an accepted billing instruction
        dbContext.Add(new ProducerResultFileSuggestedBillingInstruction
        {
            CalculatorRunId = calculatorRunId,
            ProducerId = acceptedProducerId,
            BillingInstructionAcceptReject = "Rejected",
            SuggestedBillingInstruction = "TestInstruction"
        });

        await dbContext.SaveChangesAsync();

        var approvedBy = "user";

        // Act
        var result = await sut.PrepareBillingFileAsync(calculatorRunId, calculatorName, approvedBy);

        // Assert
        Assert.AreEqual(true, result.IsSuccess);
    }
}
