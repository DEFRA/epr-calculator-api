using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EPR.Calculator.API.Dtos
{
    public class DefaultParameterSettingMasterDto
    {
        public int Id { get; set; }

        public string ParameterYear { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime EffectiveTo { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
