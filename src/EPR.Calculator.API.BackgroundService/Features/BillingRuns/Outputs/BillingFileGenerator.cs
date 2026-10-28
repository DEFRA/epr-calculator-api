using EPR.Calculator.API.BackgroundService.Exporter;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Options;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.API.BackgroundService.Features.BillingRuns.Outputs;

public interface IBillingFileGenerator
{
    /// <summary>
    ///     Serializes the billing result to CSV/JSON billing files and exports them to blob storage.
    /// </summary>
    Task<BillingFileResult> SerializeAndExport(BillingRunContext runContext, CalcResult billingResult, IReadOnlyList<FeeDetail> acceptedFeeDetails, CancellationToken cancellationToken);
}

public class BillingFileGenerator(
    IOptions<BlobStorageUploadOptions> blobStorageUploadOptions,
    IRunFileExporter runFileExporter,
    IStorageUploadService storageUploadService,
    ILogger<BillingFileGenerator> logger)
    : IBillingFileGenerator
{
    [ActivityTrace]
    public async Task<BillingFileResult> SerializeAndExport(BillingRunContext runContext, CalcResult billingResult, IReadOnlyList<FeeDetail> acceptedFeeDetails, CancellationToken cancellationToken)
    {
        var csvMetaData = await HandleCsvFile(runContext, billingResult, acceptedFeeDetails, cancellationToken);
        logger.LogInformation($"{nameof(HandleCsvFile)} Completed. File: {{Filename}}", csvMetaData.FileName);

        var jsonMetaData = await HandleJsonFile(runContext, billingResult, acceptedFeeDetails, csvMetaData, cancellationToken);
        logger.LogInformation($"{nameof(HandleJsonFile)} Completed. File: {{Filename}}", jsonMetaData.BillingJsonFileName);

        return new BillingFileResult
        {
            CsvMetadata = csvMetaData,
            JsonMetadata = jsonMetaData
        };
    }

    private async Task<CalculatorRunCsvFileMetadata> HandleCsvFile(BillingRunContext runContext, CalcResult billingResult, IReadOnlyList<FeeDetail> acceptedFeeDetails, CancellationToken ct)
    {
        var file = runFileExporter.GetBillingCsv(runContext, billingResult, acceptedFeeDetails);

        var request = new IStorageUploadService.StreamRequest
        {
            FileName = file.FileName,
            ContainerName = blobStorageUploadOptions.Value.BillingFileCsvContainer,
            Overwrite = true,
            UseUtf8Bom = false // the file writes its own
        };

        var csvBlobUri = await storageUploadService.UploadFileStreamAsync(request, file.WriteTo, ct);

        return new CalculatorRunCsvFileMetadata
        {
            BlobUri = csvBlobUri,
            CalculatorRunId = runContext.RunId,
            FileName = file.FileName
        };
    }

    private async Task<CalculatorRunBillingFileMetadata> HandleJsonFile(BillingRunContext runContext, CalcResult billingResult, IReadOnlyList<FeeDetail> acceptedFeeDetails, CalculatorRunCsvFileMetadata csvMetaData, CancellationToken ct)
    {
        var file = runFileExporter.GetBillingJson(runContext, billingResult, acceptedFeeDetails);

        var request = new IStorageUploadService.StreamRequest
        {
            FileName = file.FileName,
            ContainerName = blobStorageUploadOptions.Value.BillingFileJsonContainer,
            Overwrite = true,
            UseUtf8Bom = false
        };

        await storageUploadService.UploadFileStreamAsync(request, file.WriteTo, ct);

        return new CalculatorRunBillingFileMetadata
        {
            CalculatorRunId = runContext.RunId,
            BillingCsvFileName = csvMetaData.FileName,
            BillingFileCreatedBy = runContext.User,
            BillingFileCreatedDate = runContext.ProcessingStartedAt.UtcDateTime,
            BillingJsonFileName = file.FileName
        };
    }
}
