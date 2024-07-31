using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPR.Calculator.API.Dtos
{
    public class DefaultParameterSettingDetailDto
    {
        public int Id { get; set; }

        public string ParameterType { get; set; }

        public string ParameterCategory { get; set; }
        public string ParameterUnit { get; set; }

        public decimal ParameterValue { get; set; }
    }
}
