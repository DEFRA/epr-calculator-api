namespace EPR.Calculator.API.Models
{
    public record CalcResultOnePlusFourApportionmentDetail
    {
        public required string Name { get; set; }
        public required string EnglandDisposalTotal { get; set; }
        public required string WalesDisposalTotal { get; set; }
        public required string ScotlandDisposalTotal { get; set; }
        public required string NorthernIrelandDisposalTotal { get; set; }
        public string Total { get; set; } = string.Empty;
        public decimal EnglandTotal { get; set; }
        public decimal WalesTotal { get; set; }
        public decimal ScotlandTotal { get; set; }
        public decimal NorthernIrelandTotal { get; set; }
        public decimal AllTotal { get; set; }
        public int OrderId { get; set; }
    }
}
