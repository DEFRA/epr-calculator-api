using System.Text;
using EPR.Calculator.API.BackgroundService.Constants;
using EPR.Calculator.API.BackgroundService.Enums;
using EPR.Calculator.API.BackgroundService.Misc;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Modulation
{
    public interface ICalcResultModulationExporter
    {
        void Export(
            CalcResultLaDisposalCostData laDisposalCostData,
            ModulationResult modulationResult,
            StringBuilder csvContent
        );
    }

    public class CalcResultModulationExporter : ICalcResultModulationExporter
    {
        [ActivityTrace]
        public void Export(
            CalcResultLaDisposalCostData laDisposalCostData,
            ModulationResult modulationResult,
            StringBuilder csvContent
        )
        {
            void nl()
            {
                csvContent.AppendLine();
            }
            void append(string? s)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(s));
            }

            void appendd(decimal? d, DecimalPlaces dp, DecimalFormats df)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(d, dp, df));
            }
            void appendc(decimal? d, DecimalPlaces dp, DecimalFormats df)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(d, dp, df, true));
            }

            nl();
            nl();

            append("Modulation Calculation");
            nl();

            append("Red Modulation Factor");
            appendd(modulationResult.RedFactor, DecimalPlaces.Three, DecimalFormats.F3);
            nl();

            append("Green Modulation Factor");
            appendd(modulationResult.GreenFactor, DecimalPlaces.Six, DecimalFormats.F6);
            nl();

            append("Material");
            append("Household Tonnage");
            append("Public Bin Tonnage");
            append("Household Drinks Containers Tonnage");
            append("Late Reporting Tonnage");
            append("Actioned Self-managed Consumer Waste Tonnage");
            append("Net Tonnage + Late Reporting Tonnage");
            append("Red + Red Medical Net Tonnage + Late Reporting Tonnage");
            append("Amber + Amber Medical Net Tonnage + Late Reporting Tonnage");
            append("Green + Green Medical Net Tonnage + Late Reporting Tonnage");
            append("Total Red Material at Amber Disposal Cost = Amber Material Disposal Cost x Red Material Tonnage");
            append("Total Green Material at Amber Disposal Cost = Amber Material Disposal Cost x Green Material Tonnage");
            append("Red Material Disposal Cost = Red Modulation Factor * Amber Material Disposal Cost");
            append("Amber Material Disposal Cost = Material Disposal Cost per Tonne");
            append("Green Material Disposal Cost = Green Modulation Factor * Amber Material Disposal Cost");
            nl();

            foreach (var kv in modulationResult.ModulationByMaterial)
            {
                var material = kv.Key;
                var modulation = kv.Value;
                append(material.Name); // A

                var laDisposalMaterial = laDisposalCostData.ByMaterial[material.Code];
                appendd(laDisposalMaterial.HouseholdPackagingWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3); // B
                appendd(laDisposalMaterial.PublicBinTonnage              , DecimalPlaces.Three, DecimalFormats.F3); // C
                if (material.Code != MaterialCodes.Glass && laDisposalMaterial.HouseholdDrinkContainersTonnage == 0)
                    append(null); // D
                else
                    appendd(laDisposalMaterial.HouseholdDrinkContainersTonnage, DecimalPlaces.Three, DecimalFormats.F3); // D
                appendd(laDisposalMaterial.LateReportingTonnage, DecimalPlaces.Three, DecimalFormats.F3); // E

                appendd(laDisposalMaterial.ActionedSelfManagedConsumerWasteTonnage ?? 0, DecimalPlaces.Three, DecimalFormats.F3); // F

                appendd(modulation.TotalMaterialTonnages, DecimalPlaces.Three, DecimalFormats.F3); // G
                appendd(modulation.RedMaterialTonnages  , DecimalPlaces.Three, DecimalFormats.F3); // H
                appendd(modulation.AmberMaterialTonnages, DecimalPlaces.Three, DecimalFormats.F3); // I
                appendd(modulation.GreenMaterialTonnages, DecimalPlaces.Three, DecimalFormats.F3); // J

                appendc(modulation.TotalRedMaterialAtAmberDisposalCost, DecimalPlaces.Two, DecimalFormats.F2); // K
                appendc(modulation.TotalGreenMaterialAtAmberDisposalCost, DecimalPlaces.Two, DecimalFormats.F2); // L

                appendc(modulation.RedMaterialDisposalCost  , DecimalPlaces.Four, DecimalFormats.F4); // M
                appendc(modulation.AmberMaterialDisposalCost, DecimalPlaces.Four, DecimalFormats.F4); // N
                appendc(modulation.GreenMaterialDisposalCost, DecimalPlaces.Four, DecimalFormats.F4); // O
                nl();
            }

            {
                append(CommonConstants.Total); // A
                var laDisposalTotal = laDisposalCostData.Total;
                appendd(laDisposalTotal.HouseholdPackagingWasteTonnage , DecimalPlaces.Three, DecimalFormats.F3); // B
                appendd(laDisposalTotal.PublicBinTonnage               , DecimalPlaces.Three, DecimalFormats.F3); // C
                appendd(laDisposalTotal.HouseholdDrinkContainersTonnage, DecimalPlaces.Three, DecimalFormats.F3); // D
                appendd(laDisposalTotal.LateReportingTonnage           , DecimalPlaces.Three, DecimalFormats.F3); // E
                appendd(laDisposalTotal.ActionedSelfManagedConsumerWasteTonnage ?? 0         , DecimalPlaces.Three, DecimalFormats.F3); // F

                var total = modulationResult.Total;
                appendd(total.TotalMaterialTonnages, DecimalPlaces.Three, DecimalFormats.F3); // G
                appendd(total.RedMaterialTonnages  , DecimalPlaces.Three, DecimalFormats.F3); // H
                appendd(total.AmberMaterialTonnages, DecimalPlaces.Three, DecimalFormats.F3); // I
                appendd(total.GreenMaterialTonnages, DecimalPlaces.Three, DecimalFormats.F3); // J

                appendc(total.TotalRedMaterialAtAmberDisposalCost  , DecimalPlaces.Two, DecimalFormats.F2); // K
                appendc(total.TotalGreenMaterialAtAmberDisposalCost, DecimalPlaces.Two, DecimalFormats.F2); // L

                append(null); // M
                append(null); // N
                append(null); // O
                nl();
            }
        }
    }
}
