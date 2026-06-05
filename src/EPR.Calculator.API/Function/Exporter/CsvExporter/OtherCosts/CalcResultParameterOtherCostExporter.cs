using System.Text;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts
{
    public interface ICalcResultParameterOtherCostExporter
    {
        void Export(CalcResultParameterOtherCost otherCost, StringBuilder csvContent);
    }

    public class CalcResultParameterOtherCostExporter : ICalcResultParameterOtherCostExporter
    {
        public void Export(CalcResultParameterOtherCost otherCost, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();
            csvContent.AppendLine(CsvSanitiser.SanitiseData("Parameters - Other"));
            SaOperatingCosts(otherCost, csvContent);

            csvContent.AppendLine();

            LaDataPrepCosts(otherCost, csvContent);
            CountryApportionment(otherCost, csvContent);

            csvContent.AppendLine();
            SchemeSetupCost(otherCost, csvContent);

            csvContent.AppendLine();
            csvContent.Append(CsvSanitiser.SanitiseData("6 Bad Debt Provision"));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.BadDebtValue, DecimalPlaces.Two, DecimalFormats.F2, isPercentage: true));
            csvContent.AppendLine();

            csvContent.AppendLine();

            csvContent.Append(CsvSanitiser.SanitiseData("7 Materiality"));
            csvContent.Append(CsvSanitiser.SanitiseData("Amount £s"));
            csvContent.Append(CsvSanitiser.SanitiseData("%"));
            csvContent.AppendLine();
            csvContent.Append(CsvSanitiser.SanitiseData("Increase"));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.MaterialityIncrease.Amount, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.MaterialityIncrease.Percentage, DecimalPlaces.Two, DecimalFormats.F2, isPercentage: true));
            csvContent.AppendLine();
            csvContent.Append(CsvSanitiser.SanitiseData("Decrease"));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.MaterialityDecrease.Amount, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.MaterialityDecrease.Percentage, DecimalPlaces.Two, DecimalFormats.F2, isPercentage: true));
            csvContent.AppendLine();
            csvContent.Append(CsvSanitiser.SanitiseData("8 Tonnage Change"));
            csvContent.Append(CsvSanitiser.SanitiseData("Amount £s"));
            csvContent.Append(CsvSanitiser.SanitiseData("%"));
            csvContent.AppendLine();
            csvContent.Append(CsvSanitiser.SanitiseData("Increase"));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.TonnageChangeIncrease.Amount, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.TonnageChangeIncrease.Percentage, DecimalPlaces.Two, DecimalFormats.F2, isPercentage: true));
            csvContent.AppendLine();
            csvContent.Append(CsvSanitiser.SanitiseData("Decrease"));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.TonnageChangeDecrease.Amount, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.TonnageChangeDecrease.Percentage, DecimalPlaces.Two, DecimalFormats.F2, isPercentage: true));
            csvContent.AppendLine();
        }

        public void SchemeSetupCost(CalcResultParameterOtherCost otherCost, StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData("5 Scheme set up cost Yearly Cost"));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.SchemeSetupCost.England        , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.SchemeSetupCost.Wales          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.SchemeSetupCost.Scotland       , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.SchemeSetupCost.NorthernIreland, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.SchemeSetupCost.Total          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.AppendLine();
        }

        public void LaDataPrepCosts(CalcResultParameterOtherCost otherCost, StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData("4 LA Data Prep Charge"));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.LaDataPrepCharge.England        , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.LaDataPrepCharge.Wales          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.LaDataPrepCharge.Scotland       , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.LaDataPrepCharge.NorthernIreland, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.LaDataPrepCharge.Total          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.AppendLine();
        }

        public void CountryApportionment(CalcResultParameterOtherCost otherCost, StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData("4 Country Apportionment %s"));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.CountryApportionment.England        , DecimalPlaces.Eight, DecimalFormats.F8, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.CountryApportionment.Wales          , DecimalPlaces.Eight, DecimalFormats.F8, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.CountryApportionment.Scotland       , DecimalPlaces.Eight, DecimalFormats.F8, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.CountryApportionment.NorthernIreland, DecimalPlaces.Eight, DecimalFormats.F8, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(100                                           , DecimalPlaces.Eight, DecimalFormats.F8, isPercentage: true));
            csvContent.AppendLine();
        }

        public void SaOperatingCosts(CalcResultParameterOtherCost otherCost, StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData((string?)null));
            csvContent.Append(CsvSanitiser.SanitiseData("England"));
            csvContent.Append(CsvSanitiser.SanitiseData("Wales"));
            csvContent.Append(CsvSanitiser.SanitiseData("Scotland"));
            csvContent.Append(CsvSanitiser.SanitiseData("Northern Ireland"));
            csvContent.Append(CsvSanitiser.SanitiseData("Total"));
            csvContent.AppendLine();

            csvContent.Append(CsvSanitiser.SanitiseData("3 SA Operating Costs"));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.SaOperatingCost.England        , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.SaOperatingCost.Wales          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.SaOperatingCost.Scotland       , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.SaOperatingCost.NorthernIreland, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.SaOperatingCost.Total          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.AppendLine();
        }
    }
}
