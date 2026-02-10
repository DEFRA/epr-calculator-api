using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class CreateLapcapDataDto
    {
        public required RelativeYear RelativeYear { get; set; }

        public required IEnumerable<LapcapDataTemplateValueDto> LapcapDataTemplateValues { get; set; }

        public required string LapcapFileName { get; set; }
    }
}