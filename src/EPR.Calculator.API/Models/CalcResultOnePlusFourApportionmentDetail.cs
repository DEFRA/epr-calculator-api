namespace EPR.Calculator.API.Models
{
    public class CalcResultOnePlusFourApportionmentDetail
    {
        public string Name { get; set; }
        public string Total { get; set; }
        public string EnglandDisposalTotal { get; set; }
        public string WalesDisposalTotal { get; set; }
        public string ScotlandDisposalTotal { get; set; }
        public string NorthernIrelandDisposalTotal { get; set; }
        public decimal EnglandTotal { get; set; }
        public decimal WalesTotal { get; set; }
        public decimal ScotlandTotal { get; set; }
        public decimal NorthernIrelandTotal { get; set; }
        public decimal AllTotal { get; set; }
        public int OrderId { get; set; }
    }
}
