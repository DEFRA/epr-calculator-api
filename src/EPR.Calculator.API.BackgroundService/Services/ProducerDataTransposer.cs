using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Utils;
using EPR.CommonDataService.DataApi.Alignment;
using EPR.CommonDataService.DataApi.CommonDataApi;
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
        ProducerCalculationData data,
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
        ProducerCalculationData data,
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
        var newProducerDetails = data.Producers
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
        var organisations = data.Organisations
            .Select(o => ToCalculatorRunOrganisation(o, calculatorRun.Id))
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

        await errorReportService.PersistErrors(data.Errors, calculatorRun.Id, calculatorRun.CreatedBy, cancellationToken);

        calculatorRun.OrgPomDataLoadedAt = timeProvider.GetUtcNow().UtcDateTime;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static CalculatorRunOrganisation ToCalculatorRunOrganisation(AlignmentOrganisation o, int calculatorRunId) => new()
    {
        CalculatorRunId = calculatorRunId,
        OrganisationId = o.OrganisationId,
        SubsidiaryId = o.SubsidiaryId,
        SubmitterId = o.SubmitterId,
        OrganisationName = o.OrganisationName,
        TradingName = o.TradingName,
        ObligationStatus = o.ObligationStatus,
        DaysObligated = o.DaysObligated,
        JoinerDate = o.JoinerDate,
        LeaverDate = o.LeaverDate,
        StatusCode = o.StatusCode,
        ErrorCode = o.ErrorCode,
        HasH1 = o.HasH1,
        HasH2 = o.HasH2
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
