using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Exporter.JsonExporter;
using EPR.Calculator.Service.Function.Features.BillingRun.Contexts;
using EPR.Calculator.Service.Function.Features.BillingRun.Outputs;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Options;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Billing.FileExports;

[TestCategory(TestCategories.BillingRuns)]
[TestClass]
public class BillingFileGeneratorTests : TestsFor<BillingFileGenerator>
{
    private Mock<IOptions<BlobStorageOptions>> blobOptions = null!;
    private CalcResult calcResult = null!;
    private Mock<IBillingFileExporter> csvWriter = null!;
    private Mock<IBillingFileJsonWriter> jsonWriter = null!;
    private BillingRunContext runContext = null!;
    private Mock<IStorageUploadService> storageUploadService = null!;

    protected override void TestInitialize()
    {
        blobOptions = fixture.Freeze<Mock<IOptions<BlobStorageOptions>>>();
        blobOptions.Setup(m => m.Value).Returns(new BlobStorageOptions
        {
            BillingFileCsvContainerName = "csv-container",
            BillingFileJsonContainerName = "json-container"
        });

        csvWriter = fixture.Freeze<Mock<IBillingFileExporter>>();
        csvWriter.Setup(m => m.Export(
                It.IsAny<BillingRunContext>(), It.IsAny<CalcResult>()))
            .ReturnsAsync("csv-content");

        jsonWriter = fixture.Freeze<Mock<IBillingFileJsonWriter>>();
        jsonWriter.Setup(m => m.WriteToString(
                It.IsAny<BillingRunContext>(), It.IsAny<CalcResult>()))
            .ReturnsAsync("json-content");

        storageUploadService = fixture.Freeze<Mock<IStorageUploadService>>();
        storageUploadService.Setup(m => m.UploadFileContentAsync(
                It.Is<(string, string, string, string, bool)>(a => a.Item4 == "csv-container"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://csv.uri");
        storageUploadService.Setup(m => m.UploadFileContentAsync(
                It.Is<(string, string, string, string, bool)>(a => a.Item4 == "json-container"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://json.uri");

        runContext = fixture.Create<BillingRunContext>();
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
                args.Content == "csv-content"
                && args.RunName == runContext.RunName
                && args.ContainerName == "csv-container"
                && args.Overwrite),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Should_upload_json_to_correct_container()
    {
        // Act
        await testSubject.SerializeAndExport(runContext, calcResult, CancellationToken.None);

        // Assert
        storageUploadService.Verify(x => x.UploadFileContentAsync(
            It.Is<(string FileName, string Content, string RunName, string ContainerName, bool Overwrite)>(args =>
                args.Content == "json-content"
                && args.RunName == runContext.RunName
                && args.ContainerName == "json-container"
                && args.Overwrite),
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
        result.CsvMetadata.BlobUri.ShouldBe("https://csv.uri");
        result.CsvMetadata.FileName.ShouldContain("Billing");
        result.CsvMetadata.FileName.ShouldEndWith(".csv");
    }

    [TestMethod]
    public async Task Should_return_json_metadata_with_correct_values()
    {
        // Act
        var result = await testSubject.SerializeAndExport(runContext, calcResult, CancellationToken.None);

        // Assert
        result.JsonMetadata.CalculatorRunId.ShouldBe(runContext.RunId);
        result.JsonMetadata.BillingFileCreatedBy.ShouldBe(runContext.User);
        result.JsonMetadata.BillingFileCreatedDate.ShouldBe(runContext.ProcessingStartedAt.UtcDateTime);
        result.JsonMetadata.BillingCsvFileName.ShouldBe(result.CsvMetadata.FileName);
        result.JsonMetadata.BillingJsonFileName.ShouldNotBeNull();
        result.JsonMetadata.BillingJsonFileName.ShouldContain("Billing");
        result.JsonMetadata.BillingJsonFileName.ShouldEndWith(".json");
    }
}
