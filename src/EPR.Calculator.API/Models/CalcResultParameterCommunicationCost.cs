﻿namespace EPR.Calculator.API.Models
{
    public class CalcResultParameterCommunicationCost
    {
        public IEnumerable<CalcResultParameterCostDetail> CalcResultParameterCommunicationCostDetails { get; set; } = 
            new List<CalcResultParameterCostDetail>();
    }
}
