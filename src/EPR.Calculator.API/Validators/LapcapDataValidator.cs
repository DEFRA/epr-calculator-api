using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Utils;

namespace api.Validators
{
    public class LapcapDataValidator : ILapcapDataValidator
    {
        private readonly ApplicationDBContext context;
        public LapcapDataValidator(ApplicationDBContext context) 
        {
            this.context = context;
        }

        public LapcapValidationResultDto Validate(CreateLapcapDataDto createLapcapDataDto)
        {
            var lapcapTemplateList = this.context.LapcapDataTemplateMaster.ToList();
            var validationResult = new LapcapValidationResultDto();
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
                    errorMessage = $"Enter the lapcap data for {country} and {material}";
                }
                else if (matchingCount > 1) 
                {
                    errorMessage = $"Expecting only One with {country} and {material}";
                }
                else
                {
                    decimal totalCostValue;
                    var data = matchingLapcapData.Single();
                    var totalCostStr = data.TotalCost;
                    if (string.IsNullOrEmpty(totalCostStr))
                    {
                        errorMessage = $"Enter the lapcap data for {country} and {material}";
                    }
                    else if (decimal.TryParse(totalCostStr, out totalCostValue))
                    {
                        if (totalCostValue < lapcapTemplate.TotalCostFrom ||
                            totalCostValue > lapcapTemplate.TotalCostTo)
                        {
                            errorMessage = $"Total cost for {country} and {material} should be between {totalCostFrom} and {totalCostTo}";
                        }
                    }
                    else
                    {
                        errorMessage = $"Enter the lapcap data for {country} and {material}";
                    }
                }
                if(!string.IsNullOrEmpty(errorMessage))
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