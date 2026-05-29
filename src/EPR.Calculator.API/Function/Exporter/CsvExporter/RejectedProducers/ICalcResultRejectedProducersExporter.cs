using System.Text;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.RejectedProducers
{
    public interface ICalcResultRejectedProducersExporter
    {
        public void Export(IEnumerable<CalcResultRejectedProducer> rejectedProducers, StringBuilder csvContent);
    }
}
