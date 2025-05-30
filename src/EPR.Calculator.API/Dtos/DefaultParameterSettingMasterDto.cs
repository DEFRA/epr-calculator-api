﻿using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class DefaultParameterSettingMasterDto
    {
        public int Id { get; set; }

        public required string ParameterYear { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime EffectiveTo { get; set; }

        public required string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
