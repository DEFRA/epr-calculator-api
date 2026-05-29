using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.ProjectedProducers
{
    public static class H1ProjectedProducersExporterUtils
    {
        public static void AppendProjectedProducers(IImmutableList<CalcResultH1ProjectedProducer> h1ProjectedProducers, StringBuilder csvContent)
        {
            foreach (var producer in h1ProjectedProducers)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerId));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.SubsidiaryId));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.Level));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.SubmissionPeriodCode));

                AppendProjectedProducerTonnageByMaterial(csvContent, producer.H1ProjectedTonnageByMaterial);

                csvContent.AppendLine();
            }
        }

        private static void AppendProjectedProducerTonnageByMaterial(StringBuilder csvContent, IReadOnlyDictionary<string, CalcResultH1ProjectedProducerMaterialTonnage> h1ProjectedProducerTonnageByMaterial)
        {
            foreach (var producerTonnage in h1ProjectedProducerTonnageByMaterial)
            {
                AppendMaterialTonnage(csvContent, producerTonnage.Key, producerTonnage.Value);
            }
        }

        private static void AppendMaterialTonnage(StringBuilder csvContent, string materialCode, CalcResultH1ProjectedProducerMaterialTonnage tonnage)
        {
            string GetProportionPercentage(decimal proportion) {
                var showProportion = tonnage.IsWithoutRamTonnage() && tonnage.H2RamProportions.AnyProportions();
                return showProportion ? CsvSanitiser.SanitiseData(proportion * 100, DecimalPlaces.Two, DecimalFormats.F2, isPercentage: true) : CsvSanitiser.SanitiseData(CommonConstants.Hyphen);
            }

            csvContent.Append(CsvSanitiser.SanitiseData(tonnage.HouseholdTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            AppendRamTonnage(csvContent, tonnage.HouseholdRAMTonnage);
            csvContent.Append(CsvSanitiser.SanitiseData(tonnage.HouseholdTonnageWithoutRAM, DecimalPlaces.Three, DecimalFormats.F3));

            csvContent.Append(CsvSanitiser.SanitiseData(tonnage.PublicBinTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            AppendRamTonnage(csvContent, tonnage.PublicBinRAMTonnage);
            csvContent.Append(CsvSanitiser.SanitiseData(tonnage.PublicBinTonnageWithoutRAM, DecimalPlaces.Three, DecimalFormats.F3));

            if (materialCode == MaterialCodes.Glass)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.HouseholdDrinksContainerTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                AppendRamTonnage(csvContent, tonnage.HouseholdDrinksContainerRAMTonnage!);
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.HouseholdDrinksContainerTonnageWithoutRAM, DecimalPlaces.Three, DecimalFormats.F3));
            }

            csvContent.Append(GetProportionPercentage(tonnage.H2RamProportions.Red));
            csvContent.Append(GetProportionPercentage(tonnage.H2RamProportions.Amber));
            csvContent.Append(GetProportionPercentage(tonnage.H2RamProportions.Green));
            csvContent.Append(GetProportionPercentage(tonnage.H2RamProportions.RedMedical));
            csvContent.Append(GetProportionPercentage(tonnage.H2RamProportions.AmberMedical));
            csvContent.Append(GetProportionPercentage(tonnage.H2RamProportions.GreenMedical));

            csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ProjectedHouseholdTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            AppendRamTonnage(csvContent, tonnage.ProjectedHouseholdRAMTonnage);
            csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ProjectedPublicBinTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            AppendRamTonnage(csvContent, tonnage.ProjectedPublicBinRAMTonnage);

            if (materialCode == MaterialCodes.Glass)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ProjectedHouseholdDrinksContainerTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                AppendRamTonnage(csvContent, tonnage.ProjectedHouseholdDrinksContainerRAMTonnage!);
            }
        }

        private static void AppendRamTonnage(StringBuilder csvContent, RAMTonnage tonnage)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(tonnage.RedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(tonnage.AmberTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(tonnage.GreenTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(tonnage.RedMedicalTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(tonnage.AmberMedicalTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(tonnage.GreenMedicalTonnage, DecimalPlaces.Three, DecimalFormats.F3));
        }

        public static ProjectedProducersHeaders GetProjectedProducerHeaders(IImmutableList<MaterialDetail> materials)
        {
            return new ProjectedProducersHeaders {
                TitleHeader = new ProjectedProducersHeader
                {
                    Name = CalcResultProjectedProducersHeaders.H1ProjectedProducers,
                    ColumnIndex = 1,
                },
                MaterialBreakdownHeaders = GetMaterialsBreakdownHeader(materials),
                ColumnHeaders = GetColumnHeaders(materials)
            };
        }

        private static List<ProjectedProducersHeader> GetMaterialsBreakdownHeader(IImmutableList<MaterialDetail> materials)
        {
            var materialsBreakdownHeaders = new List<ProjectedProducersHeader>();
            var breakdownColumnIndex = GetInitialColumnHeaders().Count + 1;
            var materialHeaderCount = GetMaterialColumnHeaders().Count + GetH2ProportionHeaders().Count;
            var glassHeaderCount = GetGlassColumnHeaders().Count;
            var projectedMaterialHeaderCount = GetProjectedMaterialColumnHeaders().Count;
            var projectedGlassHeaderCount = GetProjectedGlassColumnHeaders().Count;

            foreach (var material in materials)
            {
                var header = $"{material.Name} Breakdown";
                materialsBreakdownHeaders.Add(new ProjectedProducersHeader
                {
                    Name = header,
                    ColumnIndex = breakdownColumnIndex
                });

                var projectedColumnIndex = 0;

                if(material.Code == MaterialCodes.Glass)
                {
                    var breakdownHeaderCount = materialHeaderCount + glassHeaderCount;
                    projectedColumnIndex = breakdownColumnIndex + breakdownHeaderCount;
                    breakdownColumnIndex += breakdownHeaderCount + projectedMaterialHeaderCount + projectedGlassHeaderCount;
                } else {
                    var breakdownHeaderCount = materialHeaderCount;
                    projectedColumnIndex = breakdownColumnIndex + breakdownHeaderCount;
                    breakdownColumnIndex += breakdownHeaderCount + projectedMaterialHeaderCount;
                }

                materialsBreakdownHeaders.Add(new ProjectedProducersHeader
                {
                    Name = $"Projected {header}",
                    ColumnIndex = projectedColumnIndex
                });
            }

            return materialsBreakdownHeaders;
        }

        private static List<ProjectedProducersHeader> GetColumnHeaders(IImmutableList<MaterialDetail> materials)
        {
            var columnHeaders = new List<ProjectedProducersHeader>();

            columnHeaders.AddRange(GetInitialColumnHeaders());

            foreach (var material in materials.Select(m => m.Code))
            {
                columnHeaders.AddRange(GetMaterialColumnHeaders());

                if (material == MaterialCodes.Glass)
                {
                    columnHeaders.AddRange(GetGlassColumnHeaders());
                }

                columnHeaders.AddRange(GetH2ProportionHeaders().Concat(GetProjectedMaterialColumnHeaders()));

                if (material == MaterialCodes.Glass)
                {
                    columnHeaders.AddRange(GetProjectedGlassColumnHeaders());
                }
            }

            return columnHeaders;
        }

        private static List<ProjectedProducersHeader> GetInitialColumnHeaders()
        {
            return new List<ProjectedProducersHeader>
            {
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.ProducerId },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.SubsidiaryId },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.Level },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.SubmissionPeriodCode }
            };
        }

        private static List<ProjectedProducersHeader> GetMaterialColumnHeaders()
        {
            return new List<ProjectedProducersHeader>
            {
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdPackagingTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdRedTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdAmberTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdGreenTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdRedMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdAmberMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdGreenMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdTonnageWithoutRAM },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinPackagingTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinRedTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinAmberTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinGreenTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinRedMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinAmberMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinGreenMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinTonnageWithoutRAM },

            };
        }

        private static List<ProjectedProducersHeader> GetH2ProportionHeaders()
        {
            return new List<ProjectedProducersHeader>
            {
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.RedH2MaterialTonnageProportion },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.AmberH2MaterialTonnageProportion },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.GreenH2MaterialTonnageProportion },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.RedMedicalH2MaterialTonnageProportion },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.AmberMedicalH2MaterialTonnageProportion },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.GreenMedicalH2MaterialTonnageProportion }
            };
        }

        private static List<ProjectedProducersHeader> GetProjectedMaterialColumnHeaders()
        {
            return new List<ProjectedProducersHeader>
            {
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdPackagingTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdRedTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdAmberTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdGreenTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdRedMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdAmberMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdGreenMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinPackagingTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinRedTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinAmberTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinGreenTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinRedMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinAmberMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinGreenMedicalTonnage },
            };
        }

        private static List<ProjectedProducersHeader> GetGlassColumnHeaders()
        {
            return new List<ProjectedProducersHeader>
            {
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersPackagingTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersTonnageWithoutRAM }
            };
        }

        private static List<ProjectedProducersHeader> GetProjectedGlassColumnHeaders()
        {
            return new List<ProjectedProducersHeader>
            {
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersPackagingTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenMedicalTonnage }
            };
        }
    }
}
