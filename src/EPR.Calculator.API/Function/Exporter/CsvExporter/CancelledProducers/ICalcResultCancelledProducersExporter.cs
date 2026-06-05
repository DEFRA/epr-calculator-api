using System.Text;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers
{
    public interface ICalcResultCancelledProducersExporter
    {
        public void Export(CalcResultCancelledProducersResponse calcResultCancelledProducers, StringBuilder csvContent);
    }
}