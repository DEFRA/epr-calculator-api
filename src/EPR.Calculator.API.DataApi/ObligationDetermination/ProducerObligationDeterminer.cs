using System.Globalization;
using EPR.CommonDataService.DataApi.CommonDataApi.Entities;

namespace EPR.CommonDataService.DataApi.ObligationDetermination;

/// <summary>
///     Classifies producer registrations as Obligated (O) / Not Obligated (N) / Error (E), ported from
///     dbo.fn_ProducerObligationDetermination.sql (and the matching PySpark reference implementation in
///     epr-data's epr-obligation-determination module). Operates on the whole set of registrations for a
///     run at once, since the decision depends on cross-row aggregation (how many registrations share a
///     producer in the same submission period).
/// </summary>
public interface IProducerObligationDeterminer
{
    /// <summary>
    ///     Returns the given organisations with ObligationStatus/NumDaysObligated/ErrorCode populated.
    ///     Row identity and count are preserved 1:1 - this performs no filtering or deduplication.
    /// </summary>
    IReadOnlyList<PayCalOrganisation> Determine(IReadOnlyList<PayCalOrganisation> organisations);
}

public sealed class ProducerObligationDeterminer : IProducerObligationDeterminer
{
    private const string RawObligated = "Obligated";
    private const string RawNotObligated = "Not Obligated";
    private const string RawInvalid = "Invalid leaver code";
    private const string RawBlank = "Blank";

    private static readonly HashSet<string> InvalidLeaverCodesWithSubsidiary = ["09", "18"];
    private static readonly HashSet<string> InvalidLeaverCodesWithoutSubsidiary = ["06", "07", "08", "10"];
    private static readonly HashSet<string> BlankLeaverCodes = ["11", "12"];
    private static readonly HashSet<string> ObligatedLeaverCodes = ["01", "02", "03", "04", "05", "06", "08", "10", "15", "17", "19", "20"];
    private static readonly HashSet<string> NotObligatedLeaverCodes = ["07", "09", "13", "14", "16", "18", "21"];
    private static readonly HashSet<string> DateSensitiveLeaverCodes = ["02", "03"];

    public IReadOnlyList<PayCalOrganisation> Determine(IReadOnlyList<PayCalOrganisation> organisations) =>
        DataApiTelemetry.Trace(typeof(ProducerObligationDeterminer), nameof(Determine), () =>
        {
            var rows = organisations.Select(o => new Row(o, ComputeProducerId(o))).ToList();

            ComputeRawObligationStatus(rows);
            ApplyStatusInheritance(rows);

            var pivotCounts = PivotRawObligation(rows);

            ApplyDecisionTree(rows, pivotCounts);
            ApplyRule11And12(rows);
            ApplyRule13And14(rows);
            ApplyRule16(rows);

            return rows
                .Select(r => r.Source with
                {
                    ObligationStatus = r.ObligationStatus,
                    NumDaysObligated = r.NumDaysObligated,
                    ErrorCode = r.ErrorCode
                })
                .ToList();
        });

    private static int? ComputeProducerId(PayCalOrganisation o) =>
        int.TryParse(o.SubsidiaryId, out var subsidiaryId) ? subsidiaryId : o.OrganisationId;

    private static void ComputeRawObligationStatus(List<Row> rows)
    {
        foreach (var row in rows)
        {
            row.RawObligationStatus = ComputeRawObligationStatus(row.Source);
        }
    }

    private static string ComputeRawObligationStatus(PayCalOrganisation o)
    {
        if (o.RegulatorStatus == "Cancelled")
        {
            return RawNotObligated;
        }

        if (string.IsNullOrEmpty(o.StatusCode))
        {
            return RawBlank;
        }

        if (InvalidLeaverCodesWithSubsidiary.Contains(o.StatusCode) && o.SubsidiaryId is not null)
        {
            return RawInvalid;
        }

        if (InvalidLeaverCodesWithoutSubsidiary.Contains(o.StatusCode) && o.SubsidiaryId is null)
        {
            return RawInvalid;
        }

        if (BlankLeaverCodes.Contains(o.StatusCode))
        {
            return RawBlank;
        }

        if (ObligatedLeaverCodes.Contains(o.StatusCode))
        {
            return RawObligated;
        }

        if (NotObligatedLeaverCodes.Contains(o.StatusCode))
        {
            return RawNotObligated;
        }

        return RawInvalid;
    }

    /// <summary>
    ///     If any parent registration (no subsidiary) in an organisation/submitter/period group is
    ///     "Not Obligated", every registration in that group inherits "Not Obligated".
    /// </summary>
    private static void ApplyStatusInheritance(List<Row> rows)
    {
        var groups = rows.GroupBy(r => (r.Source.OrganisationId, r.Source.SubmitterId, r.Source.SubmissionPeriodYear));

        foreach (var group in groups)
        {
            var isNotObligatedGroup = group.Any(r => r.Source.SubsidiaryId is null && r.RawObligationStatus == RawNotObligated);
            if (!isNotObligatedGroup)
            {
                continue;
            }

            foreach (var row in group)
            {
                row.RawObligationStatus = RawNotObligated;
            }
        }
    }

