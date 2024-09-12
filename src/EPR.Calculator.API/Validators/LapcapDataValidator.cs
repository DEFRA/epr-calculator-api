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
            var errors = new List<CreateLapcapDataErrorDto>();

            foreach (var lapcapTemplate in lapcapTemplateList)
            {
                var matchingLapcapData = lapcapDataTemplateValues.Where(x => x.CountryName == lapcapTemplate.Country &&
                    x.Material == lapcapTemplate.Material);
                var matchingCount = matchingLapcapData.Count();

                if (matchingCount == 0)
                {
                    var message = $"";
                    var errorDto = Util.CreateLapcapDataErrorDto(lapcapTemplate.Country,
                                                                    lapcapTemplate.Material,
                                                                    lapcapTemplate.Material,
                                                                    string.Empty,
                                                                    message);
                    errors.Add(errorDto);
                }
                else if (matchingCount > 1) 
                {
                    var message = "";
                    var errorDto = Util.CreateLapcapDataErrorDto(lapcapTemplate.Country,
                                                                    lapcapTemplate.Material,
                                                                    lapcapTemplate.Material,
                                                                    string.Empty,
                                                                    message);
                    errors.Add(errorDto);
                }
                else
                {
                    decimal totalCostValue;
                    var data = matchingLapcapData.Single();
                    var totalCostStr = data.TotalCost;
                    if (string.IsNullOrEmpty(totalCostStr))
                    {
                        var message = "";
                        var errorDto = Util.CreateLapcapDataErrorDto(lapcapTemplate.Country,
                                                                        lapcapTemplate.Material,
                                                                        lapcapTemplate.Material,
                                                                        string.Empty,
                                                                        message);
                        errors.Add(errorDto);
                    }
                    else if (decimal.TryParse(totalCostStr, out totalCostValue))
                    {
                        if (totalCostValue < lapcapTemplate.TotalCostFrom ||
                            totalCostValue > lapcapTemplate.TotalCostTo)
                        {
                            var message = "";
                            var errorDto = Util.CreateLapcapDataErrorDto(lapcapTemplate.Country,
                                                                            lapcapTemplate.Material,
                                                                            lapcapTemplate.Material,
                                                                            string.Empty,
                                                                            message);
                            errors.Add(errorDto);
                        }
                    }
                    else
                    {
                        var message = "";
                        var errorDto = Util.CreateLapcapDataErrorDto(lapcapTemplate.Country,
                                                                        lapcapTemplate.Material,
                                                                        lapcapTemplate.Material,
                                                                        string.Empty,
                                                                        message);
                        errors.Add(errorDto);
                    }
                }
            }
            validationResult.Errors.AddRange(errors);
            validationResult.IsInvalid = errors.Count() > 0;
            return validationResult;
        }
    }
}