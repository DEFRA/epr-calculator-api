using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class DefaultParameterSettingDetailDto
    {
        public int Id { get; set; }

        public required string ParameterType { get; set; }

        public required string ParameterCategory { get; set; }

        public required string ParameterUnit { get; set; }

        public required decimal ParameterValue { get; set; }
    }
}
