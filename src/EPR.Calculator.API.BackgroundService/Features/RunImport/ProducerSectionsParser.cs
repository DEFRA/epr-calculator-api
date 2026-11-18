using System.Collections.Immutable;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Features.RunImport;

/// <summary>
///     Reads the cancelled, scaled-up and partial obligation producer sections, finding columns by their headers since the wording has changed between layouts.
/// </summary>
public static class ProducerSectionsParser
{
    public static IReadOnlyList<CalcResultCancelledProducer> ReadCancelledProducers(BillingCsv csv)
    {
        var header = csv.FindRow("Producer ID", csv.FindRow("Cancelled Producers"));
        var columns = Columns(csv, header);
        var producers = new List<CalcResultCancelledProducer>();

        for (var row = header + 1; IsProducerRow(csv, row); row++)
        {
            decimal? Last(string material) => csv.NullableNumber(row, columns[material]);
            string? Text(params string[] names) => names.Select(n => columns.TryGetValue(n, out var c) ? c : (int?)null).FirstOrDefault(c => c is not null) is { } column
                ? csv.Text(row, column)
                : null;

            producers.Add(new CalcResultCancelledProducer
            {
                ProducerId               = (int)csv.Number(row, columns["producer id"]),
                SubsidiaryId             = Text("subsidiary id"),
                ProducerOrSubsidiaryName = Text("producer / subsidiary name", "producer name"),
                TradingName              = Text("trading name"),
                LastTonnage = new LastTonnage
                {
                    Aluminium      = Last("aluminium"),
                    FibreComposite = Last("fibre composite"),
                    Glass          = Last("glass"),
                    PaperOrCard    = Last("paper or card"),
                    Plastic        = Last("plastic"),
                    Steel          = Last("steel"),
                    Wood           = Last("wood"),
                    OtherMaterials = Last("other materials")
                },
                LatestInvoice = new LatestInvoice
                {
                    CurrentYearInvoicedTotalToDate = csv.NullableNumber(row, columns.TryGetValue("last invoiced total", out var total) ? total : columns["current year invoiced total to date"]),
                    RunNumber                      = csv.Text(row, columns["run number"]),
                    RunName                        = csv.Text(row, columns["run name"]),
                    BillingInstructionId           = csv.Text(row, columns["billing instruction id"])
                }
            });
        }

        return producers;
    }

    public record RejectedProducer(
        int ProducerId,
        string? ProducerName,
        string? TradingName,
        string SuggestedBillingInstruction,
        decimal? SuggestedInvoiceAmount,
        DateTime? ConfirmedAt,
        string? ConfirmedBy,
        string Reason);

    /// <summary>The Rejected Report; empty for files from before it was added.</summary>
    public static IReadOnlyList<RejectedProducer> ReadRejectedProducers(BillingCsv csv)
    {
        if (csv.TryFindRow("Rejected Report") is not { } section)
            return [];

        var header = csv.FindRow("Producer ID", section);
        var columns = Columns(csv, header);
        var producers = new List<RejectedProducer>();

        for (var row = header + 1; IsProducerRow(csv, row); row++)
            producers.Add(new RejectedProducer(
                (int)csv.Number(row, columns["producer id"]),
                csv.Text(row, columns["producer"]),
                csv.Text(row, columns["trading name"]),
                csv.Cell(row, columns["suggested billing instruction"]),
                csv.NullableNumber(row, columns["suggested invoice amount"]),
                csv.Date(row, columns["instruction confirmed date"]),
                csv.Text(row, columns["instruction confirmed by"]),
                csv.Cell(row, columns["reason for reject"])));

        return producers;
    }

