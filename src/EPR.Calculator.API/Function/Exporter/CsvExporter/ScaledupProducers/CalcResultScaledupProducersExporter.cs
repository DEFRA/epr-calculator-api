using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers
{
    public interface ICalcResultScaledupProducersExporter
    {
        public void Export(
            CalcResultScaledupProducers calcResultScaledupProducers,
            IImmutableList<MaterialDetail> materials,
            bool showTotal,
            StringBuilder stringBuilder
        );
    }

    public class CalcResultScaledupProducersExporter : ICalcResultScaledupProducersExporter
    {
        private const int MaterialsBreakdownHeaderInitialColumnIndex = 10;
        private const int MaterialsBreakdownHeaderIncrementalColumnIndex = 10;


        public void Export(
            CalcResultScaledupProducers calcResultScaledupProducers,
            IImmutableList<MaterialDetail> materials,
            bool showTotal,
            StringBuilder stringBuilder
        )
        {
            // Add empty lines
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();

            // Add headers
            PrepareScaledupProducersHeader(materials, stringBuilder);

            // Add data
            if (calcResultScaledupProducers.ScaledupProducers?.Any() == true)
            {
                foreach (var producer in calcResultScaledupProducers.ScaledupProducers)
                    producer.ScaledupProducerTonnageByMaterial = GetTonnages(producer.PomData, materials);

                AppendScaledupProducers(calcResultScaledupProducers.ScaledupProducers, stringBuilder);
                if (showTotal)
                {
                    AppendOverallTotalRow(GetOverallTotalRow(calcResultScaledupProducers.ScaledupProducers, materials), stringBuilder);
                }
            }
            else
            {
                stringBuilder.AppendLine(CsvSanitiser.SanitiseData(CalcResultScaledupProducerHeaders.NoScaledupProducers));
            }
        }

        private static void AppendScaledupProducers(IEnumerable<CalcResultScaledupProducer> producers, StringBuilder csvContent)
        {
            foreach (var producer in producers)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerId));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.SubsidiaryId));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerName));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.TradingName));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.Level));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.SubmissionPeriodCode));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.DaysInSubmissionPeriod != -1 ? producer.DaysInSubmissionPeriod.ToString() : string.Empty));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.DaysInWholePeriod != -1 ? producer.DaysInWholePeriod.ToString() : string.Empty));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ScaleupFactor == -1 ? CommonConstants.Totals : producer.ScaleupFactor.ToString()));

                AppendScaledupProducerTonnageByMaterial(csvContent, producer);

                csvContent.AppendLine();
            }
        }

        private static void AppendOverallTotalRow(CalcResultScaledupProducer totalProducer, StringBuilder csvContent)
        {
            csvContent.Append(new string(CommonConstants.CsvFileDelimiter[0], 8));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Totals));
            AppendScaledupProducerTonnageByMaterial(csvContent, totalProducer);
            csvContent.AppendLine();
        }

        private static void AppendScaledupProducerTonnageByMaterial(StringBuilder csvContent, CalcResultScaledupProducer producer)
        {
            foreach (var producerTonnage in producer.ScaledupProducerTonnageByMaterial)
            {
                var materialCode = producerTonnage.Key;
                var tonnage = producerTonnage.Value;

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ReportedHouseholdPackagingWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ReportedPublicBinTonnage, DecimalPlaces.Three, DecimalFormats.F3));

                if (materialCode == MaterialCodes.Glass || materialCode == MaterialNames.Glass)
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(tonnage.HouseholdDrinksContainersTonnageGlass, DecimalPlaces.Three, DecimalFormats.F3));
                }

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.TotalReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ReportedSelfManagedConsumerWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.NetReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ScaledupReportedHouseholdPackagingWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ScaledupReportedPublicBinTonnage, DecimalPlaces.Three, DecimalFormats.F3));

                if (materialCode == MaterialCodes.Glass || materialCode == MaterialNames.Glass)
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ScaledupHouseholdDrinksContainersTonnageGlass, DecimalPlaces.Three, DecimalFormats.F3));
                }

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ScaledupTotalReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ScaledupReportedSelfManagedConsumerWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ScaledupNetReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            }
        }

        public static CalcResultScaledupProducer GetOverallTotalRow(
            IEnumerable<CalcResultScaledupProducer> producers,
            IEnumerable<MaterialDetail> materials
        )
        {
            var overallTotalRow = new CalcResultScaledupProducer
            {
                ScaledupProducerTonnageByMaterial = new Dictionary<string, CalcResultScaledupProducerTonnage>(),
            };

            var allMaterialDict = producers.Where(x => !x.IsSubtotalRow).Select(x => x.ScaledupProducerTonnageByMaterial);
            foreach (var material in materials)
            {
                var totalRow = new CalcResultScaledupProducerTonnage();
                var materialValues = allMaterialDict.Where(x => x.ContainsKey(material.Code)).Select(x => x[material.Code]).ToList();
                totalRow.ReportedHouseholdPackagingWasteTonnage = materialValues.Sum(x => x.ReportedHouseholdPackagingWasteTonnage);
                totalRow.ReportedPublicBinTonnage = materialValues.Sum(x => x.ReportedPublicBinTonnage);
                if (material.Code == MaterialCodes.Glass)
                {
                    totalRow.HouseholdDrinksContainersTonnageGlass = materialValues.Sum(x => x.HouseholdDrinksContainersTonnageGlass);
                }

                totalRow.TotalReportedTonnage = materialValues.Sum(x => x.TotalReportedTonnage);
                totalRow.ReportedSelfManagedConsumerWasteTonnage = materialValues.Sum(x => x.ReportedSelfManagedConsumerWasteTonnage);
                totalRow.NetReportedTonnage = materialValues.Sum(x => x.NetReportedTonnage);
                totalRow.ScaledupReportedHouseholdPackagingWasteTonnage = materialValues.Sum(x => x.ScaledupReportedHouseholdPackagingWasteTonnage);
                totalRow.ScaledupReportedPublicBinTonnage = materialValues.Sum(x => x.ScaledupReportedPublicBinTonnage);
                if (material.Code == MaterialCodes.Glass)
                {
                    totalRow.ScaledupHouseholdDrinksContainersTonnageGlass = materialValues.Sum(x => x.ScaledupHouseholdDrinksContainersTonnageGlass);
                }

                totalRow.ScaledupTotalReportedTonnage = materialValues.Sum(x => x.ScaledupTotalReportedTonnage);
                totalRow.ScaledupReportedSelfManagedConsumerWasteTonnage = materialValues.Sum(x => x.ScaledupReportedSelfManagedConsumerWasteTonnage);
                totalRow.ScaledupNetReportedTonnage = materialValues.Sum(x => x.ScaledupNetReportedTonnage);

                overallTotalRow.ScaledupProducerTonnageByMaterial.Add(material.Name, totalRow);
            }

            return overallTotalRow;
        }

        private static void PrepareScaledupProducersHeader(IImmutableList<MaterialDetail> materials, StringBuilder csvContent)
        {
            // Add scaledup producer header
            csvContent.AppendLine(CsvSanitiser.SanitiseData(CalcResultScaledupProducerHeaders.ScaledupProducers));
            csvContent.AppendLine();

            // Add material breakdown header
            WriteScaledupProducersSecondaryHeaders(GetMaterialsBreakdownHeader(materials), csvContent);

            // Add column header
            WriteScaledupProducersColumnHeaders(GetColumnHeaders(materials), csvContent);
            csvContent.AppendLine();
        }

        private static void WriteScaledupProducersSecondaryHeaders(IEnumerable<CalcResultScaledupProducerHeader> headers, StringBuilder csvContent)
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

        private static void WriteScaledupProducersColumnHeaders(IEnumerable<CalcResultScaledupProducerHeader> columnHeaders, StringBuilder csvContent)
        {
            foreach (var item in columnHeaders)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(item.Name));
            }
        }

        public static ImmutableList<CalcResultScaledupProducerHeader> GetMaterialsBreakdownHeader(IEnumerable<MaterialDetail> materials)
        {
            var materialsBreakdownHeaders = ImmutableList.CreateBuilder<CalcResultScaledupProducerHeader>();
            var columnIndex = MaterialsBreakdownHeaderInitialColumnIndex;

            materialsBreakdownHeaders.Add(new CalcResultScaledupProducerHeader
            {
                Name = CalcResultScaledupProducerHeaders.EachSubmissionForTheYear,
                ColumnIndex = 1,
            });

            foreach (var material in materials)
            {
                materialsBreakdownHeaders.Add(new CalcResultScaledupProducerHeader
                {
                    Name = $"{material.Name} Breakdown",
                    ColumnIndex = columnIndex,
                });

                columnIndex = material.Code == MaterialCodes.Glass
                    ? columnIndex + MaterialsBreakdownHeaderIncrementalColumnIndex + 2
                    : columnIndex + MaterialsBreakdownHeaderIncrementalColumnIndex;
            }

            return materialsBreakdownHeaders.ToImmutable();
        }

        public static ImmutableList<CalcResultScaledupProducerHeader> GetColumnHeaders(IEnumerable<MaterialDetail> materials)
        {
            var columnHeaders = ImmutableList.CreateBuilder<CalcResultScaledupProducerHeader>();

            columnHeaders.AddRange([
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ProducerId },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.SubsidiaryId },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ProducerOrSubsidiaryName },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.TradingName },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.Level },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.SubmissionPeriodCode },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.DaysInSubmissionPeriod },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.DaysInWholePeriod },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaleupFactor }
            ]);

            foreach (var material in materials)
            {
                var columnHeadersList = new List<CalcResultScaledupProducerHeader>
                {
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.HouseholdPackagingWasteTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.PublicBinTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.TotalTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.SelfManagedConsumerWasteTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.NetTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupHouseholdPackagingWasteTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupPublicBinTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupTotalTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupSelfManagedConsumerWasteTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupNetTonnage },
                };

                if (material.Code == MaterialCodes.Glass)
                {
                    columnHeadersList.Insert(2, new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.HouseholdDrinksContainersTonnageGlass });
                    columnHeadersList.Insert(8, new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupHouseholdDrinksContainersTonnageGlass });
                }

                columnHeaders.AddRange(columnHeadersList);
            }

            return columnHeaders.ToImmutable();
        }

        public static Dictionary<string, CalcResultScaledupProducerTonnage> GetTonnages(
            IReadOnlyList<ScaledupPomEntry> pomData,
            IEnumerable<MaterialDetail> materials
        )
        {
            var scaledupProducerTonnages = new Dictionary<string, CalcResultScaledupProducerTonnage>();

            foreach (var material in materials)
            {
                var scaledupProducerTonnage = new CalcResultScaledupProducerTonnage();
                var materialPomData = pomData.Where(e => e.MaterialId == material.Id);

                var hh  = materialPomData.SingleOrDefault(e => e.PackagingType == PackagingTypes.Household);
                var pb  = materialPomData.SingleOrDefault(e => e.PackagingType == PackagingTypes.PublicBin);
                var cw  = materialPomData.SingleOrDefault(e => e.PackagingType == PackagingTypes.ConsumerWaste);

                scaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage          = hh?.Tonnage       ?? 0;
                scaledupProducerTonnage.ScaledupReportedHouseholdPackagingWasteTonnage  = hh?.ScaledTonnage ?? 0;
                scaledupProducerTonnage.ReportedPublicBinTonnage                        = pb?.Tonnage       ?? 0;
                scaledupProducerTonnage.ScaledupReportedPublicBinTonnage                = pb?.ScaledTonnage ?? 0;
                scaledupProducerTonnage.ReportedSelfManagedConsumerWasteTonnage         = cw?.Tonnage       ?? 0;
                scaledupProducerTonnage.ScaledupReportedSelfManagedConsumerWasteTonnage = cw?.ScaledTonnage ?? 0;

                if (material.Code == MaterialCodes.Glass)
                {
                    var hdc = materialPomData.SingleOrDefault(e => e.PackagingType == PackagingTypes.HouseholdDrinksContainers);
                    scaledupProducerTonnage.HouseholdDrinksContainersTonnageGlass          = hdc?.Tonnage       ?? 0;
                    scaledupProducerTonnage.ScaledupHouseholdDrinksContainersTonnageGlass  = hdc?.ScaledTonnage ?? 0;

                    scaledupProducerTonnage.TotalReportedTonnage = scaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage +
                        scaledupProducerTonnage.ReportedPublicBinTonnage + scaledupProducerTonnage.HouseholdDrinksContainersTonnageGlass;
                    scaledupProducerTonnage.ScaledupTotalReportedTonnage = scaledupProducerTonnage.ScaledupReportedHouseholdPackagingWasteTonnage +
                        scaledupProducerTonnage.ScaledupReportedPublicBinTonnage + scaledupProducerTonnage.ScaledupHouseholdDrinksContainersTonnageGlass;
                }
                else
                {
                    scaledupProducerTonnage.TotalReportedTonnage = scaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage +
                        scaledupProducerTonnage.ReportedPublicBinTonnage;
                    scaledupProducerTonnage.ScaledupTotalReportedTonnage = scaledupProducerTonnage.ScaledupReportedHouseholdPackagingWasteTonnage +
                        scaledupProducerTonnage.ScaledupReportedPublicBinTonnage;
                }

                scaledupProducerTonnage.NetReportedTonnage = scaledupProducerTonnage.TotalReportedTonnage -
                    scaledupProducerTonnage.ReportedSelfManagedConsumerWasteTonnage;
                scaledupProducerTonnage.ScaledupNetReportedTonnage = scaledupProducerTonnage.ScaledupTotalReportedTonnage -
                    scaledupProducerTonnage.ScaledupReportedSelfManagedConsumerWasteTonnage;

                scaledupProducerTonnages.Add(material.Code, scaledupProducerTonnage);
            }

            return scaledupProducerTonnages;
        }
    }
}
