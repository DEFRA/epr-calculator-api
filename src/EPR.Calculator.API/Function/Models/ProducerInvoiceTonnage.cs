namespace EPR.Calculator.Service.Function.Models
{
    public record ProducerInvoiceTonnage
    {
        public int RunId {  get; init; }
        public int ProducerId { get; init; }
        public int MaterialId { get; init; }
        public decimal? NetTonnage { get; init; }
    }
}
