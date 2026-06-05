using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Features.CalculatorRun.Contexts;
using EPR.Calculator.Service.Function.Features.CalculatorRun.Outputs;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Options;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Calculator.FileExports;

[TestCategory(TestCategories.CalculatorRuns)]
[TestClass]
public class CalculatorFileGeneratorTests : TestsFor<CalculatorFileGenerator>
{
    private Mock<IOptions<BlobStorageOptions>> blobOptions = null!;
    private CalcResult calcResult = null!;
    private Mock<ICalcResultsExporter> csvWriter = null!;
    private CalculatorRunContext runContext = null!;
    private Mock<IStorageService> storageService = null!;

    protected override void TestInitialize()
    {
        blobOptions = fixture.Freeze<Mock<IOptions<BlobStorageOptions>>>();
        blobOptions.Setup(m => m.Value).Returns(new BlobStorageOptions
        {
            ResultFileCsvContainer = "results-container"
        });

        csvWriter = fixture.Freeze<Mock<ICalcResultsExporter>>();
        csvWriter
            .Setup(m => m.Export(
                It.IsAny<CalculatorRunContext>(), It.IsAny<CalcResult>()))
            .ReturnsAsync("results-content");

        storageService = fixture.Freeze<Mock<IStorageService>>();
        storageService.Setup(m => m.UploadFileContentAsync(
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
        storageService.Verify(x => x.UploadFileContentAsync(
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
