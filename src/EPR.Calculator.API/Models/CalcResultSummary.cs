﻿namespace EPR.Calculator.API.Models
{
    public class CalcResultSummary
    {
        public string ProducerId { get; set; }

        public string SubsidiaryId { get; set; }

        public string ProducerName { get; set; }

        public int Level { get; set; }

        public int Order {  get; set; }

        public Dictionary<MaterialDetail, IEnumerable<CalcResultSummaryMaterialCost>> MaterialCostSummary { get; set; }
    }
}
