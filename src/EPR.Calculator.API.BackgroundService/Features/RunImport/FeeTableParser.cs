using EPR.Calculator.API.BackgroundService.Constants;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Features.RunImport;

/// <summary>
///     Reads the producer fee table (the "Calculation Result" section) into fee details, finding each column by its
///     section, breakdown and header, since columns have been added and reworded between layouts.
/// </summary>
public static class FeeTableParser
{
    public record FeeTable(IReadOnlyList<FeeDetail> Producers, FeeDetail Total, IReadOnlyList<string> Warnings);

    public static FeeTable Read(BillingCsv csv, IReadOnlyDictionary<string, string> materialCodes)
    {
        var header = csv.FindRow("Producer ID", csv.FindRow("Calculation Result"));
        var columns = new Columns(csv, header);
        var warnings = new List<string>();

        var producers = new List<FeeDetail>();
        var row = header + 1;
        for (; row < csv.Rows.Count && int.TryParse(csv.Cell(row, 0), out _); row++)
            producers.Add(ReadRow(csv, row, columns, materialCodes, warnings));

        return new FeeTable(producers, ReadTotals(csv, header - 1, columns), warnings);
    }

    /// <summary>
    ///     The overall totals, which the file shows in the row above the column headers (its Totals row is empty).
    ///     Only each section's totals are shown, not their country splits.
    /// </summary>
    private static FeeDetail ReadTotals(BillingCsv csv, int row, Columns columns)
    {
        decimal Number(string? group, string header) => columns.Find(group, null, header) is { } c ? csv.NullableNumber(row, c) ?? 0 : 0;

        FeeWithBadDebt Section(string group, string withoutBadDebt, string badDebt, string withBadDebt) => new()
        {
            FeeWithoutBadDebt = Number(group, withoutBadDebt),
            BadDebt           = Number(null, badDebt),
            ByCountry         = ByCountryCost.Empty with { Total = Number(group, withBadDebt) }
        };

        return new FeeDetail
        {
            ProducerId                    = 0,
            SubsidiaryId                  = string.Empty,
            ProducerName                  = string.Empty,
            LADisposalCostsSection1       = Section("1 fee for la disposal costs", "1 total producer fee for la disposal costs w/o bad debt provision", "bad debt provision for 1", "1 total producer fee for la disposal costs with bad debt provision"),
            CommsCostsSection2a           = Section("2a fee for comms costs - by material", "2a total producer fee for comms costs - by material w/o bad debt provision", "bad debt provision for 2a", "2a total producer fee for comms costs - by material with bad debt provision"),
            CommsCostsSection2b           = Section("2b comms costs - uk wide", "2b total producer fee for comms costs - uk wide|w/o", "bad debt provision for 2b", "2b total producer fee for comms costs - uk wide|with"),
            CommsCostsSection2c           = Section("2c comms costs - by country", "2c total producer fee for comms costs - by country|w/o", "bad debt provision for 2c", "2c total producer fee for comms costs - by country|with"),
            SaOperatingCostsSection3      = Section("3 sa operating costs", "3 total producer fee for sa operating costs|w/o", "bad debt provision for 3", "3 total producer fee for sa operating costs|with"),
            LaDataPrepSection4            = Section("4 la data prep costs", "4 total producer fee for la data prep costs|w/o", "bad debt provision for 4", "4 total producer fee for la data prep costs|with"),
            SaSetupCostsSection5          = Section("5 one-off fee for sa set up costs", "5 total producer one-off fee for sa set up costs|w/o", "bad debt provision for 5", "5 total producer one-off fee for sa set up costs|with"),
            TotalOnePlus2A2B2CWithBadDebt = Number(null, "producer total (1+2a+2b+2c) with bad debt provision"),
            BillingInstruction            = new BillingInstruction { SuggestedBillingInstruction = string.Empty }
        };
    }

