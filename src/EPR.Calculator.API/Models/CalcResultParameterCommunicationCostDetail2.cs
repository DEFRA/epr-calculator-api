namespace EPR.Calculator.API.Models
{
    public class CalcResultParameterCommunicationCostDetail2
    {
        public required string Name { get; set; }
        public required string England { get; set; }
        public required string Wales { get; set; }
        public required string Scotland { get; set; }
        public required string NorthernIreland { get; set; }
        public required string Total { get; set; }
        public int OrderId { get; set; }
        public required string ProducerReportedHouseholdPackagingWasteTonnage { get; set; }
        public required string LateReportingTonnage { get; set; }
        public required string ProducerReportedHouseholdTonnagePlusLateReportingTonnage { get; set; }
        public required string CommsCostByMaterialPricePerTonne { get; set; }

    }
}
