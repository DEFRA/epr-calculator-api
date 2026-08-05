using System.Text;
using EPR.Calculator.API.BackgroundService.Models;

namespace EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.RejectedProducers
{
    public interface ICalcResultRejectedProducersExporter
    {
        public void Export(IEnumerable<CalcResultRejectedProducer> rejectedProducers, StringBuilder csvContent);
    }
}
