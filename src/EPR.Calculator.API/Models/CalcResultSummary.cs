﻿namespace EPR.Calculator.API.Models
{
    public class CalcResultSummary
    {
        public CalcResultSummaryHeader ResultSummaryHeader { get; set; }

        public CalcResultSummaryHeader ProducerDisposalFeesHeader { get; set; }

        public CalcResultSummaryHeader CommsCostHeader { get; set; }

        public IEnumerable<CalcResultSummaryHeader> MaterialBreakdownHeaders { get; set; }

       // public IEnumerable<CalcResultSummaryHeader> MaterialBreakdownHeadersForTwoACommsCost { get; set; }

        public IEnumerable<string> ColumnHeaders { get; set; }

        public IEnumerable<CalcResultSummaryProducerDisposalFees> ProducerDisposalFees { get; set; }

        public IEnumerable<CalcResultSummaryProducerDisposalFeesByCountry> ProducerDisposalFeesByCountry { get; set; }

        public IEnumerable<CalcResultSummaryProducerCommsFeesByCountry> ProducerCommsFeesByCountry { get; set; }
    }
}
