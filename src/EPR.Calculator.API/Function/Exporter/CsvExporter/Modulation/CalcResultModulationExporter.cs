using System.Text;
using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Modulation
{
    public interface ICalcResultModulationExporter
    {
        void Export(
            CalcResultLaDisposalCostData laDisposalCostData,
            SelfManagedConsumerWaste smcw,
            ModulationResult modulationResult,
            StringBuilder csvContent
        );
    }

    public class CalcResultModulationExporter : ICalcResultModulationExporter
    {
        public void Export(
            CalcResultLaDisposalCostData laDisposalCostData,
            SelfManagedConsumerWaste smcw,
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
            append("Producer Household Packaging Tonnage");
            append("Public Bin Tonnage");
            append("Household Drinks Containers Tonnage");
            append("Late Reporting Tonnage");
            append("Actioned Self-Managed Consumer Waste");
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

            foreach (var kv in modulationResult.MaterialModulation)
            {
                var material = kv.Key;
                var modulation = kv.Value;
                append(material.Name); // A

                var laDisposalCost = laDisposalCostData.ByMaterial[material.Code];
                appendd(laDisposalCost.HouseholdPackagingWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3); // B
                appendd(laDisposalCost.PublicBinTonnage              , DecimalPlaces.Three, DecimalFormats.F3); // C
                if (material.Code != MaterialCodes.Glass && laDisposalCost.HouseholdDrinkContainersTonnage == 0)
                    append(null); // D
                else
                    appendd(laDisposalCost.HouseholdDrinkContainersTonnage, DecimalPlaces.Three, DecimalFormats.F3); // D
                appendd(laDisposalCost.LateReportingTonnage, DecimalPlaces.Three, DecimalFormats.F3); // E

                appendd(smcw.OverallTotalPerMaterials[material.Code].ActionedSelfManagedConsumerWasteTonnage.total ?? 0, DecimalPlaces.Three, DecimalFormats.F3); // F

                appendd(modulation.RedMaterialTonnages + modulation.AmberMaterialTonnages + modulation.GreenMaterialTonnages, DecimalPlaces.Three, DecimalFormats.F3); // G
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
                var laDisposalCost = laDisposalCostData.Total;
                appendd(laDisposalCost.HouseholdPackagingWasteTonnage , DecimalPlaces.Three, DecimalFormats.F3); // B
                appendd(laDisposalCost.PublicBinTonnage               , DecimalPlaces.Three, DecimalFormats.F3); // C
                appendd(laDisposalCost.HouseholdDrinkContainersTonnage, DecimalPlaces.Three, DecimalFormats.F3); // D
                appendd(laDisposalCost.LateReportingTonnage           , DecimalPlaces.Three, DecimalFormats.F3); // E
                appendd(smcw.OverallTotalPerMaterials.Values.Sum(e => e.ActionedSelfManagedConsumerWasteTonnage.total), DecimalPlaces.Three, DecimalFormats.F3); // F

                var r = modulationResult.MaterialModulation.Values.Sum(m => m.RedMaterialTonnages  );
                var a = modulationResult.MaterialModulation.Values.Sum(m => m.AmberMaterialTonnages);
                var g = modulationResult.MaterialModulation.Values.Sum(m => m.GreenMaterialTonnages);

                appendd(r + a + g, DecimalPlaces.Three, DecimalFormats.F3); // G
                appendd(r        , DecimalPlaces.Three, DecimalFormats.F3); // H
                appendd(a        , DecimalPlaces.Three, DecimalFormats.F3); // I
                appendd(g        , DecimalPlaces.Three, DecimalFormats.F3); // J

                appendc(modulationResult.MaterialModulation.Values.Sum(m => m.TotalRedMaterialAtAmberDisposalCost  ), DecimalPlaces.Two, DecimalFormats.F2); // K
                appendc(modulationResult.MaterialModulation.Values.Sum(m => m.TotalGreenMaterialAtAmberDisposalCost), DecimalPlaces.Two, DecimalFormats.F2); // L

                append(null); // M
                append(null); // N
                append(null); // O
                nl();
            }
        }
    }
}
