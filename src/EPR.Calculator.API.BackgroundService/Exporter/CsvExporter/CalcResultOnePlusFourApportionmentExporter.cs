using System.Text;
using EPR.Calculator.API.BackgroundService.Enums;
using EPR.Calculator.API.BackgroundService.Misc;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Exporter.CsvExporter
{
    public interface ICalcResultOnePlusFourApportionmentExporter
    {
        void Export(CalcResultOnePlusFourApportionment calcResult1Plus4Apportionment, StringBuilder csvContent);
    }

    public class CalcResultOnePlusFourApportionmentExporter : ICalcResultOnePlusFourApportionmentExporter
    {
        [ActivityTrace]
        public void Export(CalcResultOnePlusFourApportionment calcResult1Plus4Apportionment, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(CsvSanitiser.SanitiseData("1 + 4 Apportionment %"));

            AppendHeaders(csvContent);
            AppendByCountryCost("1 Fee for LA Disposal Costs", calcResult1Plus4Apportionment.LaDisposalCost, csvContent);
            AppendByCountryCost("4 LA Data Prep Charge"      , calcResult1Plus4Apportionment.LADataPrepCharge, csvContent);
            AppendByCountryCost("Total of 1 + 4"             , calcResult1Plus4Apportionment.TotalOnePlusFour, csvContent);
            AppendByCountryApportionment("1 + 4 Apportionment %", calcResult1Plus4Apportionment.OnePlusFourApportionment, csvContent);
        }

        private static void AppendHeaders(StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData((string?)null));
            csvContent.Append(CsvSanitiser.SanitiseData("England"));
            csvContent.Append(CsvSanitiser.SanitiseData("Wales"));
            csvContent.Append(CsvSanitiser.SanitiseData("Scotland"));
            csvContent.Append(CsvSanitiser.SanitiseData("Northern Ireland"));
            csvContent.Append(CsvSanitiser.SanitiseData("Total UK"));
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
