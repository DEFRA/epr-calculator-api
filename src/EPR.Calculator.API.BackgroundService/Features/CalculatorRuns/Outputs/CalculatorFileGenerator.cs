using EPR.Calculator.API.BackgroundService.Exporter;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Options;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Outputs;

public interface ICalculatorFileGenerator
{
    /// <summary>
    ///     Serializes the calcResult to a CSV result file and exports it to blob storage.
    /// </summary>
    Task<CalculatorFileResult> SerializeAndExport(CalculatorRunContext runContext, CalcResult calcResult, IReadOnlyList<FeeDetail> producerFeeDetails, CancellationToken cancellationToken);
}

public class CalculatorFileGenerator(
    IOptions<BlobStorageUploadOptions> blobStorageUploadOptions,
    IRunFileExporter runFileExporter,
    IStorageUploadService storageUploadService,
    ILogger<CalculatorFileGenerator> logger)
    : ICalculatorFileGenerator
{
    [ActivityTrace]
    public async Task<CalculatorFileResult> SerializeAndExport(CalculatorRunContext runContext, CalcResult calcResult, IReadOnlyList<FeeDetail> producerFeeDetails, CancellationToken cancellationToken)
    {
        var csvMetaData = await HandleCsvFile(runContext, calcResult, producerFeeDetails, cancellationToken);
        logger.LogInformation($"{nameof(HandleCsvFile)} Completed. File: {{Filename}}", csvMetaData.FileName);

        return new CalculatorFileResult
        {
            CsvMetadata = csvMetaData
        };
    }

    private async Task<CalculatorRunCsvFileMetadata> HandleCsvFile(CalculatorRunContext runContext, CalcResult calcResult, IReadOnlyList<FeeDetail> producerFeeDetails, CancellationToken cancellationToken)
    {
        var file = runFileExporter.GetResultsCsv(runContext, calcResult, producerFeeDetails);

        var request = new IStorageUploadService.StreamRequest
        {
            FileName = file.FileName,
            ContainerName = blobStorageUploadOptions.Value.ResultFileCsvContainer,
            UseUtf8Bom = false // the file writes its own
        };

        var csvBlobUri = await storageUploadService.UploadFileStreamAsync(request, file.WriteTo, cancellationToken);

        return new CalculatorRunCsvFileMetadata
        {
            BlobUri = csvBlobUri,
            CalculatorRunId = runContext.RunId,
            FileName = file.FileName
        };
    }
}