    private static Dictionary<(int? ProducerId, int? Year), PivotCounts> PivotRawObligation(List<Row> rows) =>
        rows
            .GroupBy(r => (r.ProducerId, r.Source.SubmissionPeriodYear))
            .ToDictionary(
                g => g.Key,
                g => new PivotCounts(
                    Obligated: g.Count(r => (r.RawObligationStatus ?? RawBlank) == RawObligated),
                    NotObligated: g.Count(r => (r.RawObligationStatus ?? RawBlank) == RawNotObligated),
                    Blank: g.Count(r => (r.RawObligationStatus ?? RawBlank) == RawBlank)));

    private static void ApplyDecisionTree(List<Row> rows, Dictionary<(int?, int?), PivotCounts> pivotCounts)
    {
        foreach (var row in rows)
        {
            var pivot = pivotCounts[(row.ProducerId, row.Source.SubmissionPeriodYear)];
            var joinerDate = ParseJoinerDate(row.Source.JoinerDate);
            var isDateSensitiveCode = row.Source.StatusCode is not null && DateSensitiveLeaverCodes.Contains(row.Source.StatusCode);
            var yearMismatch = isDateSensitiveCode && joinerDate is not null && row.Source.SubmissionPeriodYear != joinerDate.Value.Year;

            if (yearMismatch)
            {
                row.ObligationStatus = "E";
                row.ErrorCode = "Date input issue";
            }
            else if (row.RawObligationStatus == RawInvalid)
            {
                row.ObligationStatus = "E";
                row.ErrorCode = RawInvalid;
            }
            else if (pivot is { Obligated: 0, Blank: 0, NotObligated: > 0 })
            {
                row.ObligationStatus = "E";
                row.ErrorCode = RawNotObligated;
            }
            else if (pivot is { Obligated: 0, Blank: > 1 })
            {
                row.ObligationStatus = "E";
                row.ErrorCode = "Conflicting Obligations (Blanks)";
            }
            else if (pivot is { Obligated: 0, Blank: 1 })
            {
                row.ObligationStatus = row.RawObligationStatus == RawBlank ? "O" : "N";
                row.ErrorCode = null;
            }
            else if (pivot.Obligated == 1)
            {
                row.ObligationStatus = row.RawObligationStatus == RawObligated ? "O" : "N";
                row.ErrorCode = null;
            }
            else
            {
                // pivot.Obligated > 1, or any other combination not covered above.
                row.ObligationStatus = "E";
                row.ErrorCode = pivot.Obligated > 1 ? "Conflicting Obligations (Leaver Codes)" : "E";
            }

            row.NumDaysObligated = isDateSensitiveCode && joinerDate is not null && joinerDate.Value.Year == row.Source.SubmissionPeriodYear
                ? (short)(new DateTime(joinerDate.Value.Year, 12, 31).DayOfYear - joinerDate.Value.DayOfYear + 1)
                : null;
        }
    }

    private static DateTime? ParseJoinerDate(string? joinerDate) =>
        DateTime.TryParseExact(joinerDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;

    /// <summary>Leaver codes 11/12 ("no longer trading") always end up as an error if otherwise Not Obligated.</summary>
    private static void ApplyRule11And12(List<Row> rows)
    {
        foreach (var row in rows.Where(r => r.Source.StatusCode is "11" or "12"))
        {
            row.ErrorCode ??= "No longer trading";
            if (row.ObligationStatus == "N")
            {
                row.ObligationStatus = "E";
            }
        }
    }

    /// <summary>Leaver codes 13/14 ("compliance scheme leaver") relabel an existing error.</summary>
    private static void ApplyRule13And14(List<Row> rows)
    {
        foreach (var row in rows.Where(r => r.Source.StatusCode is "13" or "14" && r.ObligationStatus == "E"))
        {
            row.ErrorCode = "Compliance Scheme Leaver";
        }
    }

    /// <summary>
    ///     Leaver code 16 ("merged with another producer") is always an error, and forces an error on any
    ///     other registration sharing the same producer/period (an obligation can't stand next to a merge).
    /// </summary>
    private static void ApplyRule16(List<Row> rows)
    {
        foreach (var row in rows.Where(r => r.Source.StatusCode == "16"))
        {
            row.ObligationStatus = "E";
            row.ErrorCode = "Merged with another Producer";
        }

        var producersWith16 = rows
            .Where(r => r.Source.StatusCode == "16")
            .Select(r => (r.ProducerId, r.Source.SubmissionPeriodYear))
            .ToHashSet();

        foreach (var row in rows.Where(r => r.ObligationStatus == "O" && producersWith16.Contains((r.ProducerId, r.Source.SubmissionPeriodYear))))
        {
            row.ObligationStatus = "E";
            row.ErrorCode = "Conflicting Obligations (Leaver code)";
        }
    }

    private sealed class Row(PayCalOrganisation source, int? producerId)
    {
        public PayCalOrganisation Source { get; } = source;

        public int? ProducerId { get; } = producerId;

        public string? RawObligationStatus { get; set; }

        public string? ObligationStatus { get; set; }

        public short? NumDaysObligated { get; set; }

        public string? ErrorCode { get; set; }
    }

    private readonly record struct PivotCounts(int Obligated, int NotObligated, int Blank);
}