    private static FeeDetail ReadRow(BillingCsv csv, int row, Columns columns, IReadOnlyDictionary<string, string> materialCodes, List<string> warnings)
    {
        decimal? Value(int? column) => column is { } c ? csv.NullableNumber(row, c) : null;
        decimal Number(int? column) => Value(column) ?? 0;
        string? Text(int? column) => column is { } c && csv.Cell(row, c) is { Length: > 0 } text ? text : null;
        string? Raw(int? column) => column is { } c ? csv.Cell(row, c) : null;

        // A section's bad debt column has its own group heading ("Bad Debt Provision"), so it's found by its header alone.
        FeeWithBadDebt Section(string group, string withoutBadDebt, string badDebt, string withBadDebt) => new()
        {
            FeeWithoutBadDebt = Number(columns.Find(group, null, withoutBadDebt)),
            BadDebt           = Number(columns.Find(null, null, badDebt)),
            ByCountry         = ByCountry(group, null, withBadDebt, "{0} total with bad debt provision")
        };

        ByCountryCost ByCountry(string group, string? subgroup, string total, string countryHeader) => new()
        {
            England         = Number(columns.Find(group, subgroup, string.Format(countryHeader, "england"))),
            Wales           = Number(columns.Find(group, subgroup, string.Format(countryHeader, "wales"))),
            Scotland        = Number(columns.Find(group, subgroup, string.Format(countryHeader, "scotland"))),
            NorthernIreland = Number(columns.Find(group, subgroup, string.Format(countryHeader, "northern ireland"))),
            Total           = Number(columns.Find(group, subgroup, total))
        };

        RamTonnage AmberOnly(int? column) => Value(column) is { } tonnage
            ? new RamTonnage { Amber = tonnage, TotalRamTonnage = tonnage }
            : RamTonnage.Empty;

        var materialFees = new Dictionary<string, Fees>();
        foreach (var (material, code) in columns.Materials(materialCodes))
        {
            var disposal = $"{material} breakdown";
            const string section1 = "1 producer disposal fees";
            const string section2a = "2a fees for comms costs - by material";

            materialFees[code] = new Fees
            {
                DisposalFee = new DisposalFee
                {
                    PreviousInvoicedTonnage = Value(columns.Find(section1, disposal, "previous invoiced tonnage")),
                    HhTonnage               = AmberOnly(columns.Find(section1, disposal, "household tonnage")),
                    PbTonnage               = AmberOnly(columns.Find(section1, disposal, "public bin tonnage")),
                    HdcTonnage              = AmberOnly(columns.Find(section1, disposal, "household drinks containers tonnage - glass")),
                    TotalTonnage            = AmberOnly(columns.Find(section1, disposal, "total tonnage")),
                    SmcwTonnage             = Number(columns.Find(section1, disposal, "self-managed consumer waste tonnage")),
                    ActionedSmcwTonnage     = new RamTonnageGroup(),
                    NetTonnage              = new RamTonnageGroup { Total = Value(columns.Find(section1, disposal, "net tonnage")) },
                    TonnageChange           = Value(columns.Find(section1, disposal, "tonnage change")),
                    PricePerTonne           = new RamTonnageGroup { Total = Value(columns.Find(section1, disposal, "price per tonne")) },
                    Fee                     = new RamTonnageGroup { Total = Value(columns.Find(section1, disposal, "producer disposal fee w/o bad debt provision")) },
                    BadDebt                 = Number(columns.Find(section1, disposal, "bad debt provision")),
                    FeeWithBadDebtByCountry = ByCountry(section1, disposal, "producer disposal fee with bad debt provision", "{0} with bad debt provision")
                },
                CommFee = new CommsFee
                {
                    HhTonnage     = Number(columns.Find(section2a, disposal, "household tonnage")),
                    PbTonnage     = Number(columns.Find(section2a, disposal, "public bin tonnage")),
                    HdcTonnage    = Number(columns.Find(section2a, disposal, "household drinks containers tonnage - glass")),
                    TotalTonnage  = Number(columns.Find(section2a, disposal, "total tonnage")),
                    PricePerTonne = Number(columns.Find(section2a, disposal, "price per tonne")),
                    Costs = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = Number(columns.Find(section2a, disposal, "producer total cost w/o bad debt provision")),
                        BadDebt           = Number(columns.Find(section2a, disposal, "bad debt provision")),
                        ByCountry         = ByCountry(section2a, disposal, "producer total cost with bad debt provision", "{0} with bad debt provision")
                    }
                }
            };
        }

