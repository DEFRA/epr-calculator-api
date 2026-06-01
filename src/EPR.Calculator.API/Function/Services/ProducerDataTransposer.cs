using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Utils;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services;

public interface IProducerDataTransposer
{
    /// <summary>
    ///     Transposes POM and organisation data for a given calculator run into ProducerDetails and ProducerReportedMaterials.
    /// </summary>
    Task Transpose(int runId, CancellationToken cancellationToken);
}

[ExcludeFromCodeCoverage]
public class ProducerDataTransposer(
    ApplicationDBContext dbContext,
    IBulkOperations bulkOps,
    IErrorReportService errorReportService,
    ILogger<ProducerDataTransposer> logger)
    : IProducerDataTransposer
{
    public async Task Transpose(int runId, CancellationToken cancellationToken)
    {
        var calculatorRun = await dbContext.CalculatorRuns
            .AsNoTracking()
            .Where(x => x.Id == runId
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

        var organisationDataDetails = calculatorRunOrgDataDetails
            .Where(odd => ObligationStates.IsObligated(odd.ObligationStatus)
                          && !string.IsNullOrWhiteSpace(odd.OrganisationName))
            .GroupBy(odd => new { odd.OrganisationId, odd.SubsidiaryId, odd.SubmitterId })
            // PERF: MaxBy is O(n) and avoids the OrderByDescending(...).First() O(n log n) sort + allocation per group.
            .Select(grp => grp.MaxBy(o => o.HasH2)!)
            .ToImmutableList();

        // PERF: pre-build an O(1) lookup of POMs keyed by (OrganisationId, SubsidiaryId, SubmitterId).
        // We also pre-apply the PackagingType / OrganisationId.HasValue filters here so each per-organisation
        // slice is ready to group by material code directly.
        var pomsByOrgSubSubmitter = calculatorRunPomDataDetails
            .Where(pdd => pdd is { PackagingType: not null, OrganisationId: not null })
            .ToLookup(pdd => (OrganisationId: pdd.OrganisationId!.Value, pdd.SubsidiaryId, pdd.SubmitterId));

        // PERF: pre-size to avoid repeated List<T> internal-array reallocations as we Add per organisation.
        var newProducerDetails = new List<ProducerDetail>(organisationDataDetails.Count);

        foreach (var organisation in organisationDataDetails)
        {
            var orgPoms = pomsByOrgSubSubmitter[(organisation.OrganisationId, organisation.SubsidiaryId, organisation.SubmitterId)];

            var subsidiaryPomsByMaterial = orgPoms
                .GroupBy(pdd => pdd.PackagingMaterial!)
                .ToImmutableDictionary(grp => grp.Key,
                    grp => grp.ToImmutableList(),
                    StringComparer.OrdinalIgnoreCase);

            if (subsidiaryPomsByMaterial.Count == 0)
                continue;

            // ⚠️ Only set scalar FK columns (e.g. CalculatorRunId, MaterialId) on the entities below.
            // Navigation properties to existing rows (CalculatorRun, Material) are intentionally left
            // unset so that the IncludeGraph bulk insert below does not try to re-insert them.
            var producerDetail = new ProducerDetail
            {
                CalculatorRunId = calculatorRun.Id,
                ProducerId = organisation.OrganisationId,
                TradingName = organisation.TradingName,
                SubsidiaryId = organisation.SubsidiaryId,
                ProducerName = organisation.OrganisationName
            };

            foreach (var reportedMaterial in GetProducerReportedMaterials(materials, subsidiaryPomsByMaterial))
                producerDetail.ProducerReportedMaterials.Add(reportedMaterial);

            newProducerDetails.Add(producerDetail);
        }

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

    private static IEnumerable<ProducerReportedMaterial> GetProducerReportedMaterials(ImmutableList<Material> materials, ImmutableDictionary<string, ImmutableList<CalculatorRunPomDataDetail>> pomsByMaterial)
    {
        foreach (var material in materials)
        {
            if (!pomsByMaterial.TryGetValue(material.Code, out var subsidiaryPomsForMaterial))
                continue;

            // PERF: ValueTuple key avoids the anonymous-type allocation per group.
            foreach (var poms in subsidiaryPomsForMaterial.GroupBy(p => (p.SubmissionPeriod, p.PackagingType)))
            {
                // PERF: single pass over the group computing every tonnage breakdown
                double total = 0d,
                    red = 0d,
                    amber = 0d,
                    green = 0d,
                    redMedical = 0d,
                    amberMedical = 0d,
                    greenMedical = 0d;

                foreach (var pom in poms)
                {
                    var weight = pom.PackagingMaterialWeight ?? 0d;
                    total += weight;

                    switch (pom.RamRagRating)
                    {
                        case RagRating.Red: red += weight; break;
                        case RagRating.Amber: amber += weight; break;
                        case RagRating.Green: green += weight; break;
                        case RagRating.RedMedical: redMedical += weight; break;
                        case RagRating.AmberMedical: amberMedical += weight; break;
                        case RagRating.GreenMedical: greenMedical += weight; break;
                    }
                }

                yield return new ProducerReportedMaterial
                {
                    MaterialId = material.Id,
                    PackagingType = poms.Key.PackagingType!,
                    SubmissionPeriod = poms.Key.SubmissionPeriod!,
                    PackagingTonnage = Math.Round((decimal)total / 1000m, 3),
                    PackagingTonnageRed = Math.Round((decimal)red / 1000m, 3),
                    PackagingTonnageAmber = Math.Round((decimal)amber / 1000m, 3),
                    PackagingTonnageGreen = Math.Round((decimal)green / 1000m, 3),
                    PackagingTonnageRedMedical = Math.Round((decimal)redMedical / 1000m, 3),
                    PackagingTonnageAmberMedical = Math.Round((decimal)amberMedical / 1000m, 3),
                    PackagingTonnageGreenMedical = Math.Round((decimal)greenMedical / 1000m, 3)
                };
            }
        }
    }
}
