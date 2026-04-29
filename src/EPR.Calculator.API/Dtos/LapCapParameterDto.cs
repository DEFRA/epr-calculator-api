using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class LapCapParameterDto
    {
        public int Id { get; set; }

        public required RelativeYear RelativeYear { get; set; }

        public required string CreatedBy { get; set; }

        public required DateTime CreatedAt { get; set; }

        public required DateTime EffectiveFrom { get; set; }

        public required int LapcapDataMasterId { get; set; }

        public required string LapcapTempUniqueRef { get; set; }

        public required string Country { get; set; }

        public required string Material { get; set; }

        public required decimal TotalCost { get; set; }
    }
}