using System.Text;
using EPR.Calculator.API.BackgroundService.Models;

namespace EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.ErrorReport
{
    public interface ICalcResultErrorReportExporter
    {
        void Export(IEnumerable<CalcResultErrorReport> errorReport, StringBuilder csvContent);
    }
}
