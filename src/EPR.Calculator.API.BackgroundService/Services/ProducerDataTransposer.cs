using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Data.Utils;
using EPR.CommonDataService.DataApi.CommonDataApi.Alignment;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.BackgroundService.Services;

public interface IProducerDataTransposer
{
    /// <summary>
    ///     Transposes POM and organisation data for a given calculator run into ProducerDetails and ProducerReportedMaterials.
    /// </summary>
    Task Transpose(CalculatorRunContext runContext, CancellationToken cancellationToken);
}

public class ProducerDataTransposer(
    ApplicationDBContext dbContext,
    IBulkOperations bulkOps,
    IErrorReportService errorReportService,
    IProducerPomAligner aligner,
    ILogger<ProducerDataTransposer> logger
) : IProducerDataTransposer
{
    [ActivityTrace]
    public async Task Transpose(CalculatorRunContext runContext, CancellationToken cancellationToken)
    {
        var calculatorRun = await dbContext.CalculatorRuns
            .AsNoTracking()
            .Where(x => x.Id == runContext.RunId
                        && x.CalculatorRunOrganisationDataMaster != null
                        && x.CalculatorRunPomDataMaster != null)
            .SingleAsync(cancellationToken);

        var materials = await dbContext.Material
            .AsNoTracking()
            .ToImmutableListAsync(cancellationToken);

        var calculatorRunOrgDataDetails = await dbContext.CalculatorRunOrganisationDataDetails
            .AsNoTracking()
            .Where(x => x.CalculatorRunOrganisationDataMasterId == calculatorRun.CalculatorRunOrganisationDataMasterId)
            .ToImmutableListAsync(cancellationToken);

        var calculatorRunPomDataDetails = await dbContext.CalculatorRunPomDataDetails
            .AsNoTracking()
            .Where(x => x.CalculatorRunPomDataMasterId == calculatorRun.CalculatorRunPomDataMasterId)
            .ToImmutableListAsync(cancellationToken);

        var unmatchedSet = await errorReportService.HandleErrors(
            calculatorRunPomDataDetails,
            calculatorRunOrgDataDetails,
            calculatorRun.Id,
            calculatorRun.CreatedBy,
            calculatorRun.RelativeYear,
            cancellationToken);

        calculatorRunPomDataDetails = calculatorRunPomDataDetails
            .Where(p =>
            {
                var orgId = p.OrganisationId.GetValueOrDefault();
                var subId = p.SubsidiaryId;
                return !unmatchedSet.Contains((orgId, subId));
            })
            .ToImmutableList();

        var materialCodes = materials.Select(m => m.Code).ToImmutableList();
        var materialsByCode = materials.ToImmutableDictionary(m => m.Code, StringComparer.OrdinalIgnoreCase);

        var alignedProducers = aligner.Align(
            calculatorRunOrgDataDetails.Select(ToAlignmentOrganisation).ToImmutableList(),
            calculatorRunPomDataDetails.Select(ToAlignmentPom).ToImmutableList(),
            materialCodes);

        // ⚠️ Only set scalar FK columns (e.g. CalculatorRunId, MaterialId) on the entities below.
        // Navigation properties to existing rows (CalculatorRun, Material) are intentionally left
        // unset so that the IncludeGraph bulk insert below does not try to re-insert them.
        var newProducerDetails = alignedProducers
            .Select(producer =>
            {
                var producerDetail = new ProducerDetail
                {
                    CalculatorRunId = calculatorRun.Id,
                    ProducerId = producer.OrganisationId,
                    TradingName = producer.TradingName,
                    SubsidiaryId = producer.SubsidiaryId,
                    ProducerName = producer.ProducerName
                };

                foreach (var reportedMaterial in producer.ReportedMaterials)
                    producerDetail.ProducerReportedMaterials.Add(ToProducerReportedMaterial(reportedMaterial, materialsByCode[reportedMaterial.MaterialCode]));

                return producerDetail;
            })
            .ToList();

        var totalReportedMaterials = newProducerDetails.Sum(p => p.ProducerReportedMaterials.Count);

        logger.LogInformation("Transpose produced {ProducerDetailCount} producer details and {ReportedMaterialCount} reported materials",
            newProducerDetails.Count, totalReportedMaterials);

        await bulkOps.BulkInsertAsync(dbContext, newProducerDetails, cfg =>
        {
            // Must set IncludeGraph for EF navigational properties to be correctly set on the inserted entities.
            cfg.IncludeGraph = true;

            // When IncludeGraph is true, the bulk insert creates/drops tables before a final MERGE.
            // Set UseTempDB to use temp tables instead of 'proper' tables since they don't require permissions.
            cfg.UseTempDB = true;
        }, cancellationToken);
    }

    private static AlignmentOrganisation ToAlignmentOrganisation(CalculatorRunOrganisationDataDetail o) => new()
    {
        OrganisationId = o.OrganisationId,
        SubsidiaryId = o.SubsidiaryId,
        SubmitterId = o.SubmitterId,
        OrganisationName = o.OrganisationName,
        TradingName = o.TradingName,
        ObligationStatus = o.ObligationStatus,
        HasH2 = o.HasH2
    };

    private static AlignmentPom ToAlignmentPom(CalculatorRunPomDataDetail p) => new()
    {
        OrganisationId = p.OrganisationId,
        SubsidiaryId = p.SubsidiaryId,
        SubmitterId = p.SubmitterId,
        PackagingMaterial = p.PackagingMaterial,
        PackagingType = p.PackagingType,
        SubmissionPeriod = p.SubmissionPeriod,
        PackagingMaterialWeight = p.PackagingMaterialWeight,
        RamRagRating = p.RamRagRating?.ToDbValue()
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
