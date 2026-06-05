namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultCancelledProducersResponse
    {
        public string? TitleHeader { get; set; }
        public IEnumerable<CalcResultCancelledProducersDto> CancelledProducers { get; set; } = [];
    }

    public class CalcResultCancelledProducersDto
    {
        public string? ProducerId_Header { get; set; }
        public string? ProducerName_Header { get; set; }
        public string? TradingName_Header { get; set; }
        public int ProducerId { get; set; }
        public string? SubsidiaryIdValue { get; set; }
        public string? ProducerOrSubsidiaryNameValue { get; set; }
        public string? TradingNameValue { get; set; }
        public LastTonnage? LastTonnage { get; set; }
        public LatestInvoice? LatestInvoice { get; set; }
    }

    public class LastTonnage
    {
        public string? LastTonnage_Header { get; set; }
        public string? Aluminium_Header { get; set; }
        public string? FibreComposite_Header { get; set; }
        public string? Glass_Header { get; set; }
        public string? PaperOrCard_Header { get; set; }
        public string? Plastic_Header { get; set; }
        public string? Steel_Header { get; set; }
        public string? Wood_Header { get; set; }
        public string? OtherMaterials_Header { get; set; }

        public decimal? AluminiumValue { get; set; }
        public decimal? FibreCompositeValue { get; set; }
        public decimal? GlassValue { get; set; }
        public decimal? PaperOrCardValue { get; set; }
        public decimal? PlasticValue { get; set; }
        public decimal? SteelValue { get; set; }
        public decimal? WoodValue { get; set; }
        public decimal? OtherMaterialsValue { get; set; }
    }

    public class LatestInvoice
    {
        public string? LatestInvoice_Header { get; set; }
        public string? CurrentYearInvoicedTotalToDate_Header { get; set; }
        public string? RunNumber_Header { get; set; }
        public string? RunName_Header { get; set; }
        public string? BillingInstructionId_Header { get; set; }

        public decimal? CurrentYearInvoicedTotalToDateValue { get; set; }
        public string? RunNumberValue { get; set; }
        public string? RunNameValue { get; set; }
        public string? BillingInstructionIdValue { get; set; }
    }
}