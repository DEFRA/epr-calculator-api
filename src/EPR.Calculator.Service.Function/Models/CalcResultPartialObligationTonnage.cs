using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Utils;

using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultPartialObligationTonnage
    {
        public required decimal ObligatedFactor { get; set; }
        public required decimal HouseholdTonnage { get; set; }
        public RamTonnage? HouseholdRAMTonnage { get; set; }
        public required decimal PublicBinTonnage { get; set; }
        public RamTonnage? PublicBinRAMTonnage { get; set; }
        public decimal? HouseholdDrinksContainersTonnage { get; set; }
        public RamTonnage? HouseholdDrinksContainersRAMTonnage { get; set; }
        public required decimal SelfManagedConsumerWasteTonnage { get; set; }
        public decimal PartialHouseholdTonnage()
        {
            var partialRam = PartialHouseholdRAMTonnage();
            return partialRam != null ? partialRam.TotalRamTonnage() : MathUtils.RoundAwayFromZero(HouseholdTonnage * ObligatedFactor, 3);
        }
        public RamTonnage? PartialHouseholdRAMTonnage()
        {
            return HouseholdRAMTonnage != null ? ToPartialRam(HouseholdRAMTonnage, ObligatedFactor) : null;
        }
        public decimal PartialPublicBinTonnage()
        {
            var partialRam = PartialPublicBinRAMTonnage();
            return partialRam != null ? partialRam.TotalRamTonnage() : MathUtils.RoundAwayFromZero(PublicBinTonnage * ObligatedFactor, 3);
        }
        public RamTonnage? PartialPublicBinRAMTonnage()
        {
            return PublicBinRAMTonnage != null ? ToPartialRam(PublicBinRAMTonnage, ObligatedFactor) : null;
        }
        public decimal? PartialHouseholdDrinksContainersTonnage()
        {
            var partialRam = PartialHouseholdDrinksContainersRAMTonnage();
            if (partialRam != null)
            {
                return partialRam.TotalRamTonnage();
            }
            return HouseholdDrinksContainersTonnage != null ? MathUtils.RoundAwayFromZero(HouseholdDrinksContainersTonnage.Value * ObligatedFactor, 3) : null;
        }
        public RamTonnage? PartialHouseholdDrinksContainersRAMTonnage()
        {
            return HouseholdDrinksContainersRAMTonnage != null ? ToPartialRam(HouseholdDrinksContainersRAMTonnage, ObligatedFactor) : null;
        }
        public decimal PartialSelfManagedConsumerWasteTonnage()
        {
            return MathUtils.RoundAwayFromZero(SelfManagedConsumerWasteTonnage * ObligatedFactor, 3);
        }
        public decimal TotalTonnage()
        {
            return HouseholdTonnage + PublicBinTonnage + (HouseholdDrinksContainersTonnage ?? 0);
        }
        public decimal PartialTotalTonnage()
        {
            return PartialHouseholdTonnage() + PartialPublicBinTonnage() + (PartialHouseholdDrinksContainersTonnage() ?? 0);
        }

        private RamTonnage ToPartialRam(RamTonnage ram, decimal partialAmount)
        {
            return new RamTonnage
            {
                Red        = MathUtils.RoundAwayFromZero(ram.Red * partialAmount, 3),
                Amber      = MathUtils.RoundAwayFromZero(ram.Amber * partialAmount, 3),
                Green      = MathUtils.RoundAwayFromZero(ram.Green * partialAmount, 3),
                RedMedical = MathUtils.RoundAwayFromZero(ram.RedMedical * partialAmount, 3),
                AmberMedical = MathUtils.RoundAwayFromZero(ram.AmberMedical * partialAmount, 3),
                GreenMedical = MathUtils.RoundAwayFromZero(ram.GreenMedical * partialAmount, 3)
            };
        }
    }
}

