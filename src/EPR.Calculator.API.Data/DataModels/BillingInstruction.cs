namespace EPR.Calculator.API.Data.DataModels;

public class BillingInstruction
{
    public decimal? CurrentYearInvoiceTotalToDate { get; set; }

    public string? TonnageChangeSinceLastInvoice { get; set; }

    public decimal? LiabilityDifference { get; set; }

    public LiabilityDirection? MaterialityLiabilityDirection { get; set; } 

    public LiabilityDirection? TonnageAmountLiabilityDirection { get; set; }

    public decimal? PercentageLiabilityDifference { get; set; }
    
    public LiabilityDirection? MaterialityPercentageLiabilityDirection { get; set; }

    public LiabilityDirection? TonnageAmountPercentageLiabilityDirection { get; set; }

    public required string SuggestedBillingInstruction { get; set; } = string.Empty;

    public decimal? SuggestedInvoiceAmount { get; set; }
}