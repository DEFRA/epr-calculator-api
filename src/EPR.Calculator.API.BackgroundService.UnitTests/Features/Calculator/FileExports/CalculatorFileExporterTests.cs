using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Outputs;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Options;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Features.Calculator.FileExports;

[TestCategory(TestCategories.CalculatorRuns)]
[TestClass]
public class CalculatorFileGeneratorTests : TestsFor<CalculatorFileGenerator>
{
    private Mock<IOptions<BlobStorageUploadOptions>> blobUploadOptions = null!;
    private CalcResult calcResult = null!;
    private Mock<ICalcResultsExporter> csvWriter = null!;
    private CalculatorRunContext runContext = null!;
    private Mock<IStorageUploadService> storageUploadService = null!;

    protected override void TestInitialize()
    {
        blobUploadOptions = fixture.Freeze<Mock<IOptions<BlobStorageUploadOptions>>>();
        blobUploadOptions.Setup(m => m.Value).Returns(new BlobStorageUploadOptions
        {
            ResultFileCsvContainer = "results-container"
        });

        csvWriter = fixture.Freeze<Mock<ICalcResultsExporter>>();
        csvWriter
            .Setup(m => m.Export(
                It.IsAny<CalculatorRunContext>(), It.IsAny<CalcResult>()))
            .ReturnsAsync("results-content");

        storageUploadService = fixture.Freeze<Mock<IStorageUploadService>>();
        storageUploadService.Setup(m => m.UploadFileContentAsync(
                It.Is<(string, string, string, string, bool)>(a => a.Item4 == "results-container"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://results.uri");

        runContext = fixture.Create<CalculatorRunContext>();
        calcResult = fixture.Create<CalcResult>();
    }

    [TestMethod]
    public async Task Should_upload_csv_to_correct_container()
    {
        // Act
        await testSubject.SerializeAndExport(runContext, calcResult, CancellationToken.None);

        // Assert
        storageUploadService.Verify(x => x.UploadFileContentAsync(
            It.Is<(string FileName, string Content, string RunName, string ContainerName, bool Overwrite)>(args =>
                args.Content == "results-content"
                && args.RunName == runContext.RunName
                && args.ContainerName == "results-container"
                && !args.Overwrite),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Should_return_csv_metadata_with_correct_values()
    {
        // Act
        var result = await testSubject.SerializeAndExport(runContext, calcResult, CancellationToken.None);

        // Assert
        result.CsvMetadata.CalculatorRunId.ShouldBe(runContext.RunId);
        result.CsvMetadata.FileName.ShouldNotBeNull();
        result.CsvMetadata.BlobUri.ShouldBe("https://results.uri");
        result.CsvMetadata.FileName.ShouldContain("Results File");
        result.CsvMetadata.FileName.ShouldEndWith(".csv");
    }
}
