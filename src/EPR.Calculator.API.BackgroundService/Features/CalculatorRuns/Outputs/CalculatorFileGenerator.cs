using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter;
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
    ///     Serializes the calcResult to a CSV result file and exports it.
    /// </summary>
    Task<CalculatorFileResult> SerializeAndExport(CalculatorRunContext runContext, CalcResult calcResult, CancellationToken cancellationToken);
}

public class CalculatorFileGenerator(
    IOptions<BlobStorageUploadOptions> blobStorageUploadOptions,
    ICalcResultsExporter csvWriter,
    IStorageUploadService storageUploadService,
    ILogger<CalculatorFileGenerator> logger)
    : ICalculatorFileGenerator
{
    public async Task<CalculatorFileResult> SerializeAndExport(CalculatorRunContext runContext, CalcResult calcResult, CancellationToken cancellationToken)
    {
        var csvMetaData = await HandleCsvFile(runContext, calcResult, cancellationToken);
        logger.LogInformation($"{nameof(HandleCsvFile)} Completed. File: {{Filename}}", csvMetaData.FileName);

        return new CalculatorFileResult
        {
            CsvMetadata = csvMetaData
        };
    }

    public async Task<CalculatorRunCsvFileMetadata> HandleCsvFile(CalculatorRunContext runContext, CalcResult calcResult, CancellationToken cancellationToken)
    {
        var csvContent = await csvWriter.Export(runContext, calcResult);

        var csvFilename = new CalcResultsAndBillingFileName(
            calcResult.CalcResultDetail.RunId,
            calcResult.CalcResultDetail.RunName,
            calcResult.CalcResultDetail.RunDate);

        var request = new IStorageUploadService.Request
        {
            FileName = csvFilename,
            Content = csvContent,
            ContainerName = blobStorageUploadOptions.Value.ResultFileCsvContainer
        };

        var csvBlobUri = await storageUploadService.UploadFileContentAsync(request, cancellationToken);

        return new CalculatorRunCsvFileMetadata
        {
            BlobUri = csvBlobUri,
            CalculatorRunId = runContext.RunId,
            FileName = csvFilename
        };
    }
}
