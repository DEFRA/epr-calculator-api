using System.Text;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter
{
    public interface ICalcResultOnePlusFourApportionmentExporter
    {
        void Export(CalcResultOnePlusFourApportionment calcResult1Plus4Apportionment, StringBuilder csvContent);
    }

    public class CalcResultOnePlusFourApportionmentExporter : ICalcResultOnePlusFourApportionmentExporter
    {
        public void Export(CalcResultOnePlusFourApportionment calcResult1Plus4Apportionment, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(CsvSanitiser.SanitiseData("1 + 4 Apportionment %s"));

            AppendHeaders(csvContent);
            AppendByCountryCost("1 Fee for LA Disposal Costs", calcResult1Plus4Apportionment.LaDisposalCost, csvContent);
            AppendByCountryCost("4 LA Data Prep Charge"      , calcResult1Plus4Apportionment.LADataPrepCharge, csvContent);
            AppendByCountryCost("Total of 1 + 4"             , calcResult1Plus4Apportionment.TotalOnePlusFour, csvContent);
            AppendByCountryApportionment("1 + 4 Apportionment %s", calcResult1Plus4Apportionment.OnePlusFourApportionment, csvContent);
        }

        private static void AppendHeaders(StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData((string?)null));
            csvContent.Append(CsvSanitiser.SanitiseData("England"));
            csvContent.Append(CsvSanitiser.SanitiseData("Wales"));
            csvContent.Append(CsvSanitiser.SanitiseData("Scotland"));
            csvContent.Append(CsvSanitiser.SanitiseData("Northern Ireland"));
            csvContent.Append(CsvSanitiser.SanitiseData("Total"));
            csvContent.AppendLine();
        }

        private static void AppendByCountryCost(string name, ByCountryCost byCountryValue, StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(name));
            csvContent.Append(CsvSanitiser.SanitiseData(byCountryValue.England        , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(byCountryValue.Wales          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(byCountryValue.Scotland       , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(byCountryValue.NorthernIreland, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(byCountryValue.Total          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.AppendLine();
        }

        private static void AppendByCountryApportionment(string name, ByCountryApportionment byCountryApportionment, StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(name));
            csvContent.Append(CsvSanitiser.SanitiseData(byCountryApportionment.England        , DecimalPlaces.Eight, DecimalFormats.F8, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(byCountryApportionment.Wales          , DecimalPlaces.Eight, DecimalFormats.F8, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(byCountryApportionment.Scotland       , DecimalPlaces.Eight, DecimalFormats.F8, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(byCountryApportionment.NorthernIreland, DecimalPlaces.Eight, DecimalFormats.F8, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(100                                   , DecimalPlaces.Eight, DecimalFormats.F8, isPercentage: true));
            csvContent.AppendLine();
        }
    }
}
