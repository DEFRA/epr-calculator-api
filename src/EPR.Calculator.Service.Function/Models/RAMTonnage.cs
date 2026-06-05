using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Models
{
    public record RAMTonnage
    {
        public decimal RedTonnage { get; init; }
        public decimal AmberTonnage { get; init; }
        public decimal GreenTonnage { get; init; }
        public decimal RedMedicalTonnage { get; init; }
        public decimal AmberMedicalTonnage { get; init; }
        public decimal GreenMedicalTonnage { get; init; }

        public decimal TotalRamTonnage()
        {
            return RedTonnage + RedMedicalTonnage + AmberTonnage + AmberMedicalTonnage + GreenTonnage + GreenMedicalTonnage;
        }

        public static RAMTonnage ToRAMTonnage(string packagingType, List<ProducerReportedMaterial> reportedMaterials) {
            return new RAMTonnage {
                RedTonnage = GetReportedTonnage(reportedMaterials, packagingType, t => t.PackagingTonnageRed),
                RedMedicalTonnage = GetReportedTonnage(reportedMaterials, packagingType, t => t.PackagingTonnageRedMedical),
                AmberTonnage = GetReportedTonnage(reportedMaterials, packagingType, t => t.PackagingTonnageAmber),
                AmberMedicalTonnage = GetReportedTonnage(reportedMaterials, packagingType, t => t.PackagingTonnageAmberMedical),
                GreenTonnage = GetReportedTonnage(reportedMaterials, packagingType, t => t.PackagingTonnageGreen),
                GreenMedicalTonnage = GetReportedTonnage(reportedMaterials, packagingType, t => t.PackagingTonnageGreenMedical),
            };
        }

        public static decimal GetReportedTonnage(List<ProducerReportedMaterial> reportedMaterials, string packagingType, Func<ProducerReportedMaterial, decimal?> tonnageFunc) {
            return reportedMaterials.Where(p => p.PackagingType == packagingType).Sum(t => tonnageFunc(t) ?? 0);
        }
    }
}