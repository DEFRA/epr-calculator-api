using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost
{
    public interface ICalcResultCommsCostExporter
    {
        void Export(CalcResultCommsCost communicationCost, IImmutableList<MaterialDetail> materials, StringBuilder csvContent);
    }

    /// <summary>
    /// Provides functionality to export communication cost details to a CSV format.
    /// </summary>
    public class CalcResultCommsCostExporter : ICalcResultCommsCostExporter
    {
        /// <summary>
        /// Exports the communication cost details to the provided StringBuilder in CSV format.
        /// </summary>
        public void Export(CalcResultCommsCost communicationCost, IImmutableList<MaterialDetail> materials, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();
            csvContent.AppendLine(CsvSanitiser.SanitiseData("Parameters - Comms Costs"));

            var onePlusFourApportionment = communicationCost.OnePlusFourApportionment;

            AppendHeaderApportionmentHeaders(csvContent);
            csvContent.Append(CsvSanitiser.SanitiseData("1 + 4 Apportionment %s"));
            csvContent.Append(CsvSanitiser.SanitiseData(onePlusFourApportionment.England        , DecimalPlaces.Eight, DecimalFormats.F8, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(onePlusFourApportionment.Wales          , DecimalPlaces.Eight, DecimalFormats.F8, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(onePlusFourApportionment.Scotland       , DecimalPlaces.Eight, DecimalFormats.F8, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(onePlusFourApportionment.NorthernIreland, DecimalPlaces.Eight, DecimalFormats.F8, isPercentage: true));
            csvContent.Append(CsvSanitiser.SanitiseData(onePlusFourApportionment.Total          , DecimalPlaces.Eight, DecimalFormats.F8, isPercentage: true));
            csvContent.AppendLine();

            csvContent.AppendLine();
            AppendHeader(csvContent);
            foreach (var commCostByMaterial in communicationCost.ByMaterial)
            {
                var material = materials.First(m => m.Code == commCostByMaterial.Key);
                var commCost = commCostByMaterial.Value;
                AppendRow(material.Name, commCost, csvContent);
            }
            AppendRow("Total", communicationCost.Total, csvContent);

            csvContent.AppendLine();

            csvContent.Append(CsvSanitiser.SanitiseData((string?)null));
            csvContent.Append(CsvSanitiser.SanitiseData("England"));
            csvContent.Append(CsvSanitiser.SanitiseData("Wales"));
            csvContent.Append(CsvSanitiser.SanitiseData("Scotland"));
            csvContent.Append(CsvSanitiser.SanitiseData("Northern Ireland"));
            csvContent.Append(CsvSanitiser.SanitiseData("Total"));
            csvContent.AppendLine();

            csvContent.Append(CsvSanitiser.SanitiseData("2b Comms Costs - UK wide"));
            csvContent.Append(CsvSanitiser.SanitiseData(communicationCost.CommsCostUkWide.England        , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(communicationCost.CommsCostUkWide.Wales          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(communicationCost.CommsCostUkWide.Scotland       , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(communicationCost.CommsCostUkWide.NorthernIreland, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(communicationCost.CommsCostUkWide.Total          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.AppendLine();

            csvContent.Append(CsvSanitiser.SanitiseData("2c Comms Costs - by Country"));
            csvContent.Append(CsvSanitiser.SanitiseData(communicationCost.CommsCostByCountry.England        , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(communicationCost.CommsCostByCountry.Wales          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(communicationCost.CommsCostByCountry.Scotland       , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(communicationCost.CommsCostByCountry.NorthernIreland, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(communicationCost.CommsCostByCountry.Total          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.AppendLine();
        }

        private static void AppendHeaderApportionmentHeaders(StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData((string?)null));
            csvContent.Append(CsvSanitiser.SanitiseData("England"));
            csvContent.Append(CsvSanitiser.SanitiseData("Wales"));
            csvContent.Append(CsvSanitiser.SanitiseData("Scotland"));
            csvContent.Append(CsvSanitiser.SanitiseData("Northern Ireland"));
            csvContent.Append(CsvSanitiser.SanitiseData("Total"));
            csvContent.AppendLine();
        }

        private static void AppendHeader(StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData("2a Comms Costs - by Material"));
            csvContent.Append(CsvSanitiser.SanitiseData("England"));
            csvContent.Append(CsvSanitiser.SanitiseData("Wales"));
            csvContent.Append(CsvSanitiser.SanitiseData("Scotland"));
            csvContent.Append(CsvSanitiser.SanitiseData("Northern Ireland"));
            csvContent.Append(CsvSanitiser.SanitiseData("Total"));
            csvContent.Append(CsvSanitiser.SanitiseData("Producer Household Packaging Tonnage"));
            csvContent.Append(CsvSanitiser.SanitiseData("Public Bin Tonnage"));
            csvContent.Append(CsvSanitiser.SanitiseData("Household Drinks Containers Tonnage"));
            csvContent.Append(CsvSanitiser.SanitiseData("Late Reporting Tonnage"));
            csvContent.Append(CsvSanitiser.SanitiseData("Producer Household Tonnage + Late Reporting Tonnage + Public Bin Tonnage + Household Drinks Containers Tonnage"));
            csvContent.Append(CsvSanitiser.SanitiseData("Comms Cost - by Material Price Per Tonne"));
            csvContent.AppendLine();
        }

        private static void AppendRow(string name, CalcResultCommsCostCommsCostByMaterial commCost, StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(name));
            csvContent.Append(CsvSanitiser.SanitiseData(commCost.Cost.England        , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(commCost.Cost.Wales          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(commCost.Cost.Scotland       , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(commCost.Cost.NorthernIreland, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(commCost.Cost.Total          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(commCost.HouseholdPackagingWasteTonnage  , DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(commCost.PublicBinTonnage                , DecimalPlaces.Three, DecimalFormats.F3));
            if (name != "Total" && name != MaterialNames.Glass && commCost.HouseholdDrinksContainersTonnage == 0)
            {
                csvContent.Append(CsvSanitiser.SanitiseData((string?)null));
            }
            else
            {
                csvContent.Append(CsvSanitiser.SanitiseData(commCost.HouseholdDrinksContainersTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            }
            csvContent.Append(CsvSanitiser.SanitiseData(commCost.LateReportingTonnage            , DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(commCost.TotalTonnage                    , DecimalPlaces.Three, DecimalFormats.F3));
            if (name == "Total")
            {
                csvContent.Append(CsvSanitiser.SanitiseData((string?)null));
            }
            else
            {
                csvContent.Append(CsvSanitiser.SanitiseData(commCost.PricePerTonne, DecimalPlaces.Four, null, isCurrency: true));
            }
            csvContent.AppendLine();
        }
    }
}
