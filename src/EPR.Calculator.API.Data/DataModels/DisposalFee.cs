namespace EPR.Calculator.API.Data.DataModels;

public record DisposalFee
{
    public required RamTonnage HhTonnage { get; set; }

    public required RamTonnage PbTonnage { get; set; }

    public required RamTonnage HdcTonnage { get; set; }

    public required RamTonnage TotalTonnage { get; set; }

    public decimal BadDebt { get; set; }

    public required ByCountryCost FeeWithBadDebtByCountry { get; set; }

    public decimal SmcwTonnage { get; set; }

    public required RamTonnageGroup ActionedSmcwTonnage { get; set; }

    public decimal? ResidualSmcwTonnage { get; set; }

    public required RamTonnageGroup NetTonnage { get; set; }

    public required RamTonnageGroup PricePerTonne { get; set; }

    public required RamTonnageGroup Fee { get; set; }

    public decimal? PreviousInvoicedTonnage { get; set; }

    public decimal? TonnageChange { get; set; }

    public static DisposalFee Empty => new DisposalFee { 
        HhTonnage = RamTonnage.Empty,
        PbTonnage = RamTonnage.Empty,
        HdcTonnage = RamTonnage.Empty,
        TotalTonnage = RamTonnage.Empty,
        FeeWithBadDebtByCountry = ByCountryCost.Empty,
        ActionedSmcwTonnage = RamTonnageGroup.Empty,
        NetTonnage = RamTonnageGroup.Empty,
        PricePerTonne = RamTonnageGroup.Empty,
        Fee = RamTonnageGroup.Empty
    };
}
