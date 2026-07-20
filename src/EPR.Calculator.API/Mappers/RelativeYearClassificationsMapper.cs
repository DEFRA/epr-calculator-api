using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Mappers
{
    public static class RelativeYearClassificationsMapper
    {
        public static RelativeYearClassificationResponseDto Map(
            RelativeYear relativeYear,
            IEnumerable<CalculatorRunClassification> classifications)
        {
            return new RelativeYearClassificationResponseDto
            {
                RelativeYear = relativeYear,
                Classifications = classifications.Select(c =>
                    new CalculatorRunClassificationDto
                    {
                        Id = c.Id,
                        Status = c.Status,
                    }).ToList(),
            };
        }

        public static RelativeYearClassificationResponseDto Map(
            RelativeYear relativeYear,
            IEnumerable<CalculatorRunClassification> classifications,
            List<CalculatorRunDto>? runs)
        {
            if (runs is null)
            {
                return Map(relativeYear, classifications);
            }

            return new RelativeYearClassificationResponseDto
            {
                RelativeYear = relativeYear,
                Classifications = classifications.Select(c =>
                    new CalculatorRunClassificationDto
                    {
                        Id = c.Id,
                        Status = c.Status,
                    }).ToList(),
                ClassifiedRuns = runs,
            };
        }
    }
}
