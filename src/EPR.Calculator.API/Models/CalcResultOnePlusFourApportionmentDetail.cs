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
        public string TotalDisposalTotal { get; set; }
        public int OrderId { get; set; }
    }
}