        var fee = new FeeDetail
        {
            ProducerId               = (int)csv.Number(row, columns.Require(null, null, "producer id")),
            SubsidiaryId             = Text(columns.Find(null, null, "subsidiary id")) ?? string.Empty,
            ProducerName             = Text(columns.Find(null, null, "producer / subsidiary name")) ?? string.Empty,
            TradingName              = Text(columns.Find(null, null, "trading name")),
            Level                    = Text(columns.Find(null, null, "level")),
            StatusCode               = Text(columns.Find(null, null, "registration status code")),
            JoinerDate               = Text(columns.Find(null, null, "joiner's date")),
            LeaverDate               = Text(columns.Find(null, null, "leaver's date")),
            FeesByMaterial           = materialFees,
            // The export shows a null as "-", so an empty cell stays empty.
            TonnageChangeCount       = Raw(columns.Find("1 producer disposal fees", "disposal fee summary", "tonnage change count")),
            TonnageChangeAdvice      = Raw(columns.Find("1 producer disposal fees", "disposal fee summary", "tonnage change advice")),
            LADisposalCostsSection1  = Section("1 fee for la disposal costs", "1 total producer fee for la disposal costs w/o bad debt provision", "bad debt provision for 1", "1 total producer fee for la disposal costs with bad debt provision"),
            CommsCostsSection2a      = Section("2a fee for comms costs - by material", "2a total producer fee for comms costs - by material w/o bad debt provision", "bad debt provision for 2a", "2a total producer fee for comms costs - by material with bad debt provision"),
            CommsCostsSection2b      = Section("2b comms costs - uk wide", "2b total producer fee for comms costs - uk wide|w/o", "bad debt provision for 2b", "2b total producer fee for comms costs - uk wide|with"),
            CommsCostsSection2c      = Section("2c comms costs - by country", "2c total producer fee for comms costs - by country|w/o", "bad debt provision for 2c", "2c total producer fee for comms costs - by country|with"),
            SaOperatingCostsSection3 = Section("3 sa operating costs", "3 total producer fee for sa operating costs|w/o", "bad debt provision for 3", "3 total producer fee for sa operating costs|with"),
            LaDataPrepSection4       = Section("4 la data prep costs", "4 total producer fee for la data prep costs|w/o", "bad debt provision for 4", "4 total producer fee for la data prep costs|with"),
            SaSetupCostsSection5     = Section("5 one-off fee for sa set up costs", "5 total producer one-off fee for sa set up costs|w/o", "bad debt provision for 5", "5 total producer one-off fee for sa set up costs|with"),
            TotalBillBreakdown       = Section("total producer bill breakdown", "total producer bill (1+2a+2b+2c+3+4+5) w/o bad debt provision", "bad debt provision for total producer bill", "total producer bill (1+2a+2b+2c+3+4+5) with bad debt provision"),
            ReportedTonnagePercentage = Number(columns.Find("2a fee for comms costs - by material", null, "percentage of producer tonnage vs all producers")),
            TotalOnePlus2A2B2CWithBadDebt = Number(columns.Find("total (1+2a+2b+2c) with bad debt provision", null, "producer total (1+2a+2b+2c) with bad debt provision")),
            TotalOnePlus2A2B2CWithBadDebtPercentage = Number(columns.Find("total (1+2a+2b+2c) with bad debt provision", null, "producer percentage of overall producer cost for (1+2a+2b+2c)")),
            BillingInstruction       = ReadBillingInstruction(csv, row, columns)
        };

        CheckDuplicate(fee.LADisposalCostsSection1, columns.Find("1 producer disposal fees", "disposal fee summary", "1 total producer disposal fee with bad debt provision"), "1", csv, row, warnings);
        CheckDuplicate(fee.CommsCostsSection2a, columns.Find("2a fees for comms costs - by material", "summary of fee for comms costs", "2a total producer fee for comms costs - by material with bad debt provision"), "2a", csv, row, warnings);

