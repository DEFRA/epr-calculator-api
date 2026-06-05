using System.Text;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter
{
    public interface ICalcResultSummaryExporter
    {
        void Export(CalcResultSummary resultSummary, StringBuilder csvContent, bool applyModulation);
    }
}
