namespace EPR.Calculator.API.Models
{
    public class CalcResultParameterCommunicationCostDetail2
    {
        public string Name { get; set; }
        public string England { get; set; }
        public string Wales { get; set; }
        public string Scotland { get; set; }
        public string NorthernIreland { get; set; }
        public string Total { get; set; }
        public int OrderId { get; set; }
        public string ProducerReportedHouseholdPackagingWasteTonnage { get; set; }
        public string LateReportingTonnage { get; set; }
        public string ProducerReportedHouseholdTonnagePlusLateReportingTonnage { get; set; }
        public string CommsCostByMaterialPricePerTonne { get; set; }

    }
}
