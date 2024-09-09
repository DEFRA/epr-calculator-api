using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class LapCapParameterDto
    {
        public int Id { get; set; }
        public string Year { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public int LapcapDataMasterId { get; set; }
        public string LapcapTempUniqueRef { get; set; }
        public string Country { get; set; }

        public string Material { get; set; }

        public decimal TotalCost { get; set; }
    }
}
