using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;

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
                    errors.Add(new CreateLapcapDataErrorDto {
                        Country = lapcapTemplate.Country,
                        Material = lapcapTemplate.Material,
                        Message = "",
                        Description = "",
                        UniqueReference =lapcapTemplate.UniqueReference
                    });
                }
                else if (matchingCount > 1) 
                {
                    errors.Add(new CreateLapcapDataErrorDto
                    {
                        Country = lapcapTemplate.Country,
                        Material = lapcapTemplate.Material,
                        Message = "",
                        Description = "",
                        UniqueReference = lapcapTemplate.UniqueReference
                    });
                }
                else
                {
                    decimal totalCostValue;
                    var data = matchingLapcapData.Single();
                    var totalCostStr = data.TotalCost;
                    if (string.IsNullOrEmpty(totalCostStr))
                    {
                        errors.Add(new CreateLapcapDataErrorDto
                        {
                            Country = lapcapTemplate.Country,
                            Material = lapcapTemplate.Material,
                            Message = "",
                            Description = "",
                            UniqueReference = lapcapTemplate.UniqueReference
                        });
                    }
                    else if (decimal.TryParse(totalCostStr, out totalCostValue))
                    {
                        if (totalCostValue < lapcapTemplate.TotalCostFrom ||
                            totalCostValue > lapcapTemplate.TotalCostTo)
                        {
                            errors.Add(new CreateLapcapDataErrorDto
                            {
                                Country = lapcapTemplate.Country,
                                Material = lapcapTemplate.Material,
                                Message = "",
                                Description = "",
                                UniqueReference = lapcapTemplate.UniqueReference
                            });
                        }
                    }
                    else
                    {
                        errors.Add(new CreateLapcapDataErrorDto
                        {
                            Country = lapcapTemplate.Country,
                            Material = lapcapTemplate.Material,
                            Message = "",
                            Description = "",
                            UniqueReference = lapcapTemplate.UniqueReference
                        });
                    }
                }
            }
            validationResult.Errors.AddRange(errors);
            validationResult.IsInvalid = errors.Count() > 0;
            return validationResult;
        }
    }
}