using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Utils;

namespace EPR.Calculator.API.Mappers
{
    public static class RelativeYearClassificationsMapper
    {
        public static RelativeYearClassificationResponseDto Map(
            RelativeYear relativeYear,
            List<RunClassification> classifications,
            List<CalculatorRunDto> runs)
        {
            return new RelativeYearClassificationResponseDto
            {
                RelativeYear = relativeYear,
                Classifications = classifications,
                ClassifiedRuns = runs,
            };
        }
    }
}
