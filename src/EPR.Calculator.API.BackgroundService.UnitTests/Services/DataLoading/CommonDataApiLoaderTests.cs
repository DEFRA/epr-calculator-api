using EPR.Calculator.API.BackgroundService.Options;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.Services.DataLoading;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.TestData;
using EPR.Calculator.API.Data.DataModels;
using EPR.CommonDataService.DataApi.CommonDataApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Services.DataLoading;

/// <summary>
///     Unit tests for <see cref="CommonDataApiLoader" />.
///     <para>
///         All streaming/business-rule logic now lives in DataApi's <c>IProducerDataService</c> (a single
///         call). This loader's own job is just: the disabled guard, and gathering the one small
///         BackgroundService-owned input (material codes) that DataApi needs but doesn't own.
///     </para>
/// </summary>
[TestClass]
public class CommonDataApiLoaderTests
{
    private Mock<IProducerDataService> mockProducerDataService = null!;
    private Mock<IMaterialService> mockMaterialService = null!;
    private Mock<ILogger<CommonDataApiLoader>> mockLogger = null!;

    [TestInitialize]
    public void SetUp()
    {
        mockProducerDataService = new Mock<IProducerDataService>();
        mockMaterialService = new Mock<IMaterialService>();
        mockLogger = new Mock<ILogger<CommonDataApiLoader>>();

        mockMaterialService
            .Setup(m => m.GetMaterials())
            .ReturnsAsync(ImmutableList<MaterialDetail>.Empty);
    }

    [TestMethod]
    public async Task LoadData_WhenDisabled_DoesNotCallDataApi()
    {
        // Arrange
        var loader = CreateLoader(enabled: false);

        // Act
        var result = await loader.LoadData(TestDataHelper.CalculatorRun2024);

        // Assert
        result.Organisations.ShouldBeEmpty();
        result.Producers.ShouldBeEmpty();
        result.Errors.ShouldBeEmpty();
        mockProducerDataService.Verify(
            s => s.GetProducerData(It.IsAny<int>(), It.IsAny<DateTimeOffset?>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task LoadData_WhenEnabled_PassesMaterialCodesToDataApi()
    {
        // Arrange
        mockMaterialService
            .Setup(m => m.GetMaterials())
            .ReturnsAsync(ImmutableList.Create(
                new MaterialDetail { Id = 1, Code = "PL", Name = "Plastic" },
                new MaterialDetail { Id = 2, Code = "GL", Name = "Glass" }));

        var expected = new ProducerCalculationData { Organisations = [], Producers = [], Errors = [] };
        mockProducerDataService
            .Setup(s => s.GetProducerData(
                It.IsAny<int>(),
                It.IsAny<DateTimeOffset?>(),
                It.Is<IReadOnlyList<string>>(codes => codes.SequenceEqual(new[] { "PL", "GL" })),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var loader = CreateLoader(enabled: true);

        // Act
        var result = await loader.LoadData(TestDataHelper.CalculatorRun2024);

        // Assert
        result.ShouldBe(expected);
        mockProducerDataService.VerifyAll();
    }

    [TestMethod]
    public async Task LoadData_WhenDataApiThrows_Propagates()
    {
        // Arrange
        mockProducerDataService
            .Setup(s => s.GetProducerData(It.IsAny<int>(), It.IsAny<DateTimeOffset?>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("data api failed"));

        var loader = CreateLoader(enabled: true);

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(async () => await loader.LoadData(TestDataHelper.CalculatorRun2024));
    }

    private CommonDataApiLoader CreateLoader(bool enabled)
    {
        var loaderOptions = new OptionsWrapper<CommonDataApiLoaderOptions>(new CommonDataApiLoaderOptions { Enabled = enabled });

        return new CommonDataApiLoader(
            loaderOptions,
            mockProducerDataService.Object,
            mockMaterialService.Object,
            mockLogger.Object);
    }
}
