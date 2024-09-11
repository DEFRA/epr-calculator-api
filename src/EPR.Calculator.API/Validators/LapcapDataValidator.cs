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

        public ValidationResultDto Validate(CreateLapcapDataDto createLapcapDataDto)
        {
            throw new NotImplementedException();
        }
    }
}