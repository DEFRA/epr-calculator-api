using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class CreateLapcapDataErrorDto : ErrorDto
    {
        public string? UniqueReference { get; set; }

        public string? Country { get; set; }

        public string? Material { get; set; }
    }
}