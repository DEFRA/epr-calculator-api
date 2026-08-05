using System.Text;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.ErrorReport
{
    public interface ICalcResultErrorReportExporter
    {
        void Export(IEnumerable<CalcResultErrorReport> errorReport, StringBuilder csvContent);
    }
}