        return fee;
    }

    private static BillingInstruction ReadBillingInstruction(BillingCsv csv, int row, Columns columns)
    {
        const string group = "calculation of suggested billing instructions";
        decimal? Value(string header) => columns.Find(group, null, header) is { } c ? csv.NullableNumber(row, c) : null;
        string? Text(string header) => columns.Find(group, null, header) is { } c ? csv.Cell(row, c) : null;

        LiabilityDirection? Direction(string header) => Text(header) switch
        {
            CommonConstants.Positive => LiabilityDirection.Positive,
            CommonConstants.Negative => LiabilityDirection.Negative,
            _ => null
        };

        return new BillingInstruction
        {
            CurrentYearInvoiceTotalToDate             = Value("current year invoiced total to date"),
            TonnageChangeSinceLastInvoice             = Text("tonnage change since last invoice") is { Length: > 0 } t ? t : null,
            LiabilityDifference                       = Value("liability difference (calc vs prev)"),
            MaterialityLiabilityDirection             = Direction("material £ threshold breached"),
            TonnageAmountLiabilityDirection           = Direction("tonnage £ threshold breached (if tonnage changed)"),
            PercentageLiabilityDifference             = Value("% liability difference (calc vs prev)"),
            MaterialityPercentageLiabilityDirection   = Direction("material % threshold breached"),
            TonnageAmountPercentageLiabilityDirection = Direction("tonnage % threshold breached (if tonnage changed)"),
            SuggestedBillingInstruction               = Text("suggested billing instruction") ?? string.Empty,
            SuggestedInvoiceAmount                    = Value("suggested invoice amount")
        };
    }

    // The summary columns repeat the section totals; a disagreement means a column was matched to the wrong field.
    private static void CheckDuplicate(FeeWithBadDebt section, int? duplicateColumn, string name, BillingCsv csv, int row, List<string> warnings)
    {
        if (duplicateColumn is { } c && csv.NullableNumber(row, c) is { } duplicate && duplicate != section.ByCountry.Total)
            warnings.Add($"Row {row}: section {name}'s summary total doesn't match its section total");
    }

    /// <summary>The producer table's columns, by section (group row), breakdown (subgroup row) and header.</summary>
    private sealed class Columns
    {
        private readonly List<(string Group, string Subgroup, string Header, int Column)> columns = [];

        public Columns(BillingCsv csv, int headerRow)
        {
            var group = "";
            var subgroup = "";
            for (var c = 0; c < csv.Rows[headerRow].Length; c++)
            {
                if (csv.Cell(headerRow - 2, c) is { Length: > 0 } g) { group = Normalise(g); subgroup = ""; }
                if (csv.Cell(headerRow - 1, c) is { Length: > 0 } s && !s.StartsWith('£')) subgroup = Normalise(s);
                columns.Add((group, subgroup, ProducerSectionsParser.Normalise(csv.Cell(headerRow, c)), c));
            }
        }

        public IEnumerable<(string Material, string Code)> Materials(IReadOnlyDictionary<string, string> materialCodes) =>
            columns.Where(c => c.Group.StartsWith("1 producer disposal fees") && c.Subgroup.EndsWith(" breakdown"))
                .Select(c => c.Subgroup[..^" breakdown".Length])
                .Distinct()
                .Select(m => (m, materialCodes[m]));

        /// <summary>
        ///     The column under a section and breakdown whose header starts with <paramref name="header" />. A "|with" or
        ///     "|w/o" suffix also requires the header to contain "with bad debt" or "w/o bad debt".
        /// </summary>
        public int? Find(string? group, string? subgroup, string header)
        {
            var parts = header.Split('|');
            var matches = columns.Where(c =>
                (group is null || c.Group.StartsWith(group)) &&
                (subgroup is null || c.Subgroup == subgroup) &&
                c.Header.StartsWith(parts[0]) &&
                (parts.Length == 1 || c.Header.Contains(parts[1] == "with" ? "with bad debt" : "w/o bad debt")))
                .ToList();

            return matches.Count == 0 ? null : matches[0].Column;
        }

        public int Require(string? group, string? subgroup, string header) =>
            Find(group, subgroup, header) ?? throw new FormatException($"No '{header}' column");

        private static string Normalise(string text) => ProducerSectionsParser.Normalise(text);
    }
}
