namespace EPR.CommonDataService.DataApi.Alignment;

public interface IProducerPomAligner
{
    /// <summary>
    ///     Dedupes multiple registrations per organisation/subsidiary/submitter down to one row each,
    ///     preferring the row with <see cref="AlignmentOrganisation.HasH2" /> set. Applies no other
    ///     filtering - the result includes every organisation regardless of obligation status.
    /// </summary>
    IReadOnlyList<AlignmentOrganisation> DedupeOrganisations(IReadOnlyCollection<AlignmentOrganisation> organisations);

    /// <summary>
    ///     Aligns organisation and POM data into producer records: filters to obligated organisations,
    ///     matches POMs to their organisation, and aggregates reported weights by material and RAG
    ///     rating. Yields a record for every obligated organisation, even one with no POM data of its
    ///     own (e.g. a holding company whose subsidiaries report on its behalf) - <see cref="ProducerRecord.ReportedMaterials" />
    ///     is empty in that case. <see cref="ProducerRecord.Errors" /> and <see cref="ProducerRecord.Warnings" />
    ///     are left unset - it's the caller's job to attach detection results.
    /// </summary>
    /// <param name="organisations">Deduped organisation rows (see <see cref="DedupeOrganisations" />) to align.</param>
    /// <param name="poms">POM rows to align.</param>
    /// <param name="materialCodes">
    ///     The known material codes, in the order reported materials should be produced.
    /// </param>
    IEnumerable<ProducerRecord> Align(
        IReadOnlyCollection<AlignmentOrganisation> organisations,
        IReadOnlyCollection<AlignmentPom> poms,
        IReadOnlyList<string> materialCodes);
}

public sealed class ProducerPomAligner : IProducerPomAligner
{
    private const string ObligatedStatus = "O";

    public IReadOnlyList<AlignmentOrganisation> DedupeOrganisations(IReadOnlyCollection<AlignmentOrganisation> organisations) =>
        organisations
            .GroupBy(o => (o.OrganisationId, o.SubsidiaryId, o.SubmitterId))
            // PERF: MaxBy is O(n) and avoids the OrderByDescending(...).First() O(n log n) sort + allocation per group.
            .Select(grp => grp.MaxBy(o => o.HasH2)!)
            .ToImmutableList();

    public IEnumerable<ProducerRecord> Align(
        IReadOnlyCollection<AlignmentOrganisation> organisations,
        IReadOnlyCollection<AlignmentPom> poms,
        IReadOnlyList<string> materialCodes) =>
        DataApiTelemetry.Trace(typeof(ProducerPomAligner), nameof(Align),
            () => AlignCore(organisations, poms, materialCodes).ToList());

    private static IEnumerable<ProducerRecord> AlignCore(
        IReadOnlyCollection<AlignmentOrganisation> organisations,
        IReadOnlyCollection<AlignmentPom> poms,
        IReadOnlyList<string> materialCodes)
    {
        var obligatedOrganisations = organisations
            .Where(o => o.ObligationStatus == ObligatedStatus && !string.IsNullOrWhiteSpace(o.OrganisationName));

        // PERF: pre-build an O(1) lookup of POMs keyed by (OrganisationId, SubsidiaryId, SubmitterId).
        // We also pre-apply the PackagingType / OrganisationId.HasValue filters here so each per-organisation
        // slice is ready to group by material code directly.
        var pomsByOrgSubSubmitter = poms
            .Where(p => p is { PackagingType: not null, OrganisationId: not null } && IsReportablePackaging(p))
            .ToLookup(p => (OrganisationId: p.OrganisationId!.Value, p.SubsidiaryId, p.SubmitterId));

        foreach (var organisation in obligatedOrganisations)
        {
            var orgPoms = pomsByOrgSubSubmitter[(organisation.OrganisationId, organisation.SubsidiaryId, organisation.SubmitterId)];

            var pomsByMaterial = orgPoms
                .GroupBy(p => p.PackagingMaterial!)
                .ToImmutableDictionary(grp => grp.Key,
                    grp => grp.ToImmutableList(),
                    StringComparer.OrdinalIgnoreCase);

            // No POM data of its own - e.g. a holding company whose subsidiaries report on its behalf.
            // Still yielded (with no reported materials) so callers that need the org's identity don't
            // have to fall back to a separate, unfiltered organisation population.
            var reportedMaterials = pomsByMaterial.Count == 0
                ? []
                : GetReportedMaterials(materialCodes, pomsByMaterial).ToImmutableList();

            yield return new ProducerRecord
            {
                OrganisationId = organisation.OrganisationId,
                SubsidiaryId = organisation.SubsidiaryId,
                TradingName = organisation.TradingName,
                ProducerName = organisation.OrganisationName,
                DaysObligated = organisation.DaysObligated,
                JoinerDate = organisation.JoinerDate,
                LeaverDate = organisation.LeaverDate,
                StatusCode = organisation.StatusCode,
                ErrorCode = organisation.ErrorCode,
                Errors = [],
                Warnings = [],
                ReportedMaterials = reportedMaterials
            };
        }
    }

    private static bool IsReportablePackaging(AlignmentPom pom) =>
        ReportablePackaging.Includes(pom.PackagingType, pom.PackagingMaterial);

    private static IEnumerable<AlignedReportedMaterial> GetReportedMaterials(
        IReadOnlyList<string> materialCodes,
        ImmutableDictionary<string, ImmutableList<AlignmentPom>> pomsByMaterial)
    {
        foreach (var materialCode in materialCodes)
        {
            if (!pomsByMaterial.TryGetValue(materialCode, out var materialPoms))
                continue;

            // PERF: ValueTuple key avoids the anonymous-type allocation per group.
            foreach (var poms in materialPoms.GroupBy(p => (p.SubmissionPeriod, p.PackagingType)))
            {
                // PERF: single pass over the group computing every weight breakdown
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
                        case "R": red += weight; break;
                        case "A": amber += weight; break;
                        case "G": green += weight; break;
                        case "R-M": redMedical += weight; break;
                        case "A-M": amberMedical += weight; break;
                        case "G-M": greenMedical += weight; break;
                    }
                }

                yield return new AlignedReportedMaterial
                {
                    MaterialCode = materialCode,
                    PackagingType = poms.Key.PackagingType!,
                    SubmissionPeriod = poms.Key.SubmissionPeriod!,
                    TotalWeight = total,
                    RedWeight = red,
                    AmberWeight = amber,
                    GreenWeight = green,
                    RedMedicalWeight = redMedical,
                    AmberMedicalWeight = amberMedical,
                    GreenMedicalWeight = greenMedical
                };
            }
        }
    }
}