    /// <summary>Returns null when the file has no scaled-up producers section.</summary>
    public static CalcResultScaledupProducers? ReadScaledupProducers(BillingCsv csv, IReadOnlyDictionary<string, string> materialCodes)
    {
        if (csv.TryFindRow("Scaled-up Producers") is not { } section)
            return null;

        var header = csv.FindRow("Producer ID", section);
        var columns = Columns(csv, header, from: 0, to: 9);
        var blocks = MaterialBlocks(csv, header - 1, materialCodes);
        var producers = new List<CalcResultScaledupProducer>();

        for (var row = header + 1; IsProducerRow(csv, row); row++)
        {
            producers.Add(new CalcResultScaledupProducer
            {
                ProducerId                        = (int)csv.Number(row, columns["producer id"]),
                SubsidiaryId                      = csv.Text(row, columns["subsidiary id"]),
                ProducerName                      = csv.Text(row, columns["producer / subsidiary name"]),
                TradingName                       = csv.Text(row, columns["trading name"]),
                Level                             = csv.Cell(row, columns["level"]),
                SubmissionPeriodCode              = csv.Cell(row, columns["submission period code"]),
                DaysInSubmissionPeriod            = (int)(csv.NullableNumber(row, columns["days in submission period"]) ?? -1),
                DaysInWholePeriod                 = (int)(csv.NullableNumber(row, columns["days in whole period"]) ?? -1),
                ScaleupFactor                     = csv.Number(row, columns["scale-up factor"]),
                ScaledupProducerTonnageByMaterial = blocks.ToDictionary(b => b.Code, b => ScaledTonnage(csv, row, b.Columns))
            });
        }

        return new CalcResultScaledupProducers { ScaledupProducers = [.. producers] };
    }

    /// <summary>Returns null when the file has no partial obligations section (runs before partial obligations).</summary>
    public static IReadOnlyList<CalcResultPartialObligation>? ReadPartialObligations(BillingCsv csv, IReadOnlyDictionary<string, string> materialCodes)
    {
        if (csv.TryFindRow("Partial Obligation Calculation") is not { } section)
            return null;

        var header = csv.FindRow("Producer ID", section);
        var columns = Columns(csv, header, from: 0, to: 10);
        var blocks = MaterialBlocks(csv, header - 1, materialCodes);
        var producers = new List<CalcResultPartialObligation>();

        for (var row = header + 1; IsProducerRow(csv, row); row++)
        {
            var daysInYear = (int)csv.Number(row, columns["days in submission year"]);
            var daysObligated = (int?)csv.NullableNumber(row, columns["obligated days"]);
            // Worked out from the days, as the run does, since the file only shows it to 2 decimal places.
            var obligatedFactor = daysObligated is { } days ? (decimal)days / daysInYear : 1;

            producers.Add(new CalcResultPartialObligation
            {
                ProducerId                         = (int)csv.Number(row, columns["producer id"]),
                SubsidiaryId                       = csv.Text(row, columns["subsidiary id"]),
                ProducerName                       = csv.Text(row, columns["producer / subsidiary name"]),
                TradingName                        = csv.Text(row, columns["trading name"]),
                Level                              = csv.Cell(row, columns["level"]),
                SubmissionYear                     = (int)csv.Number(row, columns["submission year"]),
                DaysInSubmissionYear               = daysInYear,
                JoiningDate                        = csv.Text(row, columns["joining date"]),
                DaysObligated                      = daysObligated,
                ObligatedFactor                    = obligatedFactor,
                PartialObligationTonnageByMaterial = blocks.ToDictionary(b => b.Code, b => PartialTonnage(csv, row, b.Columns, obligatedFactor))
            });
        }

        return producers;
    }

    private static CalcResultPartialObligationTonnage PartialTonnage(BillingCsv csv, int row, IReadOnlyDictionary<string, int> columns, decimal obligatedFactor)
    {
        decimal? Value(string name) => columns.TryGetValue(name, out var column) ? csv.NullableNumber(row, column) ?? 0 : null;

        // Only runs with modulation show the RAM split.
        RamTonnage? Ram(string packaging) => columns.ContainsKey($"{packaging} red material tonnage")
            ? new RamTonnage
            {
                Red          = Value($"{packaging} red material tonnage") ?? 0,
                Amber        = Value($"{packaging} amber material tonnage") ?? 0,
                Green        = Value($"{packaging} green material tonnage") ?? 0,
                RedMedical   = Value($"{packaging} red medical material tonnage") ?? 0,
                AmberMedical = Value($"{packaging} amber medical material tonnage") ?? 0,
                GreenMedical = Value($"{packaging} green medical material tonnage") ?? 0
            }
            : null;

        return new CalcResultPartialObligationTonnage
        {
            ObligatedFactor                            = obligatedFactor,
            HouseholdTonnage                           = Value("household tonnage") ?? 0,
            HouseholdRAMTonnage                        = Ram("household"),
            PublicBinTonnage                           = Value("public bin tonnage") ?? 0,
            PublicBinRAMTonnage                        = Ram("public bin"),
            HouseholdDrinksContainersTonnage           = Value("household drinks containers tonnage"),
            HouseholdDrinksContainersRAMTonnage        = Ram("household drinks containers"),
            TotalTonnage                               = Value("total tonnage") ?? 0,
            SelfManagedConsumerWasteTonnage            = Value("self-managed consumer waste tonnage") ?? 0,
            PartialHouseholdTonnage                    = Value("partial household tonnage") ?? 0,
            PartialHouseholdRAMTonnage                 = Ram("partial household"),
            PartialPublicBinTonnage                    = Value("partial public bin tonnage") ?? 0,
            PartialPublicBinRAMTonnage                 = Ram("partial public bin"),
            PartialHouseholdDrinksContainersTonnage    = Value("partial household drinks containers tonnage"),
            PartialHouseholdDrinksContainersRAMTonnage = Ram("partial household drinks containers"),
            PartialTotalTonnage                        = Value("partial total tonnage") ?? 0,
            PartialSelfManagedConsumerWasteTonnage     = Value("partial self-managed consumer waste tonnage") ?? 0
        };
    }

