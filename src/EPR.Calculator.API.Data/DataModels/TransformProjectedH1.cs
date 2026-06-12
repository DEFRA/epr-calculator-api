namespace EPR.Calculator.API.Data.DataModels;

public class TransformProjectedH1
{
    public int Id { get; set; }
    public required int CalculatorRunId { get; set; }
    public required int ProducerId { get; set; }
    public string? SubsidiaryId { get; set; }
    public required string MaterialCode { get; set; }
    public required string SubmissionPeriodCode { get; set; }
    public required string Level { get; set; }
    public required decimal HouseholdTonnage { get; set; }
    public required decimal HouseholdTonnageRed { get; set; }
    public required decimal HouseholdTonnageAmber { get; set; }
    public required decimal HouseholdTonnageGreen { get; set; }
    public required decimal HouseholdTonnageRedMedical { get; set; }
    public required decimal HouseholdTonnageAmberMedical { get; set; }
    public required decimal HouseholdTonnageGreenMedical { get; set; }
    public required decimal PublicBinTonnage { get; set; }
    public required decimal PublicBinTonnageRed { get; set; }
    public required decimal PublicBinTonnageAmber { get; set; }
    public required decimal PublicBinTonnageGreen { get; set; }
    public required decimal PublicBinTonnageRedMedical { get; set; }
    public required decimal PublicBinTonnageAmberMedical { get; set; }
    public required decimal PublicBinTonnageGreenMedical { get; set; }
    public decimal? HDCTonnage { get; set; }
    public decimal? HDCTonnageRed { get; set; }
    public decimal? HDCTonnageAmber { get; set; }
    public decimal? HDCTonnageGreen { get; set; }
    public decimal? HDCTonnageRedMedical { get; set; }
    public decimal? HDCTonnageAmberMedical { get; set; }
    public decimal? HDCTonnageGreenMedical { get; set; }
    public decimal HouseholdTonnageWithoutRAM { get; set; }
    public decimal PublicBinTonnageWithoutRAM { get; set; }
    public decimal? HDCTonnageWithoutRAM { get; set; }
    public required decimal ProjectedHouseholdTonnage { get; set; }
    public required decimal ProjectedHouseholdTonnageRed { get; set; }
    public required decimal ProjectedHouseholdTonnageAmber { get; set; }
    public required decimal ProjectedHouseholdTonnageGreen { get; set; }
    public required decimal ProjectedHouseholdTonnageRedMedical { get; set; }
    public required decimal ProjectedHouseholdTonnageAmberMedical { get; set; }
    public required decimal ProjectedHouseholdTonnageGreenMedical { get; set; }
    public required decimal ProjectedPublicBinTonnage { get; set; }
    public required decimal ProjectedPublicBinTonnageRed { get; set; }
    public required decimal ProjectedPublicBinTonnageAmber { get; set; }
    public required decimal ProjectedPublicBinTonnageGreen { get; set; }
    public required decimal ProjectedPublicBinTonnageRedMedical { get; set; }
    public required decimal ProjectedPublicBinTonnageAmberMedical { get; set; }
    public required decimal ProjectedPublicBinTonnageGreenMedical { get; set; }
    public decimal? ProjectedHDCTonnage { get; set; }
    public decimal? ProjectedHDCTonnageRed { get; set; }
    public decimal? ProjectedHDCTonnageAmber { get; set; }
    public decimal? ProjectedHDCTonnageGreen { get; set; }
    public decimal? ProjectedHDCTonnageRedMedical { get; set; }
    public decimal? ProjectedHDCTonnageAmberMedical { get; set; }
    public decimal? ProjectedHDCTonnageGreenMedical { get; set; }
    public required decimal H2RamProportionsRed { get; set; }
    public required decimal H2RamProportionsAmber { get; set; }
    public required decimal H2RamProportionsGreen { get; set; }
    public required decimal H2RamProportionsRedMedical { get; set; }
    public required decimal H2RamProportionsAmberMedical { get; set; }
    public required decimal H2RamProportionsGreenMedical { get; set; }
}
