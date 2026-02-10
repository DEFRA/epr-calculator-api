using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class DefaultSchemeParametersDto
    {
        public int Id { get; set; }

        public required RelativeYear RelativeYear { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public required string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public int DefaultParameterSettingMasterId { get; set; }

        public required string ParameterUniqueRef { get; set; }

        public required string ParameterType { get; set; }

        public required string ParameterCategory { get; set; }

        public decimal ParameterValue { get; set; }
    }
}
