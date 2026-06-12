using System.ComponentModel.DataAnnotations;

namespace EPR.Calculator.API.Dtos
{
    public class CalcRelativeYearRequestDto
    {
        [Required]
        public required int RunId { get; set; }

        [Required]
        public required int RelativeYearValue { get; set; } // Query Param so need to be an int
    }
}
