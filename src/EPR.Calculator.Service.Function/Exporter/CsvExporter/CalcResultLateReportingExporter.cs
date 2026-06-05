using System.Text;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter
{
    public interface ICalcResultLateReportingExporter
    {
        void Export(
            CalcResultLateReportingTonnage? calcResultLateReportingData,
            IImmutableList<MaterialDetail> materials,
            StringBuilder csvContent);
    }

    /// <summary>
    /// Exports the Late Reporting Tonnage data to a string to be added to the results file.
    /// </summary>
    public class CalcResultLateReportingExporter : ICalcResultLateReportingExporter
    {
        public void Export(
            CalcResultLateReportingTonnage? calcResultLateReportingData,
            IImmutableList<MaterialDetail> materials,
            StringBuilder csvContent)
        {
            if (calcResultLateReportingData is null)
            {
                return;
            }

            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.Append(CsvSanitiser.SanitiseData("Parameters - Late Reporting Tonnages"));
            csvContent.AppendLine();
            csvContent.Append(CsvSanitiser.SanitiseData("Material"));
            csvContent.Append(CsvSanitiser.SanitiseData("Late Reporting Tonnage"));
            csvContent.Append(CsvSanitiser.SanitiseData("Red + Red Medical Late Reporting Tonnage"));
            csvContent.Append(CsvSanitiser.SanitiseData("Amber + Amber Medical Late Reporting Tonnage"));
            csvContent.Append(CsvSanitiser.SanitiseData("Green + Green Medical Late Reporting Tonnage"));
            csvContent.AppendLine();

            foreach (var kv in calcResultLateReportingData.ByMaterial)
            {
                var lateReportingData = kv.Value;
                var material = materials.First(m => m.Code == kv.Key);
                AppendRow(material.Name, lateReportingData, csvContent);
            }
            AppendRow("Total", calcResultLateReportingData.Total, csvContent);
        }

        private static void AppendRow(string name, CalcResultLateReportingTonnageDetail lateReportingData, StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(name));
            csvContent.Append(CsvSanitiser.SanitiseData(lateReportingData.Total, DecimalPlaces.Three, null));
            csvContent.Append(CsvSanitiser.SanitiseData(lateReportingData.Red  , DecimalPlaces.Three, null));
            csvContent.Append(CsvSanitiser.SanitiseData(lateReportingData.Amber, DecimalPlaces.Three, null));
            csvContent.Append(CsvSanitiser.SanitiseData(lateReportingData.Green, DecimalPlaces.Three, null));
            csvContent.AppendLine();
        }
    }
}
