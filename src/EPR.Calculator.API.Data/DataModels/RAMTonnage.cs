namespace EPR.Calculator.API.Data.DataModels
{
    public record RamTonnage
    {
        public decimal Red { get; init; }
        public decimal Amber { get; init; }
        public decimal Green { get; init; }
        public decimal RedMedical { get; init; }
        public decimal AmberMedical { get; init; }
        public decimal GreenMedical { get; init; }

        public decimal TotalRamTonnage() =>
            Red + RedMedical + Amber + AmberMedical + Green + GreenMedical;

        public static RamTonnage ToRamTonnage(List<ProducerReportedMaterial> reportedMaterials, string packagingType) =>
            new RamTonnage {
                Red          = GetReportedTonnage(reportedMaterials, packagingType, t => t.PackagingTonnageRed),
                RedMedical   = GetReportedTonnage(reportedMaterials, packagingType, t => t.PackagingTonnageRedMedical),
                Amber        = GetReportedTonnage(reportedMaterials, packagingType, t => t.PackagingTonnageAmber),
                AmberMedical = GetReportedTonnage(reportedMaterials, packagingType, t => t.PackagingTonnageAmberMedical),
                Green        = GetReportedTonnage(reportedMaterials, packagingType, t => t.PackagingTonnageGreen),
                GreenMedical = GetReportedTonnage(reportedMaterials, packagingType, t => t.PackagingTonnageGreenMedical)
            };

        public static decimal GetReportedTonnage(List<ProducerReportedMaterial> reportedMaterials, string packagingType, Func<ProducerReportedMaterial, decimal?> tonnageFunc) =>
            reportedMaterials.Where(p => p.PackagingType == packagingType).Sum(t => tonnageFunc(t) ?? 0);

        public static RamTonnage Empty => new RamTonnage();
    }
}
