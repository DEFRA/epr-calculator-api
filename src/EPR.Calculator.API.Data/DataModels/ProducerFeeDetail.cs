using System.ComponentModel.DataAnnotations.Schema;

namespace EPR.Calculator.API.Data.DataModels;

public record ProducerFeeDetail
{
    public required int ProducerId { get; set; }

    public required string SubsidiaryId { get; set; }

    public required string ProducerName { get; set; }

    public string? TradingName { get; set; }

    public string? Level { get; set; }

    public string? StatusCode { get; set; }

    public string? JoinerDate { get; set; }

    public string? LeaverDate { get; set; }

    public FeeWithBadDebt LADisposalCostsSection1 { get; set; } = FeeWithBadDebt.Empty;

    public FeeWithBadDebt CommsCostsSection2a { get; set; } = FeeWithBadDebt.Empty;

    public FeeWithBadDebt CommsCostsSection2b { get; set; } = FeeWithBadDebt.Empty;

    public FeeWithBadDebt CommsCostsSection2c { get; set; } = FeeWithBadDebt.Empty;

    public FeeWithBadDebt SaOperatingCostsSection3 { get; set; } = FeeWithBadDebt.Empty;

    public FeeWithBadDebt LaDataPrepSection4 { get; set; } = FeeWithBadDebt.Empty;

    public FeeWithBadDebt SaSetupCostsSection5 { get; set; } = FeeWithBadDebt.Empty;

    public FeeWithBadDebt TotalBillBreakdown { get; set; } = FeeWithBadDebt.Empty;

    public decimal ReportedTonnagePercentage { get; set; }

    public decimal TotalOnePlus2A2B2CWithBadDebt() =>
        LADisposalCostsSection1.ByCountry.Total
        + CommsCostsSection2a.ByCountry.Total
        + CommsCostsSection2b.ByCountry.Total
        + CommsCostsSection2c.ByCountry.Total;
    

    public decimal TotalOnePlus2A2B2CWithBadDebtPercentage { get; set; }

    internal ICollection<MaterialFee> MaterialFees { get; set; } = [];

    [NotMapped]
    public IReadOnlyDictionary<string, MaterialFee> FeesByMaterial
    {
        get => MaterialFees.ToDictionary(s => s.MaterialCode);
        set
        {
            MaterialFees = [..value.Values];
            _disposalFeesByMaterial = null;
            _commsFeesByMaterial = null;
        }
    }

    private IReadOnlyDictionary<string, DisposalFee>? _disposalFeesByMaterial;

    [NotMapped]
    public IReadOnlyDictionary<string, DisposalFee> DisposalFeesByMaterial =>
        _disposalFeesByMaterial ??= MaterialFees.ToDictionary(s => s.MaterialCode, s => s.DisposalFee);

    private IReadOnlyDictionary<string, CommsFee>? _commsFeesByMaterial;

    [NotMapped]
    public IReadOnlyDictionary<string, CommsFee> CommsFeesByMaterial =>
        _commsFeesByMaterial ??= MaterialFees.ToDictionary(s => s.MaterialCode, s => s.CommFee);

    public string? TonnageChangeCount { get; set; }

    public string? TonnageChangeAdvice { get; set; }

    public BillingInstruction? BillingInstruction { get; set; }

}
