using System.Text;
using System.Text.Json;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Messaging;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Services.DataLoading;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Services;

[TestClass]
public class CalculatorRunServiceTests
{
    private IFixture fixture = null!;
    private Mock<IDataLoader> dataLoader = null!;
    private Mock<IPrepareCalcService> prepareCalcService = null!;
    private Mock<IRpdStatusService> statusService = null!;
    private Mock<IProducerDataTransposer> transposeService = null!;
    private CalculatorRunService sut = null!;

    [TestInitialize]
    public void Init()
    {
        fixture = TestFixtures.New();
        dataLoader = fixture.Freeze<Mock<IDataLoader>>();
        transposeService = fixture.Freeze<Mock<IProducerDataTransposer>>();

        prepareCalcService = fixture.Freeze<Mock<IPrepareCalcService>>();
        prepareCalcService.Setup(s => s.PrepareCalcResultsAsync(
                It.IsAny<CalcResultsRequestDto>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(PreparedResult.Success("some-results.csv"));

        statusService = fixture.Freeze<Mock<IRpdStatusService>>();
        statusService.Setup(s => s.UpdateRpdStatus(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(RunClassification.RUNNING);

        sut = fixture.Create<CalculatorRunService>();
    }

    [TestMethod]
    public async Task StartProcessReturnsFalseWhenCalculatorTimesOut()
    {
        // Arrange
        var message = fixture.Create<CreateResultFileMessage>();
        var runName = "Test Run Name";

        statusService.Setup(s => s.UpdateRpdStatus(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Timed out!"));

        // Act
        var result = await sut.PrepareResultsFileAsync(message, runName);

        // Assert
        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task StartProcess_ShouldReturnFalseOn_TaskCanceledException()
    {
        // Arrange
        var message = new CreateResultFileMessage
            { CalculatorRunId = 1, CreatedBy = "TestUser", RelativeYear = new RelativeYear(2024), MessageType = "ignored" };
        var runName = "TestRun";

        transposeService.Setup(t => t.Transpose(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException());

        // Act
        var result = await sut.PrepareResultsFileAsync(message, runName);

        // Assert
        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task StartProcess_ShouldReturnFalseOn_Exception()
    {
        // Arrange
        var message = new CreateResultFileMessage
            { CalculatorRunId = 1, CreatedBy = "TestUser", RelativeYear = new RelativeYear(2024), MessageType = "ignored" };
        var runName = "TestRun";

        transposeService.Setup(t => t.Transpose(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test Exception"));

        // Act
        var result = await sut.PrepareResultsFileAsync(message, runName);

        // Assert
        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task PrepareResultsFileAsync_WhenAllStepsSucceed_ReturnsTrue()
    {
        // Arrange
        var message = new CreateResultFileMessage
        {
            CalculatorRunId = 1, CreatedBy = "TestUser", RelativeYear = new RelativeYear(2024), MessageType = "ignored"
        };

        // Act
        var result = await sut.PrepareResultsFileAsync(message, "TestRun");

        // Assert
        Assert.IsTrue(result.IsSuccess);
        dataLoader.Verify(d => d.LoadData(message.RelativeYear, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task PrepareResultsFileAsync_WhenStatusNotRunning_ReturnsFalse()
    {
        // Arrange
        statusService.Setup(s => s.UpdateRpdStatus(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RunClassification.UNCLASSIFIED);

        var message = new CreateResultFileMessage
        {
            CalculatorRunId = 1, CreatedBy = "TestUser", RelativeYear = new RelativeYear(2024), MessageType = "ignored"
        };

        // Act
        var result = await sut.PrepareResultsFileAsync(message, "TestRun");

        // Assert
        Assert.IsFalse(result.IsSuccess);
        transposeService.Verify(
            t => t.Transpose(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task PrepareResultsFileAsync_WhenPrepareCalcResultsFails_ReturnsFalse()
    {
        // Arrange
        prepareCalcService.Setup(s => s.PrepareCalcResultsAsync(
                It.IsAny<CalcResultsRequestDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PreparedResult.Failure<string>());

        var message = new CreateResultFileMessage
        {
            CalculatorRunId = 1, CreatedBy = "TestUser", RelativeYear = new RelativeYear(2024), MessageType = "ignored"
        };

        // Act
        var result = await sut.PrepareResultsFileAsync(message, "TestRun");

        // Assert
        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task PrepareResultsFileAsync_WhenDataLoaderThrows_ReturnsFalse()
    {
        // Arrange
        dataLoader.Setup(d => d.LoadData(
                It.IsAny<RelativeYear>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API failure"));

        var message = new CreateResultFileMessage
        {
            CalculatorRunId = 1, CreatedBy = "TestUser", RelativeYear = new RelativeYear(2024), MessageType = "ignored"
        };

        // Act
        var result = await sut.PrepareResultsFileAsync(message, "TestRun");

        // Assert
        Assert.IsFalse(result.IsSuccess);
        statusService.Verify(
            s => s.UpdateRpdStatus(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task PrepareResultsFileAsync_WhenDataLoaderCancelled_ReturnsFalse()
    {
        // Arrange
        dataLoader.Setup(d => d.LoadData(
                It.IsAny<RelativeYear>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Cancelled"));

        var message = new CreateResultFileMessage
        {
            CalculatorRunId = 1, CreatedBy = "TestUser", RelativeYear = new RelativeYear(2024), MessageType = "ignored"
        };

        // Act
        var result = await sut.PrepareResultsFileAsync(message, "TestRun");

        // Assert
        Assert.IsFalse(result.IsSuccess);
    }
}
