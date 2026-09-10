using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Utils;
using EPR.CommonDataService.DataApi.Alignment;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.BackgroundService.Services;

public interface IProducerDataTransposer
{
    /// <summary>
    ///     Persists a calculator run's organisations, producer details/reported materials, and any
    ///     errors/warnings raised while calculating them.
    /// </summary>
    Task Transpose(
        CalculatorRunContext runContext,
        IReadOnlyList<ProducerRecord> data,
        CancellationToken cancellationToken);
}

public class ProducerDataTransposer(
    ApplicationDBContext dbContext,
    IBulkOperations bulkOps,
    IErrorReportService errorReportService,
    TimeProvider timeProvider,
    ILogger<ProducerDataTransposer> logger
) : IProducerDataTransposer
{
    [ActivityTrace]
    public async Task Transpose(
        CalculatorRunContext runContext,
        IReadOnlyList<ProducerRecord> data,
        CancellationToken cancellationToken)
    {
        var calculatorRun = await dbContext.CalculatorRuns
            .SingleAsync(x => x.Id == runContext.RunId, cancellationToken);

        var materials = await dbContext.Material
            .AsNoTracking()
            .ToImmutableListAsync(cancellationToken);

        var materialsByCode = materials.ToImmutableDictionary(m => m.Code, StringComparer.OrdinalIgnoreCase);

        // ⚠️ Only set scalar FK columns (e.g. CalculatorRunId, MaterialId) on the entities below.
        // Navigation properties to existing rows (CalculatorRun, Material) are intentionally left
        // unset so that the IncludeGraph bulk insert below does not try to re-insert them.
        //
        // Only records with reported materials become a ProducerDetail row - e.g. a holding company
        // obligated in its own right but with no POM data of its own (its subsidiaries report on its
        // behalf) gets a CalculatorRunOrganisation row below but no ProducerDetail row, exactly as
        // before this type was unified. ProducerFeesBuilder/CalcResultScaledupProducersBuilder rely on
        // that absence to know to look the parent up via CalculatorRunOrganisation instead.
        var newProducerDetails = data
            .Where(producer => producer.ReportedMaterials.Count > 0)
            .Select(producer =>
            {
                var producerDetail = new ProducerDetail
                {
                    CalculatorRunId = calculatorRun.Id,
                    ProducerId = producer.OrganisationId,
                    TradingName = producer.TradingName,
                    SubsidiaryId = producer.SubsidiaryId,
                    ProducerName = producer.ProducerName,
                    SubmitterId = producer.SubmitterId,
                    ObligationStatus = producer.ObligationStatus,
                    DaysObligated = producer.DaysObligated,
                    JoinerDate = producer.JoinerDate,
                    LeaverDate = producer.LeaverDate,
                    StatusCode = producer.StatusCode
                };

                foreach (var reportedMaterial in producer.ReportedMaterials)
                    producerDetail.ProducerReportedMaterials.Add(ToProducerReportedMaterial(reportedMaterial, materialsByCode[reportedMaterial.MaterialCode]));

                return producerDetail;
            })
            .ToList();

        // ⚠️ Only set the scalar CalculatorRunId FK - the CalculatorRun navigation is intentionally
        // left unset so the bulk insert below does not try to re-insert it.
        var organisations = data
            .Select(record => ToCalculatorRunOrganisation(record, calculatorRun.Id))
            .ToList();

        var totalReportedMaterials = newProducerDetails.Sum(p => p.ProducerReportedMaterials.Count);

        logger.LogInformation(
            "Transpose produced {OrganisationCount} organisations, {ProducerDetailCount} producer details and {ReportedMaterialCount} reported materials",
            organisations.Count, newProducerDetails.Count, totalReportedMaterials);

        await bulkOps.BulkInsertAsync(dbContext, organisations, cancellationToken);

        await bulkOps.BulkInsertAsync(dbContext, newProducerDetails, cfg =>
        {
            // Must set IncludeGraph for EF navigational properties to be correctly set on the inserted entities.
            cfg.IncludeGraph = true;

            // When IncludeGraph is true, the bulk insert creates/drops tables before a final MERGE.
            // Set UseTempDB to use temp tables instead of 'proper' tables since they don't require permissions.
            cfg.UseTempDB = true;
        }, cancellationToken);

        var errors = data
            .SelectMany(record => record.Errors.Concat(record.Warnings)
                .Select(error => new OrganisationCalculationError
                {
                    OrganisationId = record.OrganisationId,
                    SubsidiaryId = record.SubsidiaryId,
                    Error = error
                }))
            .ToList();

        await errorReportService.PersistErrors(errors, calculatorRun.Id, calculatorRun.CreatedBy, runContext.RelativeYear, cancellationToken);

        calculatorRun.OrgPomDataLoadedAt = timeProvider.GetUtcNow().UtcDateTime;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static CalculatorRunOrganisation ToCalculatorRunOrganisation(ProducerRecord record, int calculatorRunId) => new()
    {
        CalculatorRunId = calculatorRunId,
        OrganisationId = record.OrganisationId,
        SubsidiaryId = record.SubsidiaryId,
        SubmitterId = record.SubmitterId,
        OrganisationName = record.ProducerName,
        TradingName = record.TradingName,
        ObligationStatus = record.ObligationStatus,
        DaysObligated = record.DaysObligated,
        JoinerDate = record.JoinerDate,
        LeaverDate = record.LeaverDate,
        StatusCode = record.StatusCode,
        ErrorCode = record.ErrorCode,
        HasH1 = record.HasH1,
        HasH2 = record.HasH2,
        IsError = record.IsError
    };

    private static ProducerReportedMaterial ToProducerReportedMaterial(AlignedReportedMaterial reportedMaterial, Material material) => new()
    {
        MaterialId = material.Id,
        PackagingType = reportedMaterial.PackagingType,
        SubmissionPeriod = reportedMaterial.SubmissionPeriod,
        PackagingTonnage = MathUtils.RoundAwayFromZero((decimal)reportedMaterial.TotalWeight / 1000m, decimals: 3),
        PackagingTonnageRed = MathUtils.RoundAwayFromZero((decimal)reportedMaterial.RedWeight / 1000m, decimals: 3),
        PackagingTonnageAmber = MathUtils.RoundAwayFromZero((decimal)reportedMaterial.AmberWeight / 1000m, decimals: 3),
        PackagingTonnageGreen = MathUtils.RoundAwayFromZero((decimal)reportedMaterial.GreenWeight / 1000m, decimals: 3),
        PackagingTonnageRedMedical = MathUtils.RoundAwayFromZero((decimal)reportedMaterial.RedMedicalWeight / 1000m, decimals: 3),
        PackagingTonnageAmberMedical = MathUtils.RoundAwayFromZero((decimal)reportedMaterial.AmberMedicalWeight / 1000m, decimals: 3),
        PackagingTonnageGreenMedical = MathUtils.RoundAwayFromZero((decimal)reportedMaterial.GreenMedicalWeight / 1000m, decimals: 3)
    };
}
