﻿using api.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Dtos
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