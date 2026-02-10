using System.ComponentModel.DataAnnotations;
using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.API.Dtos
{
    public class CalcRelativeYearRequestDto
    {
        [Required]
        public int RunId { get; set; }

        [Required]
        public required int RelativeYearValue { get; set; } // Query Param so need to be an int
    }
}
