namespace EPR.Calculator.API.Models
{
    public class CalcResultLaDisposalCostDataDetail
    {
        public string Name { get; set; }
        public string Material { get; set; }
        public string England { get; set; }
        public string Wales { get; set; }
        public string Scotland { get; set; }
        public string NorthernIreland { get; set; }
        public string Total { get; set; }

        public string ProducerReportedHouseholdPackagingWasteTonnage { get; set; }

        public string LateReportingTonnage { get; set; }

        public string ProducerReportedHouseholdTonnagePlusLateReportingTonnage { get; set; }

        public string DisposalCostPricePerTonne { get; set; }
    }
}
