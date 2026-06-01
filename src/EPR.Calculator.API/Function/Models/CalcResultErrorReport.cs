namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultErrorReport
    {
        public int Id { get; set; }

        public int ProducerId { get; set; }

        public required string SubsidiaryId { get; set; }

        public required string ProducerName { get; set; } = string.Empty;

        public required string TradingName { get; set; } = string.Empty;

        public required string LeaverCode { get; set; } = string.Empty;

        public required string ErrorCodeText { get; set; } = string.Empty;
    }
}