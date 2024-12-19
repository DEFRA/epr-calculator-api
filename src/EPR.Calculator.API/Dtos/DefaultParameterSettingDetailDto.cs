using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
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