    private static CalcResultScaledupProducerTonnage ScaledTonnage(BillingCsv csv, int row, IReadOnlyDictionary<string, int> columns)
    {
        decimal Value(string name) => columns.TryGetValue(name, out var column) ? csv.NullableNumber(row, column) ?? 0 : 0;

        return new CalcResultScaledupProducerTonnage
        {
            ReportedHouseholdPackagingWasteTonnage          = Value("household tonnage"),
            ReportedPublicBinTonnage                        = Value("public bin tonnage"),
            HouseholdDrinksContainersTonnageGlass           = Value("household drinks containers tonnage - glass"),
            TotalReportedTonnage                            = Value("total tonnage"),
            ReportedSelfManagedConsumerWasteTonnage         = Value("self-managed consumer waste tonnage"),
            NetReportedTonnage                              = Value("net tonnage"),
            ScaledupReportedHouseholdPackagingWasteTonnage  = Value("scaled-up household tonnage"),
            ScaledupReportedPublicBinTonnage                = Value("scaled-up public bin tonnage"),
            ScaledupHouseholdDrinksContainersTonnageGlass   = Value("scaled-up household drinks containers tonnage - glass"),
            ScaledupTotalReportedTonnage                    = Value("scaled-up total tonnage"),
            ScaledupReportedSelfManagedConsumerWasteTonnage = Value("scaled-up self-managed consumer waste tonnage"),
            ScaledupNetReportedTonnage                      = Value("scaled-up net tonnage")
        };
    }

    /// <summary>Each material's breakdown, from the group row's "{material} Breakdown" headings.</summary>
    internal static IReadOnlyList<(string Code, IReadOnlyDictionary<string, int> Columns)> MaterialBlocks(
        BillingCsv csv, int groupRow, IReadOnlyDictionary<string, string> materialCodes)
    {
        var starts = Enumerable.Range(0, csv.Rows[groupRow].Length)
            .Where(c => csv.Cell(groupRow, c).EndsWith(" Breakdown", StringComparison.OrdinalIgnoreCase))
            .ToList();

        return [.. starts.Select((start, i) =>
        {
            var material = csv.Cell(groupRow, start)[..^" Breakdown".Length];
            var end = i + 1 < starts.Count ? starts[i + 1] : csv.Rows[groupRow + 1].Length;
            return (materialCodes[material], Columns(csv, groupRow + 1, start, end));
        })];
    }

    /// <summary>Column positions by normalised header, so older wording ("Household Packaging Tonnage", "Self Managed") matches.</summary>
    internal static IReadOnlyDictionary<string, int> Columns(BillingCsv csv, int headerRow, int from = 0, int? to = null)
    {
        var columns = new Dictionary<string, int>();
        for (var c = from; c < (to ?? csv.Rows[headerRow].Length); c++)
            columns.TryAdd(Normalise(csv.Cell(headerRow, c)), c);
        return columns;
    }

    internal static string Normalise(string header) =>
        header.ToLowerInvariant()
            .Replace("household packaging tonnage", "household tonnage")
            .Replace("self managed", "self-managed")
            .Replace("producer household packaging", "household tonnage")
            .Replace("joiners date", "joiner's date")
            .Replace("leavers date", "leaver's date")
            .Trim();

    private static bool IsProducerRow(BillingCsv csv, int row) =>
        row < csv.Rows.Count && int.TryParse(csv.Cell(row, 0), out _);
}
