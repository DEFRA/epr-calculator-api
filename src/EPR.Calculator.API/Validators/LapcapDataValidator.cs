using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Validators
{
    public class LapcapDataValidator : ILapcapDataValidator
    {
        private readonly ApplicationDBContext context;

        public LapcapDataValidator(ApplicationDBContext context)
        {
            this.context = context;
        }

        public ValidationResultDto<CreateLapcapDataErrorDto> Validate(CreateLapcapDataDto createLapcapDataDto)
        {
            var lapcapTemplateList = this.context.LapcapDataTemplateMaster.ToList();
            var validationResult = new ValidationResultDto<CreateLapcapDataErrorDto>();
            var lapcapDataTemplateValues = createLapcapDataDto.LapcapDataTemplateValues;

            foreach (var lapcapTemplate in lapcapTemplateList)
            {
                var matchingLapcapData = lapcapDataTemplateValues.Where(x => x.CountryName == lapcapTemplate.Country &&
                    x.Material == lapcapTemplate.Material);
                var matchingCount = matchingLapcapData.Count();
                var country = lapcapTemplate.Country;
                var material = lapcapTemplate.Material;
                var uniqueRef = lapcapTemplate.UniqueReference;
                var totalCostFrom = lapcapTemplate.TotalCostFrom;
                var totalCostTo = lapcapTemplate.TotalCostTo;
                var errorMessage = string.Empty;

                if (matchingCount == 0)
                {
                    errorMessage = string.Format(CommonResources.EnterTotalCosts, material, country);
                }
                else if (matchingCount > 1)
                {
                    errorMessage = string.Format(CommonResources.TotalCostForMaterialAndCountry, material, country);
                }
                else
                {
                    errorMessage = ValidateTotalCosts(lapcapTemplate, matchingLapcapData, country, material, totalCostFrom, totalCostTo);
                }

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    validationResult.Errors.Add(
                        new CreateLapcapDataErrorDto
                        {
                            Country = country,
                            Material = material,
                            Message = errorMessage,
                            Description = string.Empty,
                            UniqueReference = uniqueRef,
                        }
                    );
                }
            }

            if(validationResult.Errors.Count == 0)
            {
                var totalErrors = ValidateOverallTotalCosts(lapcapDataTemplateValues);
                validationResult.Errors.AddRange(totalErrors);
            }

            validationResult.IsInvalid = validationResult.Errors.Count > 0;
            return validationResult;
        }

        private static List<CreateLapcapDataErrorDto> ValidateOverallTotalCosts(IEnumerable<LapcapDataTemplateValueDto> lapcapDataTemplateValues)
        {
            var totalCostsByCountry = lapcapDataTemplateValues.GroupBy(l => l.CountryName).Select(g => g.Sum(x => decimal.Parse(x.TotalCost)));
            var totalCostsByMaterial = lapcapDataTemplateValues.GroupBy(l => l.Material).Select(g => g.Sum(x => decimal.Parse(x.TotalCost)));
            List<CreateLapcapDataErrorDto> errors = new List<CreateLapcapDataErrorDto>();

            if (totalCostsByCountry.Any(t => t < 0))
            {
                errors.Add(
                    new CreateLapcapDataErrorDto
                    {
                        Message = "The total disposal cost for one or more nations is negative. Check that the total disposal cost for each nation is zero or greater."
                    }
                );
            }

            if (totalCostsByMaterial.Any(t => t < 0))
            {
                errors.Add(
                    new CreateLapcapDataErrorDto
                    {
                        Message = "The total disposal cost for one or more packaging materials is negative. Check that the total disposal cost for each packaging material is zero or greater."
                    }
                );
            }
            
            return errors;
        }

        private static string ValidateTotalCosts(
            LapcapDataTemplateMaster lapcapTemplate,
            IEnumerable<LapcapDataTemplateValueDto> matchingLapcapData,
            string country,
            string material,
            decimal totalCostFrom,
            decimal totalCostTo)
        {
            string errorMessage = string.Empty;
            var data = matchingLapcapData.Single();
            var totalCostStr = data.TotalCost;
            if (string.IsNullOrEmpty(totalCostStr))
            {
                errorMessage = string.Format(CommonResources.EnterTotalCosts, material, country);
            }
            else if (decimal.TryParse(totalCostStr, out decimal totalCostValue))
            {
                if (totalCostValue < lapcapTemplate.TotalCostFrom ||
                    totalCostValue > lapcapTemplate.TotalCostTo)
                {
                    errorMessage = string.Format(CommonResources.TotalCostsRange, material, country, totalCostFrom.ToString("#,##0.00"), totalCostTo.ToString("#,##0.00"));
                }
            }
            else
            {
                errorMessage = string.Format(CommonResources.TotalCostsForMaterial, material);
            }

            return errorMessage;
        }
    }
}
