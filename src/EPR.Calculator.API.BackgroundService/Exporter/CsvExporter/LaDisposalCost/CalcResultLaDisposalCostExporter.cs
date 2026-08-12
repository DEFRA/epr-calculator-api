using System.Text;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.BackgroundService.Constants;
using EPR.Calculator.API.BackgroundService.Enums;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.BackgroundService.Misc;
using EPR.Calculator.API.BackgroundService.Models;

namespace EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.LaDisposalCost
{
    public interface ICalcResultLaDisposalCostExporter
    {
        void Export(
            RunContext runContext,
            CalcResultLaDisposalCostData calcResultLaDisposalCostData,
            IImmutableList<MaterialDetail> materialDetails,
            StringBuilder csvContent
        );
    }

    public class CalcResultLaDisposalCostExporter : ICalcResultLaDisposalCostExporter
    {
        public void Export(
            RunContext runContext,
            CalcResultLaDisposalCostData calcResultLaDisposalCostData,
            IImmutableList<MaterialDetail> materialDetails,
            StringBuilder csvContent
        )
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(CsvSanitiser.SanitiseData("LA Disposal Cost Data"));

            AppendHeader(runContext.RequiresModulation, csvContent);
            foreach (var disposalCostData in calcResultLaDisposalCostData.ByMaterial)
            {
                var material = materialDetails.First(m => m.Code == disposalCostData.Key);
                AppendRow(runContext.RequiresModulation, material.Name, disposalCostData.Value, csvContent);
            }
            AppendRow(runContext.RequiresModulation, "Total", calcResultLaDisposalCostData.Total, csvContent);
        }

        private static void AppendHeader(bool applyModulation, StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Material));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.England));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Wales));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Scotland));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.NorthernIreland));
            csvContent.Append(CsvSanitiser.SanitiseData("Total UK"));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.ProducerReportedHouseholdPackagingWasteTonnage));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.ReportedPublicBinTonnage));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.HouseholdDrinkContainers));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.LateReportingTonnage));
            if (applyModulation)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.ActionedSelfManagedConsumerWasteTonnage));
                csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.ModulatedProducerReportedTotalTonnage));
            }
            else
            {
                csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.ProducerReportedTotalTonnage));
            }
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.DisposalCostPricePerTonne));
            csvContent.AppendLine();
        }

        private static void AppendRow(bool applyModulation, string name, CalcResultLaDisposalCostDataDetail data, StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(name));
            csvContent.Append(CsvSanitiser.SanitiseData(data.Cost.England        , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(data.Cost.Wales          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(data.Cost.Scotland       , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(data.Cost.NorthernIreland, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(data.Cost.Total          , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(data.HouseholdPackagingWasteTonnage , DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(data.PublicBinTonnage               , DecimalPlaces.Three, DecimalFormats.F3));
            if (name != "Total" && name != MaterialNames.Glass && data.HouseholdDrinkContainersTonnage == 0)
            {
                csvContent.Append(CsvSanitiser.SanitiseData((string?)null));
            }
            else
            {
                csvContent.Append(CsvSanitiser.SanitiseData(data.HouseholdDrinkContainersTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            }
            csvContent.Append(CsvSanitiser.SanitiseData(data.LateReportingTonnage           , DecimalPlaces.Three, DecimalFormats.F3));
            if (applyModulation)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(data.ActionedSelfManagedConsumerWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            }
            csvContent.Append(CsvSanitiser.SanitiseData(data.TotalTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            if (name != "Total")
            {
                csvContent.Append(CsvSanitiser.SanitiseData(data.DisposalCostPricePerTonne, DecimalPlaces.Four, DecimalFormats.F4, isCurrency: true));
            }
            csvContent.AppendLine();
        }
    }
}
