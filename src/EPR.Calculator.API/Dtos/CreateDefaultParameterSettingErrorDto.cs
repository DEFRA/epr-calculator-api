using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class CreateDefaultParameterSettingErrorDto : ErrorDto
    {
        public required string ParameterUniqueRef { get; set; }

        public required string ParameterCategory { get; set; }

        public required string ParameterType { get; set; }
    }
}