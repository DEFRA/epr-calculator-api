namespace EPR.Calculator.API.Data.DataModels
{  
    public class TransformPartial
    {
        public int Id { get; set; }
        public required int CalculatorRunId { get; set; }
        public required int ProducerId { get; set; }
        public string? SubsidiaryId { get; set; }
        public string? ProducerName { get; set; }
        public string? TradingName { get; set; }
        public required string Level { get; set; }
        public required int SubmissionYear { get; set; }
        public required int DaysInSubmissionYear { get; set; }
        public string? JoiningDate { get; set; }
        public int? DaysObligated { get; set; }
        public required decimal ObligatedFactor { get; set; }
        public required string MaterialCode { get; set; }
        public required decimal HouseholdTonnage { get; set; }
        public decimal? HouseholdTonnageRed { get; set; }
        public decimal? HouseholdTonnageAmber { get; set; }
        public decimal? HouseholdTonnageGreen { get; set; }
        public decimal? HouseholdTonnageRedMedical { get; set; }
        public decimal? HouseholdTonnageAmberMedical { get; set; }
        public decimal? HouseholdTonnageGreenMedical { get; set; }
        public required decimal PublicBinTonnage { get; set; }
        public decimal? PublicBinTonnageRed { get; set; }
        public decimal? PublicBinTonnageAmber { get; set; }
        public decimal? PublicBinTonnageGreen { get; set; }
        public decimal? PublicBinTonnageRedMedical { get; set; }
        public decimal? PublicBinTonnageAmberMedical { get; set; }
        public decimal? PublicBinTonnageGreenMedical { get; set; }
        public decimal? HDCTonnage { get; set; }
        public decimal? HDCTonnageRed { get; set; }
        public decimal? HDCTonnageAmber { get; set; }
        public decimal? HDCTonnageGreen { get; set; }
        public decimal? HDCTonnageRedMedical { get; set; }
        public decimal? HDCTonnageAmberMedical { get; set; }
        public decimal? HDCTonnageGreenMedical { get; set; }
        public required decimal SMCWTonnage { get; set; }
    }
}