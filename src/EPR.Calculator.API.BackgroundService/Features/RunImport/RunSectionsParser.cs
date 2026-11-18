using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;

namespace EPR.Calculator.API.BackgroundService.Features.RunImport;

/// <summary>
///     Reads the run header and the cost and parameter sections, which every layout lays out the same way apart from their labels.
/// </summary>
public static class RunSectionsParser
{
    public static readonly IReadOnlyDictionary<string, string> MaterialCodes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Aluminium"]       = "AL",
        ["Fibre composite"] = "FC",
        ["Glass"]           = "GL",
        ["Paper or card"]   = "PC",
        ["Plastic"]         = "PL",
        ["Steel"]           = "ST",
        ["Wood"]            = "WD",
        ["Other materials"] = "OT"
    };

    public record RunHeader
    {
        public required int RunId { get; init; }
        public required string RunName { get; init; }
        public required DateTime RunDate { get; init; }
        public required string RunBy { get; init; }
        public required RelativeYear RelativeYear { get; init; }
        public DateTime? CutOffDate { get; init; }
        public DateTime? OrgPomDataLoadedAt { get; init; }
        public required UploadedFile LapcapFile { get; init; }
        public required UploadedFile ParametersFile { get; init; }
    }

    public record UploadedFile(string FileName, DateTime UploadedAt, string UploadedBy);

    public static RunHeader ReadHeader(BillingCsv csv)
    {
        var lapcap = csv.FindRow("LAPCAP File");
        var parameters = csv.FindRow("Parameters File");
        var cutOff = csv.TryFindRow("Cut-off Date");

        return new RunHeader
        {
            RunName            = csv.Cell(csv.FindRow("Run Name"), 1),
            RunId              = (int)csv.Number(csv.FindRow("Run Id"), 1),
            RunDate            = csv.Date(csv.FindRow("Run Date"), 1)!.Value,
            RunBy              = csv.Cell(csv.FindRow("Run by"), 1),
            RelativeYear       = new RelativeYear(int.Parse(csv.Cell(csv.FindRow("Financial Year"), 1)[..4])),
            CutOffDate         = cutOff is { } row ? csv.Date(row, 1) : null,
            OrgPomDataLoadedAt = csv.Date(csv.FindRow("RPD File - ORG"), 1),
            LapcapFile         = new UploadedFile(csv.Cell(lapcap, 1), csv.Date(lapcap, 3)!.Value, csv.Cell(lapcap, 5)),
            ParametersFile     = new UploadedFile(csv.Cell(parameters, 1), csv.Date(parameters, 3)!.Value, csv.Cell(parameters, 5))
        };
    }

    public static CalcResultLapcapData ReadLapcapData(BillingCsv csv)
    {
        var section = csv.FindRow("LAPCAP Data");
        var total = csv.FindRow("Total", section);

        return new CalcResultLapcapData
        {
            ByMaterial           = MaterialRows(csv, section + 2, total).ToDictionary(m => m.Code, m => Cost(csv, m.Row, 1)),
            Total                = Cost(csv, total, 1),
            CountryApportionment = Apportionment(csv, csv.FindRow("1 Country Apportionment", total), 1)
        };
    }

    public static CalcResultLateReportingTonnage ReadLateReportingTonnage(BillingCsv csv)
    {
        var section = csv.FindRow("Parameters - Late Reporting Tonnages");
        var total = csv.FindRow("Total", section);

        // The RAM columns have been in more than one order, so they're found by heading. The earliest layout only shows
        // each material's total, which a calculated run of that year holds as amber.
        int? Column(string colour) => Enumerable.Range(1, csv.Rows[section + 1].Length - 1)
            .Cast<int?>()
            .FirstOrDefault(c => csv.Cell(section + 1, c!.Value).StartsWith(colour, StringComparison.OrdinalIgnoreCase));
        var (red, amber, green) = (Column("Red"), Column("Amber"), Column("Green"));
        var totalColumn = Column("Total") ?? 1;

        CalcResultLateReportingTonnageDetail Detail(int row) => red is null
            ? new() { Red = 0, Amber = csv.Number(row, totalColumn), Green = 0, Total = csv.Number(row, totalColumn) }
            : new() { Red = csv.Number(row, red.Value), Amber = csv.Number(row, amber!.Value), Green = csv.Number(row, green!.Value), Total = csv.Number(row, totalColumn) };

        return new CalcResultLateReportingTonnage
        {
            ByMaterial = MaterialRows(csv, section + 2, total).ToDictionary(m => m.Code, m => Detail(m.Row)),
            Total      = Detail(total)
        };
    }

    public static CalcResultParameterOtherCost ReadParameterOtherCost(BillingCsv csv, RunHeader header)
    {
        var section = csv.FindRow("Parameters - Other");
        var materiality = csv.FindRow("7 Materiality", section);
        var tonnageChange = csv.FindRow("8 Tonnage Change", section);

        Materiality Threshold(int row) => new() { Amount = csv.Number(row, 1), Percentage = csv.Number(row, 2) };

        return new CalcResultParameterOtherCost
        {
            SaOperatingCost       = Cost(csv, csv.FindRow("3 SA Operating Costs", section), 1),
            LaDataPrepCharge      = Cost(csv, csv.FindRow("4 LA Data Prep Charge", section), 1),
            CountryApportionment  = Apportionment(csv, csv.FindRow("4 Country Apportionment", section), 1),
            SchemeSetupCost       = Cost(csv, csv.FindRow("5 Scheme", section), 1),
            BadDebtValue          = csv.Number(csv.FindRow("6 Bad Debt Provision", section), 1),
            MaterialityIncrease   = Threshold(csv.FindRow("Increase", materiality)),
            MaterialityDecrease   = Threshold(csv.FindRow("Decrease", materiality)),
            TonnageChangeIncrease = Threshold(csv.FindRow("Increase", tonnageChange)),
            TonnageChangeDecrease = Threshold(csv.FindRow("Decrease", tonnageChange)),
            CutOffDate            = header.CutOffDate
        };
    }

    public static CalcResultOnePlusFourApportionment ReadOnePlusFourApportionment(BillingCsv csv)
    {
        var section = csv.FindRow("1 + 4 Apportionment");

        return new CalcResultOnePlusFourApportionment
        {
            LaDisposalCost           = Cost(csv, csv.FindRow("1 Fee for LA Disposal Costs", section), 1),
            LADataPrepCharge         = Cost(csv, csv.FindRow("4 LA Data Prep Charge", section), 1),
            TotalOnePlusFour         = Cost(csv, csv.FindRow("Total of 1 + 4", section), 1),
            OnePlusFourApportionment = Apportionment(csv, csv.FindRow("1 + 4 Apportionment", section + 1), 1)
        };
    }

    public static CalcResultCommsCost ReadCommsCost(BillingCsv csv)
    {
        var section = csv.FindRow("Parameters - Comms Costs");
        var byMaterial = csv.FindRow("2a Comms Costs - by Material", section);
        var total = csv.FindRow("Total", byMaterial);

        CalcResultCommsCostCommsCostByMaterial Material(int row) => new()
        {
            Cost                             = Cost(csv, row, 1),
            TotalCost                        = csv.Number(row, 5),
            HouseholdPackagingWasteTonnage   = csv.Number(row, 6),
            PublicBinTonnage                 = csv.Number(row, 7),
            HouseholdDrinksContainersTonnage = csv.NullableNumber(row, 8) ?? 0,
            LateReportingTonnage             = csv.Number(row, 9),
            TotalTonnage                     = csv.Number(row, 10),
            PricePerTonne                    = csv.NullableNumber(row, 11) ?? 0
        };

        return new CalcResultCommsCost
        {
            OnePlusFourApportionment = Apportionment(csv, csv.FindRow("1 + 4 Apportionment", section), 1),
            ByMaterial               = MaterialRows(csv, byMaterial + 1, total).ToDictionary(m => m.Code, m => Material(m.Row)),
            Total                    = Material(total),
            CommsCostUkWide          = Cost(csv, csv.FindRow("2b Comms Costs - UK wide", total), 1),
            CommsCostByCountry       = Cost(csv, csv.FindRow("2c Comms Costs - by Country", total), 1)
        };
    }

    public static CalcResultLaDisposalCostData ReadLaDisposalCostData(BillingCsv csv)
    {
        var section = csv.FindRow("LA Disposal Cost Data");
        var header = section + 1;
        var total = csv.FindRow("Total", section);

        // Layouts with modulation add the actioned self-managed consumer waste tonnage before the total tonnage.
        var hasSmcw = csv.Cell(header, 10).Contains("Self", StringComparison.OrdinalIgnoreCase);
        var offset = hasSmcw ? 1 : 0;

        CalcResultLaDisposalCostDataDetail Detail(int row) => new()
        {
            Cost                                    = Cost(csv, row, 1),
            HouseholdPackagingWasteTonnage          = csv.Number(row, 6),
            PublicBinTonnage                        = csv.Number(row, 7),
            HouseholdDrinkContainersTonnage         = csv.NullableNumber(row, 8) ?? 0,
            LateReportingTonnage                    = csv.Number(row, 9),
            ActionedSelfManagedConsumerWasteTonnage = hasSmcw ? csv.NullableNumber(row, 10) : null,
            TotalTonnage                            = csv.Number(row, 10 + offset),
            DisposalCostPricePerTonne               = csv.NullableNumber(row, 11 + offset)
        };

        return new CalcResultLaDisposalCostData
        {
            ByMaterial = MaterialRows(csv, header + 1, total).ToDictionary(m => m.Code, m => Detail(m.Row)),
            Total      = Detail(total)
        };
    }

    private static IEnumerable<(string Code, int Row)> MaterialRows(BillingCsv csv, int from, int to)
    {
        for (var row = from; row < to; row++)
            yield return (MaterialCodes[csv.Cell(row, 0)], row);
    }

    private static ByCountryCost Cost(BillingCsv csv, int row, int column) => new()
    {
        England         = csv.Number(row, column),
        Wales           = csv.Number(row, column + 1),
        Scotland        = csv.Number(row, column + 2),
        NorthernIreland = csv.Number(row, column + 3),
        Total           = csv.Number(row, column + 4)
    };

    private static ByCountryApportionment Apportionment(BillingCsv csv, int row, int column) => new()
    {
        England         = csv.Number(row, column),
        Wales           = csv.Number(row, column + 1),
        Scotland        = csv.Number(row, column + 2),
        NorthernIreland = csv.Number(row, column + 3),
        Total           = csv.Number(row, column + 4)
    };
}
