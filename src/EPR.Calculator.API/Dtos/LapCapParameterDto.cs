using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class LapCapParameterDto
    {
        public int Id { get; set; }

        required public string Year { get; set; }

        required public string CreatedBy { get; set; }

        required public DateTime CreatedAt { get; set; }

        required public int LapcapDataMasterId { get; set; }

        required public string LapcapTempUniqueRef { get; set; }

        required public string Country { get; set; }

        required public string Material { get; set; }

        required public decimal TotalCost { get; set; }
    }
}