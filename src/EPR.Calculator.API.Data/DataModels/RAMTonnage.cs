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

        // Read back from storage so the files show the run's own totals; until they're stored, they're the sums.
        public decimal TotalRamTonnage
        {
            get => storedTotalRamTonnage ?? Red + RedMedical + Amber + AmberMedical + Green + GreenMedical;
            init => storedTotalRamTonnage = value;
        }

        public decimal RedAndRedMedical
        {
            get => storedRedAndRedMedical ?? Red + RedMedical;
            init => storedRedAndRedMedical = value;
        }

        public decimal AmberAndAmberMedical
        {
            get => storedAmberAndAmberMedical ?? Amber + AmberMedical;
            init => storedAmberAndAmberMedical = value;
        }

        public decimal GreenAndGreenMedical
        {
            get => storedGreenAndGreenMedical ?? Green + GreenMedical;
            init => storedGreenAndGreenMedical = value;
        }

        private readonly decimal? storedTotalRamTonnage;
        private readonly decimal? storedRedAndRedMedical;
        private readonly decimal? storedAmberAndAmberMedical;
        private readonly decimal? storedGreenAndGreenMedical;

        public virtual bool Equals(RamTonnage? other) =>
            other is not null && EqualityContract == other.EqualityContract
            && Red == other.Red && Amber == other.Amber && Green == other.Green
            && RedMedical == other.RedMedical && AmberMedical == other.AmberMedical && GreenMedical == other.GreenMedical
            && TotalRamTonnage == other.TotalRamTonnage && RedAndRedMedical == other.RedAndRedMedical
            && AmberAndAmberMedical == other.AmberAndAmberMedical && GreenAndGreenMedical == other.GreenAndGreenMedical;

        public override int GetHashCode() => HashCode.Combine(Red, Amber, Green, RedMedical, AmberMedical, GreenMedical, TotalRamTonnage);

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
