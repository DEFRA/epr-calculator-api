using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.PartialObligations
{
    public interface ICalcResultPartialObligationsExporter
    {
        public void Export(
            RunContext runContext,
            CalcResultPartialObligations calcResultPartialObligations,
            IImmutableList<MaterialDetail> materials,
            StringBuilder stringBuilder);
    }

    public class CalcResultPartialObligationsExporter : ICalcResultPartialObligationsExporter
    {
        public void Export(RunContext runContext,
            CalcResultPartialObligations calcResultPartialObligations,
            IImmutableList<MaterialDetail> materials,
            StringBuilder stringBuilder)
        {
            // Add empty lines
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();

            // Add headers
            PreparePartialObligationsHeader(materials, stringBuilder, runContext.RequiresModulation);

            // Add data
            if (calcResultPartialObligations.PartialObligations?.Any() == true)
            {
                AppendPartialObligations(calcResultPartialObligations.PartialObligations!, stringBuilder, runContext.RequiresModulation);
            }
            else
            {
                stringBuilder.AppendLine(CsvSanitiser.SanitiseData(CalcResultPartialObligationHeaders.NoPartialObligations));
            }
        }

        private static void AppendPartialObligations(IEnumerable<CalcResultPartialObligation> partialObligations, StringBuilder csvContent, bool showModulation)
        {
            foreach (var producer in partialObligations)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerId));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.SubsidiaryId));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerName));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.TradingName));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.Level));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.SubmissionYear));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.DaysInSubmissionYear));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.JoiningDate));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.DaysObligated));
                csvContent.Append(CsvSanitiser.SanitiseData((producer.ObligatedFactor * 100).ToString("F2") + "%"));

                AppendPartialObligationTonnageByMaterial(csvContent, producer.PartialObligationTonnageByMaterial, showModulation);

                csvContent.AppendLine();
            }
        }

        private static void AppendPartialObligationTonnageByMaterial(StringBuilder csvContent, Dictionary<string, CalcResultPartialObligationTonnage> partialObligationTonnageByMaterial, bool showModulation)
        {
            void AppendRam(RAMTonnage? ram)
            {
                if (showModulation && ram != null)
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(ram.RedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                    csvContent.Append(CsvSanitiser.SanitiseData(ram.AmberTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                    csvContent.Append(CsvSanitiser.SanitiseData(ram.GreenTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                    csvContent.Append(CsvSanitiser.SanitiseData(ram.RedMedicalTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                    csvContent.Append(CsvSanitiser.SanitiseData(ram.AmberMedicalTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                    csvContent.Append(CsvSanitiser.SanitiseData(ram.GreenMedicalTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                }
            }

            foreach (var producerTonnage in partialObligationTonnageByMaterial)
            {
                var materialCode = producerTonnage.Key;
                var tonnage = producerTonnage.Value;

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.HouseholdTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                AppendRam(tonnage.HouseholdRAMTonnage);

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.PublicBinTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                AppendRam(tonnage.PublicBinRAMTonnage);

                if (materialCode == MaterialCodes.Glass)
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(tonnage.HouseholdDrinksContainersTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                    AppendRam(tonnage.HouseholdDrinksContainersRAMTonnage);
                }

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.TotalTonnage(), DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.SelfManagedConsumerWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.PartialHouseholdTonnage(), DecimalPlaces.Three, DecimalFormats.F3));
                AppendRam(tonnage.PartialHouseholdRAMTonnage());

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.PartialPublicBinTonnage(), DecimalPlaces.Three, DecimalFormats.F3));
                AppendRam(tonnage.PartialPublicBinRAMTonnage());

                if (materialCode == MaterialCodes.Glass)
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(tonnage.PartialHouseholdDrinksContainersTonnage(), DecimalPlaces.Three, DecimalFormats.F3));
                    AppendRam(tonnage.PartialHouseholdDrinksContainersRAMTonnage());
                }

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.PartialTotalTonnage(), DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.PartialSelfManagedConsumerWasteTonnage(), DecimalPlaces.Three, DecimalFormats.F3));
            }
        }

        private static void PreparePartialObligationsHeader(IReadOnlyCollection<MaterialDetail> materials, StringBuilder csvContent, bool showModulation)
        {
            // Add partial obligation producer header
            csvContent.AppendLine(CsvSanitiser.SanitiseData(CalcResultPartialObligationHeaders.PartialObligations));
            csvContent.AppendLine();

            // Add material breakdown header
            WritePartialObligationsSecondaryHeaders(GetMaterialsBreakdownHeader(materials, showModulation), csvContent);

            // Add column header
            WritePartialObligationsColumnHeaders(GetColumnHeaders(materials, showModulation), csvContent);
            csvContent.AppendLine();
        }

        private static void WritePartialObligationsSecondaryHeaders(IReadOnlyCollection<CalcResultPartialObligationHeader> headers, StringBuilder csvContent)
        {
            var maxColumnSize = headers.MaxBy(h => h.ColumnIndex ?? 0)?.ColumnIndex ?? throw new ArgumentException("No headers specified");

            var headerRows = new string[maxColumnSize];
            foreach (var item in headers.Where(h => h.ColumnIndex.HasValue))
            {
                headerRows[item.ColumnIndex!.Value - 1] = CsvSanitiser.SanitiseData(item.Name, false);
            }

            var headerRow = string.Join(CommonConstants.CsvFileDelimiter, headerRows);
            csvContent.AppendLine(headerRow);
        }

        private static void WritePartialObligationsColumnHeaders(IEnumerable<CalcResultPartialObligationHeader> columnHeaders, StringBuilder csvContent)
        {
            foreach (var item in columnHeaders)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(item.Name));
            }
        }

        public static ImmutableList<CalcResultPartialObligationHeader> GetMaterialsBreakdownHeader(IReadOnlyCollection<MaterialDetail> materials, bool showModulation)
        {
            var materialsBreakdownHeaders = ImmutableList.CreateBuilder<CalcResultPartialObligationHeader>();
            var columnIndex = GetInitialHeaders().Count + 1;

            foreach (var material in materials)
            {
                materialsBreakdownHeaders.Add(new CalcResultPartialObligationHeader
                {
                    Name = $"{material.Name} Breakdown",
                    ColumnIndex = columnIndex,
                });

                columnIndex = columnIndex + GetMaterialHeaders(isGlass: material.Code == MaterialCodes.Glass, showModulation).Count;
            }

            return materialsBreakdownHeaders.ToImmutable();
        }

        public static ImmutableList<CalcResultPartialObligationHeader> GetColumnHeaders(IReadOnlyCollection<MaterialDetail> materials, bool showModulation)
        {
            var columnHeaders = ImmutableList.CreateBuilder<CalcResultPartialObligationHeader>();

            columnHeaders.AddRange(GetInitialHeaders());

            foreach (var material in materials.Select(m => m.Code))
            {
                columnHeaders.AddRange(GetMaterialHeaders(isGlass: material == MaterialCodes.Glass, showModulation));
            }

            return columnHeaders.ToImmutable();
        }

        private static ImmutableList<CalcResultPartialObligationHeader> GetInitialHeaders()
        {
            return [
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.ProducerId },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.SubsidiaryId },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.ProducerOrSubsidiaryName },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.TradingName },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.Level },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.SubmissionYear },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.DaysInSubmissionYear },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.JoiningDate },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.ObligatedDays },
                new CalcResultPartialObligationHeader { Name = CalcResultPartialObligationHeaders.ObligatedPercentage },
            ];
        }
        private static ImmutableList<CalcResultPartialObligationHeader> GetMaterialHeaders(bool isGlass, bool showModulation)
        {
            var columns = ImmutableList.CreateBuilder<CalcResultPartialObligationHeader>();

            void Add(string name) => columns.Add(new CalcResultPartialObligationHeader { Name = name });

            void AddRange(params string[] names)
            {
                foreach (var name in names)
                    Add(name);
            }

            void AddRangeIf(bool condition, params string[] names)
            {
                if (!condition) return;
                AddRange(names);
            }

            Add(CalcResultPartialObligationHeaders.HouseholdPackagingWasteTonnage);
            AddRangeIf(showModulation,
                CalcResultPartialObligationHeaders.HouseholdRedTonnage,
                CalcResultPartialObligationHeaders.HouseholdAmberTonnage,
                CalcResultPartialObligationHeaders.HouseholdGreenTonnage,
                CalcResultPartialObligationHeaders.HouseholdRedMedicalTonnage,
                CalcResultPartialObligationHeaders.HouseholdAmberMedicalTonnage,
                CalcResultPartialObligationHeaders.HouseholdGreenMedicalTonnage
            );
            Add(CalcResultPartialObligationHeaders.PublicBinTonnage);
            AddRangeIf(showModulation,
                CalcResultPartialObligationHeaders.PublicBinRedTonnage,
                CalcResultPartialObligationHeaders.PublicBinAmberTonnage,
                CalcResultPartialObligationHeaders.PublicBinGreenTonnage,
                CalcResultPartialObligationHeaders.PublicBinRedMedicalTonnage,
                CalcResultPartialObligationHeaders.PublicBinAmberMedicalTonnage,
                CalcResultPartialObligationHeaders.PublicBinGreenMedicalTonnage
            );
            if (isGlass) {
                Add(CalcResultPartialObligationHeaders.HouseholdDrinksContainersTonnage);
                AddRangeIf(showModulation,
                    CalcResultPartialObligationHeaders.HouseholdDrinksContainersRedTonnage,
                    CalcResultPartialObligationHeaders.HouseholdDrinksContainersAmberTonnage,
                    CalcResultPartialObligationHeaders.HouseholdDrinksContainersGreenTonnage,
                    CalcResultPartialObligationHeaders.HouseholdDrinksContainersRedMedicalTonnage,
                    CalcResultPartialObligationHeaders.HouseholdDrinksContainersAmberMedicalTonnage,
                    CalcResultPartialObligationHeaders.HouseholdDrinksContainersGreenMedicalTonnage
                );
            }
            AddRange(
                CalcResultPartialObligationHeaders.TotalTonnage,
                CalcResultPartialObligationHeaders.SelfManagedConsumerWasteTonnage,
                CalcResultPartialObligationHeaders.PartialHouseholdPackagingWasteTonnage
            );
            AddRangeIf(showModulation,
                CalcResultPartialObligationHeaders.PartialHouseholdRedTonnage,
                CalcResultPartialObligationHeaders.PartialHouseholdAmberTonnage,
                CalcResultPartialObligationHeaders.PartialHouseholdGreenTonnage,
                CalcResultPartialObligationHeaders.PartialHouseholdRedMedicalTonnage,
                CalcResultPartialObligationHeaders.PartialHouseholdAmberMedicalTonnage,
                CalcResultPartialObligationHeaders.PartialHouseholdGreenMedicalTonnage
            );
            Add(CalcResultPartialObligationHeaders.PartialPublicBinTonnage);
            AddRangeIf(showModulation,
                CalcResultPartialObligationHeaders.PartialPublicBinRedTonnage,
                CalcResultPartialObligationHeaders.PartialPublicBinAmberTonnage,
                CalcResultPartialObligationHeaders.PartialPublicBinGreenTonnage,
                CalcResultPartialObligationHeaders.PartialPublicBinRedMedicalTonnage,
                CalcResultPartialObligationHeaders.PartialPublicBinAmberMedicalTonnage,
                CalcResultPartialObligationHeaders.PartialPublicBinGreenMedicalTonnage
            );
            if (isGlass) {
                Add(CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersTonnage);
                AddRangeIf(showModulation,
                    CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersRedTonnage,
                    CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersAmberTonnage,
                    CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersGreenTonnage,
                    CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersRedMedicalTonnage,
                    CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersAmberMedicalTonnage,
                    CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersGreenMedicalTonnage
                );
            }
            AddRange(
                CalcResultPartialObligationHeaders.PartialTotalTonnage,
                CalcResultPartialObligationHeaders.PartialSelfManagedConsumerWasteTonnage
            );

            return columns.ToImmutable();
        }
    }
}
