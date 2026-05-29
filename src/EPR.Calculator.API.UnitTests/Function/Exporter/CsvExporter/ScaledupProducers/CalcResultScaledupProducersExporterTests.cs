using System.Text;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.ScaledupProducers
{
    [TestClass]
    public class CalcResultScaledupProducersExporterTests
    {
        private CalcResultScaledupProducersExporter exporter;

        public CalcResultScaledupProducersExporterTests()
        {
            exporter = new CalcResultScaledupProducersExporter();
        }

        [TestMethod]
        public void Export_ShouldIncludeScaledupProducers_WhenNotNull()
        {
            // Arrange
            var materials = GetMaterials();
            var scaledupProducers = new CalcResultScaledupProducers
            {
                ScaledupProducers = GetCalcResultScaledupProducerList(),
            };

            var csvContent = new StringBuilder();

            // Act
            exporter.Export(scaledupProducers, materials, showTotal: true, csvContent);

            var result = csvContent.ToString().Split("\n").Select(s => s.TrimEnd(',')).ToArray();
            //Console.WriteLine($">> {JsonConvert.SerializeObject(result, Formatting.Indented)}");
            Console.WriteLine(string.Join("\n", result));

            var expected = new[] {
                new string[] {},
                new string[] {},
                new[] { "Scaled-up Producers" },
                new string[] {},
                new[]
                {
                    "Each submission for the year",
                    "","","","","","","","",
                    "Aluminium Breakdown",
                    "","","","","","","","",
                    "Glass Breakdown"
                },
                new[] {
                    "Producer ID",
                    "Subsidiary ID",
                    "Producer / Subsidiary Name",
                    "Trading Name",
                    "Level",
                    "Submission period code",
                    "Days in submission period",
                    "Days in whole period",
                    "Scale-up factor",
                    "Household Packaging Tonnage",
                    "Public Bin Tonnage",
                    "Total Tonnage",
                    "Self Managed Consumer Waste Tonnage",
                    "Net Tonnage",
                    "Scaled-up Household Packaging Tonnage",
                    "Scaled-up Public Bin Tonnage",
                    "Scaled-up Total Tonnage",
                    "Scaled-up Self Managed Consumer Waste Tonnage",
                    "Scaled-up Net Tonnage",
                    "Household Packaging Tonnage",
                    "Public Bin Tonnage",
                    "Household Drinks Containers Tonnage - Glass",
                    "Total Tonnage",
                    "Self Managed Consumer Waste Tonnage",
                    "Net Tonnage",
                    "Scaled-up Household Packaging Tonnage",
                    "Scaled-up Public Bin Tonnage",
                    "Scaled-up Household Drinks Containers Tonnage - Glass",
                    "Scaled-up Total Tonnage",
                    "Scaled-up Self Managed Consumer Waste Tonnage",
                    "Scaled-up Net Tonnage"
                },
                new[] { "101001","","Allied Packaging","","1","2024-P2","91","91","2",
                                 "1000.000","100.000","1100.000","500.000","1100.000","2000.000","200.000","2200.000","1000.000","2200.000","1000.000","100.000","120.000","1100.000","500.000","1100.000","2000.000","200.000","240.000","2200.000","1000.000","2200.000"
                 },
                new[] { "","","","","","","","",
                        "Totals","1000.000","100.000","1100.000","500.000","1100.000","2000.000","200.000","2200.000","1000.000","2200.000","1000.000","100.000","120.000","1100.000","500.000","1100.000","2000.000","200.000","240.000","2200.000","1000.000","2200.000"
                }
            };
        }

        [TestMethod]
        public void Export_ScaledUpProducer_ShouldNotContainTotalWhenIndicated()
        {
            // Arrange
            var materials = GetMaterials();
            var scaledupProducers = new CalcResultScaledupProducers
            {
                ScaledupProducers = null!,
            };

            var csvContent = new StringBuilder();

            // Act
            exporter.Export(scaledupProducers, materials, showTotal: false, csvContent);
            var result = csvContent.ToString();

            // Assert
            Assert.IsFalse(result.Contains("Totals"));
        }

        [TestMethod]
        public void Export_ScaledUpProducer_ShouldIncludeHeadersAndDisplayNone_WhenNoScaledUpProducer()
        {
            // Arrange
            var materials = GetMaterials();
            var scaledupProducers = new CalcResultScaledupProducers
            {
                ScaledupProducers = null!,
            };

            var csvContent = new StringBuilder();

            // Act
            exporter.Export(scaledupProducers, materials, showTotal: true, csvContent);
            var result = csvContent.ToString();

            // Assert
            Assert.IsTrue(result.Contains("Scaled-up Producers"));
            Assert.IsTrue(result.Contains("Each submission for the year"));
            Assert.IsTrue(result.Contains("Aluminium Breakdown"));
            Assert.IsTrue(result.Contains("Producer ID"));
            Assert.IsTrue(result.Contains("Subsidiary ID"));
            Assert.IsTrue(result.Contains("None"));
        }

        [TestMethod]
        public void Export_ShouldIncludeGlassColumns_WhenGlassMaterialPresent()
        {
            // Arrange
            var materials = GetMaterials();
            var scaledupProducers = new CalcResultScaledupProducers
            {
                ScaledupProducers = GetCalcResultScaledupProducerList(),
            };

            var csvContent = new StringBuilder();

            // Act
            exporter.Export(scaledupProducers, materials, showTotal: true, csvContent);
            var result = csvContent.ToString();

            // Assert
            Assert.IsTrue(result.Contains("Glass"));
            Assert.IsTrue(result.Contains("Household Drinks Containers Tonnage - Glass"));
            Assert.IsTrue(result.Contains("Scaled-up Household Drinks Containers Tonnage - Glass"));
        }

        private ImmutableList<MaterialDetail> GetMaterials()
        {
            return new List<MaterialDetail>
            {
                new MaterialDetail { Id = 1, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
                new MaterialDetail { Id = 2, Code = "GL", Name = "Glass", Description = "Glass" },
            }.ToImmutableList();
        }

        private ImmutableList<CalcResultScaledupProducer> GetCalcResultScaledupProducerList()
        {
            return [
                new CalcResultScaledupProducer
                {
                    ProducerId = 101001,
                    SubsidiaryId = string.Empty,
                    ProducerName = "Allied Packaging",
                    Level = "1",
                    SubmissionPeriodCode = "2024-P2",
                    DaysInSubmissionPeriod = 91,
                    DaysInWholePeriod = 91,
                    ScaleupFactor = 2,
                    PomData = new List<ScaledupPomEntry>
                    {
                        new ScaledupPomEntry(1, PackagingTypes.Household, 1000, 2000),
                        new ScaledupPomEntry(1, PackagingTypes.PublicBin, 100, 200),
                        new ScaledupPomEntry(1, PackagingTypes.ConsumerWaste, 500, 1000),
                        new ScaledupPomEntry(2, PackagingTypes.Household, 1000, 2000),
                        new ScaledupPomEntry(2, PackagingTypes.PublicBin, 100, 200),
                        new ScaledupPomEntry(2, PackagingTypes.HouseholdDrinksContainers, 120, 240),
                        new ScaledupPomEntry(2, PackagingTypes.ConsumerWaste, 500, 1000),
                    },
                }
            ];
        }

        [TestMethod]
        public void GetTonnagesTest()
        {
            var alId = 1;
            var materials = new List<Material>();
            materials.Add(new Material { Id = alId, Code = "AL", Name = "Aluminium" });
            var materialDetails = MaterialMapper.Map(materials);
            var pomData = new List<ScaledupPomEntry>
            {
                new ScaledupPomEntry(alId, PackagingTypes.Household, 0.1m, 0.2m),
            };
            var tonnage = CalcResultScaledupProducersExporter.GetTonnages(pomData, materialDetails);
            Assert.IsNotNull(tonnage);
            var aluminium = tonnage["AL"];
            Assert.AreEqual(0.1m, aluminium.ReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(0.2m, aluminium.ScaledupReportedHouseholdPackagingWasteTonnage);
        }

        [TestMethod]
        public void GetTonnages_ShouldCalculateCorrectlyForGlass()
        {
            var glassId = 1;
            var materials = new List<MaterialDetail>
            {
                new MaterialDetail { Id = glassId, Code = MaterialCodes.Glass, Name = "Glass", Description = "" },
            };
            var pomData = new List<ScaledupPomEntry>
            {
                new ScaledupPomEntry(glassId, PackagingTypes.Household, 0.1m, 0.1m),
                new ScaledupPomEntry(glassId, PackagingTypes.HouseholdDrinksContainers, 0.03m, 0.03m),
            };

            var result = CalcResultScaledupProducersExporter.GetTonnages(pomData, materials);

            Assert.IsTrue(result.ContainsKey(MaterialCodes.Glass));
            var glassTonnage = result[MaterialCodes.Glass];

            Assert.AreEqual(0.1m, glassTonnage.ReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(0, glassTonnage.ReportedPublicBinTonnage);
            Assert.AreEqual(0.03m, glassTonnage.HouseholdDrinksContainersTonnageGlass);
            Assert.AreEqual(0.13m, glassTonnage.TotalReportedTonnage);
            Assert.AreEqual(0.13m, glassTonnage.NetReportedTonnage);
            Assert.AreEqual(0.1m, glassTonnage.ScaledupReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(0, glassTonnage.ScaledupReportedPublicBinTonnage);
            Assert.AreEqual(0.03m, glassTonnage.ScaledupHouseholdDrinksContainersTonnageGlass);
            Assert.AreEqual(0.13m, glassTonnage.ScaledupTotalReportedTonnage);
            Assert.AreEqual(0.13m, glassTonnage.ScaledupNetReportedTonnage);
        }

        [TestMethod]
        public void GetOverallTotalRowTest()
        {
            var runProducerMaterialDetails = new List<CalcResultScaledupProducer>();
            var dictionary = new Dictionary<string, CalcResultScaledupProducerTonnage>();
            dictionary.Add("AL", new CalcResultScaledupProducerTonnage
            {
                ReportedHouseholdPackagingWasteTonnage = 10,
                ReportedPublicBinTonnage = 10,
                TotalReportedTonnage = 10,
                ReportedSelfManagedConsumerWasteTonnage = 10,
                NetReportedTonnage = 10,
                ScaledupReportedHouseholdPackagingWasteTonnage = 10,
                ScaledupReportedPublicBinTonnage = 10,
                ScaledupTotalReportedTonnage = 10,
                ScaledupReportedSelfManagedConsumerWasteTonnage = 10,
                ScaledupNetReportedTonnage = 10,
            });
            runProducerMaterialDetails.Add(new CalcResultScaledupProducer
            {
                ProducerId = 1,
                ScaledupProducerTonnageByMaterial = dictionary,
            });
            runProducerMaterialDetails.Add(new CalcResultScaledupProducer
            {
                ProducerId = 1,
                SubsidiaryId = "Sub1",
                ScaledupProducerTonnageByMaterial = dictionary,
            });
            runProducerMaterialDetails.Add(new CalcResultScaledupProducer
            {
                ProducerId = 1,
                SubsidiaryId = "Sub2",
                ScaledupProducerTonnageByMaterial = dictionary,
            });
            runProducerMaterialDetails.Add(new CalcResultScaledupProducer
            {
                ProducerId = 2,
                ScaledupProducerTonnageByMaterial = dictionary,
            });
            runProducerMaterialDetails.Add(new CalcResultScaledupProducer
            {
                ProducerId = 2,
                SubsidiaryId = "Sub3",
                ScaledupProducerTonnageByMaterial = dictionary,
            });
            runProducerMaterialDetails.Add(new CalcResultScaledupProducer
            {
                ProducerId = 2,
                SubsidiaryId = "Sub4",
                ScaledupProducerTonnageByMaterial = dictionary,
            });

            var materials = new List<Material>();
            materials.Add(new Material { Code = "AL", Name = "Aluminium" });
            var materialDetails = MaterialMapper.Map(materials);
            var totalRow = CalcResultScaledupProducersExporter.GetOverallTotalRow(runProducerMaterialDetails, materialDetails);
            Assert.IsNotNull(totalRow);
            var aluminium = totalRow.ScaledupProducerTonnageByMaterial["Aluminium"];
            Assert.IsNotNull(aluminium);
            Assert.AreEqual(60, aluminium.NetReportedTonnage);
            Assert.AreEqual(60, aluminium.ScaledupTotalReportedTonnage);
            Assert.AreEqual(60, aluminium.ScaledupNetReportedTonnage);
            Assert.AreEqual(60, aluminium.ScaledupReportedPublicBinTonnage);
            Assert.AreEqual(60, aluminium.ScaledupReportedPublicBinTonnage);
        }
    }
}
