namespace EPR.Calculator.API.Models
{
    public class CalcResultLapcapDataDetails
    {
        public string Key { get; set; }

        public string EnglandDisposalCost { get; set; }

        public string WalesDisposalCost { get; set; }

        public string ScotlandDisposalCost { get; set; }

        public string NorthernIrelandDisposalCost { get; set; }

        public string TotalDisposalCost { get; set; }

        public bool isHeader { get; set; } = false;
    }
}
