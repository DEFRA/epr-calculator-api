namespace EPR.Calculator.API.Models
{
    public class CalcResultScaledupProducers
    {
        public CalcResultScaledupProducerHeader TitleHeader { get; set; }

        public IEnumerable<CalcResultSummaryHeader> MaterialBreakdownHeaders { get; set; }

        public IEnumerable<CalcResultSummaryHeader> ColumnHeaders { get; set; }

        public IEnumerable<CalcResultScaledupProducer> ScaledupProducers { get; set; }
    }
}
