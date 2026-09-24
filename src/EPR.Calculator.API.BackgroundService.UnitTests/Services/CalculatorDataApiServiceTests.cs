using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.TestData;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Api.DataApi.Models;
using EPR.Calculator.Api.DataApi.Services;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Services;

/// <summary>
///     Unit tests for <see cref="CalculatorDataApiService" />.
/// </summary>
[TestClass]
public class CalculatorDataApiServiceTests
{
    private Mock<IProducerDataService> mockProducerDataService = null!;
    private Mock<IMaterialService> mockMaterialService = null!;
    private Mock<ILogger<CalculatorDataApiService>> mockLogger = null!;

    [TestInitialize]
    public void SetUp()
    {
        mockProducerDataService = new Mock<IProducerDataService>();
        mockMaterialService = new Mock<IMaterialService>();
        mockLogger = new Mock<ILogger<CalculatorDataApiService>>();

        mockMaterialService
            .Setup(m => m.GetMaterials())
            .ReturnsAsync(ImmutableList<MaterialDetail>.Empty);
    }

    [TestMethod]
    public async Task GetProducerRecords_PassesMaterialCodesAndYearToDataApi()
    {
        // Arrange
        mockMaterialService
            .Setup(m => m.GetMaterials())
            .ReturnsAsync(ImmutableList.Create(
                new MaterialDetail { Id = 1, Code = "PL", Name = "Plastic" },
                new MaterialDetail { Id = 2, Code = "GL", Name = "Glass" }));

        IReadOnlyList<ProducerRecord> expected = [new ProducerRecord
        {
            OrganisationId = 1,
            ProducerName = "Org Co",
            Errors = [],
            Warnings = [],
            ReportedMaterials = []
        }];
        mockProducerDataService
            .Setup(s => s.GetProducerData(
                2024,
                It.IsAny<DateTimeOffset?>(),
                It.Is<IReadOnlyList<string>>(codes => codes.SequenceEqual(new[] { "PL", "GL" })),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var service = CreateService();

        // Act
        var result = await service.GetProducerRecords(TestDataHelper.CalculatorRun2024);

        // Assert
        result.ShouldBe(expected);
        mockProducerDataService.VerifyAll();
    }

    [TestMethod]
    public async Task GetProducerRecords_WithNoCutOffDate_PassesNullThrough()
    {
        // Arrange
        mockProducerDataService
            .Setup(s => s.GetProducerData(It.IsAny<int>(), null, It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<ProducerRecord>)[]);

        var service = CreateService();

        // Act
        await service.GetProducerRecords(TestDataHelper.CalculatorRun2024);

        // Assert
        mockProducerDataService.VerifyAll();
    }

    [TestMethod]
    public async Task GetProducerRecords_WhenDataApiThrows_Propagates()
    {
        // Arrange
        mockProducerDataService
            .Setup(s => s.GetProducerData(It.IsAny<int>(), It.IsAny<DateTimeOffset?>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("data api failed"));

        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(async () => await service.GetProducerRecords(TestDataHelper.CalculatorRun2024));
    }

    private CalculatorDataApiService CreateService() =>
        new(mockProducerDataService.Object, mockMaterialService.Object, mockLogger.Object);
}
