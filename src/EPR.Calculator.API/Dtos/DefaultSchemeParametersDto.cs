namespace EPR.Calculator.API.Dtos
{
    public class DefaultSchemeParametersDto
    {
        public int Id { get; set; }
        public string ParameterYear { get; set; }
        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public int DefaultParameterSettingMasterId { get; set; }
        public string ParameterUniqueRef { get; set; }
        public string ParameterType { get; set; }

        public string ParameterCategory { get; set; }

        public decimal ParameterValue { get; set; }

    }
}
