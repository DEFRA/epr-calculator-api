using System.Text;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap
{
    public interface ICalcResultLapcapDataExporter
    {
        void Export(
            CalcResultLapcapData calcResultLapcapData,
            IImmutableList<MaterialDetail> materialDetails,
            StringBuilder csvContent
        );
    }

    public class CalcResultLapcapDataExporter : ICalcResultLapcapDataExporter
    {
        public void Export(
            CalcResultLapcapData calcResultLapcapData,
            IImmutableList<MaterialDetail> materialDetails,
            StringBuilder csvContent
        )
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(CsvSanitiser.SanitiseData("LAPCAP Data"));

            AppendHeaders(csvContent);
            foreach (var material in materialDetails)
            {
                var lapcapDta = calcResultLapcapData.ByMaterial[material.Code];
                AppendRow(material.Name, lapcapDta, csvContent);
            }
            AppendRow("Total", calcResultLapcapData.Total, csvContent);

            csvContent.Append($"{CsvSanitiser.SanitiseData("1 Country Apportionment %s")}");
            csvContent.Append(CsvSanitiser.SanitiseData(calcResultLapcapData.CountryApportionment.England        , DecimalPlaces.Eight, DecimalFormats.F8, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(calcResultLapcapData.CountryApportionment.Wales          , DecimalPlaces.Eight, DecimalFormats.F8, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(calcResultLapcapData.CountryApportionment.Scotland       , DecimalPlaces.Eight, DecimalFormats.F8, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(calcResultLapcapData.CountryApportionment.NorthernIreland, DecimalPlaces.Eight, DecimalFormats.F8, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(100                                                      , DecimalPlaces.Eight, DecimalFormats.F8, isPercentage: true));

            csvContent.AppendLine();
        }

        private static void AppendHeaders(StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData("Material"));
            csvContent.Append(CsvSanitiser.SanitiseData("England LA Disposal Cost"));
            csvContent.Append(CsvSanitiser.SanitiseData("Wales LA Disposal Cost"));
            csvContent.Append(CsvSanitiser.SanitiseData("Scotland LA Disposal Cost"));
            csvContent.Append(CsvSanitiser.SanitiseData("Northern Ireland LA Disposal Cost"));
            csvContent.Append(CsvSanitiser.SanitiseData("1 LA Disposal Cost Total"));
            csvContent.AppendLine();
        }

        private static void AppendRow(string name, ByCountryCost value, StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(name));
            csvContent.Append(CsvSanitiser.SanitiseData(value.England        , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(value.Wales          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(value.Scotland       , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(value.NorthernIreland, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(value.Total          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.AppendLine();
        }
    }
}
