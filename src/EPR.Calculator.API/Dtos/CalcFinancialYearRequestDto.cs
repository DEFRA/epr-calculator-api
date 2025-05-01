using System.ComponentModel.DataAnnotations;

namespace EPR.Calculator.API.Dtos
{
    public class CalcFinancialYearRequestDto
    {
        [Required]
        public int RunId { get; set; }

        [Required]
        public required string FinancialYear { get; set; }
    }
}
