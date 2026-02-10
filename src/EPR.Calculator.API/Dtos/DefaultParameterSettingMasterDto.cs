using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class DefaultParameterSettingMasterDto
    {
        public int Id { get; set; }

        public required RelativeYear RelativeYear { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime EffectiveTo { get; set; }

        public required string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
