using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Utils;

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
                    decimal totalCostValue;
                    var data = matchingLapcapData.Single();
                    var totalCostStr = data.TotalCost;
                    if (string.IsNullOrEmpty(totalCostStr))
                    {
                        errorMessage = string.Format(CommonResources.EnterTotalCosts, material, country);
                    }
                    else if (decimal.TryParse(totalCostStr, out totalCostValue))
                    {
                        if (totalCostValue < lapcapTemplate.TotalCostFrom ||
                            totalCostValue > lapcapTemplate.TotalCostTo)
                        {
                            errorMessage = string.Format(CommonResources.TotalCostsRange, material, country, Convert.ToInt16(totalCostFrom), totalCostTo.ToString("#,##0.00"));
                        }
                    }
                    else
                    {
                        errorMessage = string.Format(CommonResources.TotalCostsForMaterial, material);
                    }
                }

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    var errorDto = Util.CreateLapcapDataErrorDto(country, material, errorMessage, string.Empty, uniqueRef);
                    validationResult.Errors.Add(errorDto);
                }
            }

            validationResult.IsInvalid = validationResult.Errors.Count > 0;
            return validationResult;
        }
    }
}