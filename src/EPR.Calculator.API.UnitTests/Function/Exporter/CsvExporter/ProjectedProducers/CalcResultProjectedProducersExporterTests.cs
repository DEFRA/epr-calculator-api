using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ProjectedProducers;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.ProjectedProducers
{
    [TestClass]
    public class CalcResultProjectedProducersExporterTests
    {
        private CalcResultProjectedProducersExporter exporter = new CalcResultProjectedProducersExporter();

        private readonly ImmutableList<MaterialDetail> materials = ImmutableList.Create<MaterialDetail>
        (
            new MaterialDetail { Id = 1, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
            new MaterialDetail { Id = 2, Code = "GL", Name = "Glass", Description = "Glass" },
            new MaterialDetail { Id = 3, Code = "OT", Name = "Other materials", Description = "Other materials" }
        );

        [TestMethod]
        public void Export_ShouldIncludeProjectedProducers_WhenNotNull()
        {
            // Arrange
            var projectedProducers = new CalcResultProjectedProducers
            {
                H2ProjectedProducers = GetH2ProjectedProducersList(),
                H1ProjectedProducers = GetH1ProjectedProducersList(),
            };

            var csvContent = new StringBuilder();

            // Act
            exporter.Export(projectedProducers, materials, csvContent);
            var rows = CsvTestUtils.GetRows(csvContent);

            // Assert H2
            Assert.IsTrue(rows[2][0].Contains(CalcResultProjectedProducersHeaders.H2ProjectedProducers));

            var h2ColumnHeaders = rows[5];
            var h2ColumnValues = rows[6];
            var h2MaterialHeaders = rows[4];

            var h2MaterialHeadersIndexes = CsvTestUtils.FindAllHeaderIndexes(h2ColumnHeaders, CalcResultProjectedProducersHeaders.HouseholdPackagingTonnage);
            Assert.IsTrue(h2MaterialHeaders[h2MaterialHeadersIndexes[0]].Contains("Aluminium Breakdown"));
            Assert.IsTrue(h2MaterialHeaders[h2MaterialHeadersIndexes[1]].Contains("Glass Breakdown"));
            Assert.IsTrue(h2MaterialHeaders[h2MaterialHeadersIndexes[2]].Contains("Other materials Breakdown"));

            var h2Data = CsvTestUtils.GetColumnHeaderValues(h2ColumnHeaders, h2ColumnValues);

            Assert.AreEqual("101001", h2Data[CalcResultProjectedProducersHeaders.ProducerId].First());
            Assert.AreEqual(string.Empty, h2Data[CalcResultProjectedProducersHeaders.SubsidiaryId].First());
            Assert.AreEqual("1", h2Data[CalcResultProjectedProducersHeaders.Level].First());
            Assert.AreEqual("2026-H2", h2Data[CalcResultProjectedProducersHeaders.SubmissionPeriodCode].First());
            //Aluminium
            Assert.AreEqual("100.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdPackagingTonnage][0]);
            Assert.AreEqual("30.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdRedTonnage][0]);
            Assert.AreEqual("40.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdAmberTonnage][0]);
            Assert.AreEqual("0.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdGreenTonnage][0]);
            Assert.AreEqual("40.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdRedMedicalTonnage][0]);
            Assert.AreEqual("0.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdAmberMedicalTonnage][0]);
            Assert.AreEqual("0.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdGreenMedicalTonnage][0]);
            Assert.AreEqual("0.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdTonnageWithoutRAMDefaultedToRed][0]);
            Assert.AreEqual("300.000", h2Data[CalcResultProjectedProducersHeaders.TotalTonnage][0]);
            //Glass
            Assert.AreEqual("500.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersPackagingTonnage].First());
            Assert.AreEqual("0.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedTonnage].First());
            Assert.AreEqual("0.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberTonnage].First());
            Assert.AreEqual("0.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenTonnage].First());
            Assert.AreEqual("0.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedMedicalTonnage].First());
            Assert.AreEqual("0.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberMedicalTonnage].First());
            Assert.AreEqual("0.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenMedicalTonnage].First());
            Assert.AreEqual("500.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersTonnageWithoutRAMDefaultedToRed].First());
            Assert.AreEqual("800.000", h2Data[CalcResultProjectedProducersHeaders.TotalTonnage][1]);

            // Assert H1
            Assert.IsTrue(rows[9][0].Contains(CalcResultProjectedProducersHeaders.H1ProjectedProducers));

            var h1ColumnHeaders = rows[12];
            var h1ColumnValues = rows[13];

            var h1MaterialHeaders = rows[11];
            var h1MaterialHeadersIndexes = CsvTestUtils.FindAllHeaderIndexes(h1ColumnHeaders, CalcResultProjectedProducersHeaders.HouseholdPackagingTonnage);
            Assert.IsTrue(h1MaterialHeaders[h1MaterialHeadersIndexes[0]].Contains("Aluminium Breakdown"));
            Assert.IsTrue(h1MaterialHeaders[h1MaterialHeadersIndexes[1]].Contains("Projected Aluminium Breakdown"));
            Assert.IsTrue(h1MaterialHeaders[h1MaterialHeadersIndexes[2]].Contains("Glass Breakdown"));
            Assert.IsTrue(h1MaterialHeaders[h1MaterialHeadersIndexes[3]].Contains("Projected Glass Breakdown"));
            Assert.IsTrue(h1MaterialHeaders[h1MaterialHeadersIndexes[4]].Contains("Other materials Breakdown"));
            Assert.IsTrue(h1MaterialHeaders[h1MaterialHeadersIndexes[5]].Contains("Projected Other materials Breakdown"));

            var h1Data = CsvTestUtils.GetColumnHeaderValues(h1ColumnHeaders, h1ColumnValues);

            Assert.AreEqual("101001", h1Data[CalcResultProjectedProducersHeaders.ProducerId].First());
            Assert.AreEqual(string.Empty, h1Data[CalcResultProjectedProducersHeaders.SubsidiaryId].First());
            Assert.AreEqual("1", h1Data[CalcResultProjectedProducersHeaders.Level].First());
            Assert.AreEqual("2026-H1", h1Data[CalcResultProjectedProducersHeaders.SubmissionPeriodCode].First());
            //Aluminium
            Assert.AreEqual("100.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinPackagingTonnage][0]);
            Assert.AreEqual("30.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinRedTonnage][0]);
            Assert.AreEqual("40.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinAmberTonnage][0]);
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinGreenTonnage][0]);
            Assert.AreEqual("40.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinRedMedicalTonnage][0]);
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinAmberMedicalTonnage][0]);
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinGreenMedicalTonnage][0]);
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinTonnageWithoutRAM][0]);
            // H2 Aluminium proportions
            Assert.AreEqual("-", h1Data[CalcResultProjectedProducersHeaders.RedH2MaterialTonnageProportion][0]);
            Assert.AreEqual("-", h1Data[CalcResultProjectedProducersHeaders.AmberH2MaterialTonnageProportion][0]);
            Assert.AreEqual("-", h1Data[CalcResultProjectedProducersHeaders.GreenH2MaterialTonnageProportion][0]);
            Assert.AreEqual("-", h1Data[CalcResultProjectedProducersHeaders.RedMedicalH2MaterialTonnageProportion][0]);
            Assert.AreEqual("-", h1Data[CalcResultProjectedProducersHeaders.AmberMedicalH2MaterialTonnageProportion][0]);
            Assert.AreEqual("-", h1Data[CalcResultProjectedProducersHeaders.GreenMedicalH2MaterialTonnageProportion][0]);
            //Projected Aluminium
            Assert.AreEqual("100.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinPackagingTonnage][1]);
            Assert.AreEqual("25.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinRedTonnage][1]);
            Assert.AreEqual("20.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinAmberTonnage][1]);
            Assert.AreEqual("50.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinGreenTonnage][1]);
            Assert.AreEqual("10.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinRedMedicalTonnage][1]);
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinAmberMedicalTonnage][1]);
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinGreenMedicalTonnage][1]);

            //Glass
            Assert.AreEqual("500.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersPackagingTonnage].First());
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedTonnage].First());
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberTonnage].First());
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenTonnage].First());
            Assert.AreEqual("100.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedMedicalTonnage].First());
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberMedicalTonnage].First());
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenMedicalTonnage].First());
            Assert.AreEqual("400.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersTonnageWithoutRAM].First());
            // H2 Glass proportions
            Assert.AreEqual("60.00%", h1Data[CalcResultProjectedProducersHeaders.RedH2MaterialTonnageProportion][1]);
            Assert.AreEqual("50.00%", h1Data[CalcResultProjectedProducersHeaders.AmberH2MaterialTonnageProportion][1]);
            Assert.AreEqual("40.00%", h1Data[CalcResultProjectedProducersHeaders.GreenH2MaterialTonnageProportion][1]);
            Assert.AreEqual("30.00%", h1Data[CalcResultProjectedProducersHeaders.RedMedicalH2MaterialTonnageProportion][1]);
            Assert.AreEqual("20.00%", h1Data[CalcResultProjectedProducersHeaders.AmberMedicalH2MaterialTonnageProportion][1]);
            Assert.AreEqual("10.00%", h1Data[CalcResultProjectedProducersHeaders.GreenMedicalH2MaterialTonnageProportion][1]);
            //Projected Glass
            Assert.AreEqual("700.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersPackagingTonnage].Last());
            Assert.AreEqual("50.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedTonnage].Last());
            Assert.AreEqual("200.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberTonnage].Last());
            Assert.AreEqual("50.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenTonnage].Last());
            Assert.AreEqual("200.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedMedicalTonnage].Last());
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberMedicalTonnage].Last());
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenMedicalTonnage].Last());

            //Other material proportions
            Assert.AreEqual("-", h1Data[CalcResultProjectedProducersHeaders.RedH2MaterialTonnageProportion][2]);
            Assert.AreEqual("-", h1Data[CalcResultProjectedProducersHeaders.AmberH2MaterialTonnageProportion][2]);
            Assert.AreEqual("-", h1Data[CalcResultProjectedProducersHeaders.GreenH2MaterialTonnageProportion][2]);
            Assert.AreEqual("-", h1Data[CalcResultProjectedProducersHeaders.RedMedicalH2MaterialTonnageProportion][2]);
            Assert.AreEqual("-", h1Data[CalcResultProjectedProducersHeaders.AmberMedicalH2MaterialTonnageProportion][2]);
            Assert.AreEqual("-", h1Data[CalcResultProjectedProducersHeaders.GreenMedicalH2MaterialTonnageProportion][2]);
        }

        [TestMethod]
        public void Export_ShouldIncludeProjectedProducers_WhichWereNotComplete()
        {
            var completeProjectedProducers = new CalcResultProjectedProducers
            {
                H2ProjectedProducers = GetH2ProjectedProducersList().Concat(GetCompleteH2ProjectedProducersList()).ToImmutableList(),
                H1ProjectedProducers = GetH1ProjectedProducersList().Concat(GetCompleteH1ProjectedProducersList()).ToImmutableList(),
            };

            var csvContent = new StringBuilder();

            exporter.Export(completeProjectedProducers, materials, csvContent);

            Assert.IsTrue(csvContent.ToString().Contains("101001"));
            Assert.IsFalse(csvContent.ToString().Contains("202002"));
            Assert.IsFalse(csvContent.ToString().Contains("ABC"));
            Assert.IsFalse(csvContent.ToString().Contains("CDE"));
        }

        [TestMethod]
        public void Export_ShouldIncludeEmptyProjectedProducers_WhenAllAreH2H1Complete()
        {
            var completeProjectedProducers = new CalcResultProjectedProducers
            {
                H2ProjectedProducers = GetCompleteH2ProjectedProducersList(),
                H1ProjectedProducers = GetCompleteH1ProjectedProducersList(),
            };

            var csvContent = new StringBuilder();

            exporter.Export(completeProjectedProducers, materials, csvContent);

            var rows = CsvTestUtils.GetRows(csvContent);

            Assert.IsTrue(rows[2][0].Contains(CalcResultProjectedProducersHeaders.H2ProjectedProducers));
            Assert.IsTrue(rows[6][0].Contains(CalcResultProjectedProducersHeaders.NoProjectedProducers));
            Assert.IsTrue(rows[9][0].Contains(CalcResultProjectedProducersHeaders.H1ProjectedProducers));
            Assert.IsTrue(rows[13][0].Contains(CalcResultProjectedProducersHeaders.NoProjectedProducers));
        }

        [TestMethod]
        public void Export_ShouldIncludeEmptyProjectedProducers_WhenNull()
        {
            var projectedProducers = new CalcResultProjectedProducers
            {
                H2ProjectedProducers = null,
                H1ProjectedProducers = null,
            };

            var csvContent = new StringBuilder();

            exporter.Export(projectedProducers, materials, csvContent);

            var rows = csvContent.ToString()
                        .Split(Environment.NewLine)
                        .Select(r => r.Split(','))
                        .ToArray();

            Assert.IsTrue(rows[2][0].Contains(CalcResultProjectedProducersHeaders.H2ProjectedProducers));
            Assert.IsTrue(rows[6][0].Contains(CalcResultProjectedProducersHeaders.NoProjectedProducers));
            Assert.IsTrue(rows[9][0].Contains(CalcResultProjectedProducersHeaders.H1ProjectedProducers));
            Assert.IsTrue(rows[13][0].Contains(CalcResultProjectedProducersHeaders.NoProjectedProducers));
        }

        private ImmutableList<CalcResultH2ProjectedProducer> GetH2ProjectedProducersList()
        {
            return new List<CalcResultH2ProjectedProducer>()
            {
                new CalcResultH2ProjectedProducer
                {
                    ProducerId = 101001,
                    SubsidiaryId = null,
                    Level = "1",
                    SubmissionPeriodCode = "2026-H2",
                    H2ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                    {
                        {
                            "AL",
                            new CalcResultH2ProjectedProducerMaterialTonnage
                            {
                                HouseholdTonnage = 100,
                                HouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                HouseholdTonnageWithoutRAM = 0,
                                PublicBinTonnage = 200,
                                PublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                PublicBinTonnageWithoutRAM = 0,
                                TotalTonnage = 300,
                                ProjectedHouseholdTonnage = 100,
                                ProjectedHouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                ProjectedPublicBinTonnage = 200,
                                ProjectedPublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                            }
                        },
                        {
                            "GL",
                            new CalcResultH2ProjectedProducerMaterialTonnage
                            {
                                HouseholdTonnage = 200,
                                HouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                HouseholdTonnageWithoutRAM = 0,
                                PublicBinTonnage = 100,
                                PublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                PublicBinTonnageWithoutRAM = 0,
                                HouseholdDrinksContainerTonnage = 500,
                                HouseholdDrinksContainerRAMTonnage = new RAMTonnage{ RedTonnage = 0, RedMedicalTonnage = 0, AmberTonnage = 0, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                HouseholdDrinksContainerTonnageWithoutRAM = 500,
                                TotalTonnage = 800,
                                ProjectedHouseholdTonnage = 200,
                                ProjectedHouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                ProjectedPublicBinTonnage = 100,
                                ProjectedPublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                ProjectedHouseholdDrinksContainerTonnage = 500,
                                ProjectedHouseholdDrinksContainerRAMTonnage = new RAMTonnage{ RedTonnage = 500, RedMedicalTonnage = 0, AmberTonnage = 0, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                            }
                        },
                    },
                },
            }.ToImmutableList();
        }

        private ImmutableList<CalcResultH2ProjectedProducer> GetCompleteH2ProjectedProducersList()
        {
            return new List<CalcResultH2ProjectedProducer>()
            {
                new CalcResultH2ProjectedProducer
                {
                    ProducerId = 202002,
                    SubsidiaryId = null,
                    Level = "1",
                    SubmissionPeriodCode = "2026-H2",
                    H2ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                    {
                        {
                            "AL",
                            new CalcResultH2ProjectedProducerMaterialTonnage
                            {
                                HouseholdTonnage = 100,
                                HouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                HouseholdTonnageWithoutRAM = 0,
                                PublicBinTonnage = 400,
                                PublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 100, RedMedicalTonnage = 40, AmberTonnage = 60, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                PublicBinTonnageWithoutRAM = 0,
                                TotalTonnage = 600,
                                ProjectedHouseholdTonnage = 200,
                                ProjectedHouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 60, RedMedicalTonnage = 80, AmberTonnage = 80, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                ProjectedPublicBinTonnage = 400,
                                ProjectedPublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 100, RedMedicalTonnage = 40, AmberTonnage = 60, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                            }
                        },
                        {
                            "GL",
                            new CalcResultH2ProjectedProducerMaterialTonnage
                            {
                                HouseholdTonnage = 200,
                                HouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                HouseholdTonnageWithoutRAM = 0,
                                PublicBinTonnage = 100,
                                PublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                PublicBinTonnageWithoutRAM = 0,
                                HouseholdDrinksContainerTonnage = 500,
                                HouseholdDrinksContainerRAMTonnage = new RAMTonnage{ RedTonnage = 500, RedMedicalTonnage = 0, AmberTonnage = 0, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                HouseholdDrinksContainerTonnageWithoutRAM = 0,
                                TotalTonnage = 800,
                                ProjectedHouseholdTonnage = 200,
                                ProjectedHouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                ProjectedPublicBinTonnage = 100,
                                ProjectedPublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                ProjectedHouseholdDrinksContainerTonnage = 500,
                                ProjectedHouseholdDrinksContainerRAMTonnage = new RAMTonnage{ RedTonnage = 500, RedMedicalTonnage = 0, AmberTonnage = 0, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                            }
                        },
                    },
                },
                new CalcResultH2ProjectedProducer
                {
                    ProducerId = 202002,
                    SubsidiaryId = null,
                    Level = "2",
                    SubmissionPeriodCode = "2026-H2",
                    H2ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                    {
                        {
                            "AL",
                            new CalcResultH2ProjectedProducerMaterialTonnage
                            {
                                HouseholdTonnage = 100,
                                HouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                HouseholdTonnageWithoutRAM = 0,
                                PublicBinTonnage = 200,
                                PublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                PublicBinTonnageWithoutRAM = 0,
                                TotalTonnage = 300,
                                ProjectedHouseholdTonnage = 100,
                                ProjectedHouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                ProjectedPublicBinTonnage = 200,
                                ProjectedPublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                            }
                        },
                        {
                            "GL",
                            new CalcResultH2ProjectedProducerMaterialTonnage
                            {
                                HouseholdTonnage = 200,
                                HouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                HouseholdTonnageWithoutRAM = 0,
                                PublicBinTonnage = 100,
                                PublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                PublicBinTonnageWithoutRAM = 0,
                                HouseholdDrinksContainerTonnage = 500,
                                HouseholdDrinksContainerRAMTonnage = new RAMTonnage{ RedTonnage = 500, RedMedicalTonnage = 0, AmberTonnage = 0, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                HouseholdDrinksContainerTonnageWithoutRAM = 0,
                                TotalTonnage = 800,
                                ProjectedHouseholdTonnage = 200,
                                ProjectedHouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                ProjectedPublicBinTonnage = 100,
                                ProjectedPublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                ProjectedHouseholdDrinksContainerTonnage = 500,
                                ProjectedHouseholdDrinksContainerRAMTonnage = new RAMTonnage{ RedTonnage = 500, RedMedicalTonnage = 0, AmberTonnage = 0, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                            }
                        },
                    },
                },
                new CalcResultH2ProjectedProducer
                {
                    ProducerId = 202002,
                    SubsidiaryId = "ABC",
                    Level = "2",
                    SubmissionPeriodCode = "2026-H2",
                    H2ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                    {
                        {
                            "AL",
                            new CalcResultH2ProjectedProducerMaterialTonnage
                            {
                                HouseholdTonnage = 100,
                                HouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                HouseholdTonnageWithoutRAM = 0,
                                PublicBinTonnage = 200,
                                PublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                PublicBinTonnageWithoutRAM = 0,
                                TotalTonnage = 300,
                                ProjectedHouseholdTonnage = 100,
                                ProjectedHouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                ProjectedPublicBinTonnage = 200,
                                ProjectedPublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                            }
                        }
                    },
                }
            }.ToImmutableList();
        }

        private ImmutableList<CalcResultH1ProjectedProducer> GetH1ProjectedProducersList()
        {
            return new List<CalcResultH1ProjectedProducer>()
            {
                new CalcResultH1ProjectedProducer
                {
                    ProducerId = 101001,
                    SubsidiaryId = null,
                    Level = "1",
                    SubmissionPeriodCode = "2026-H1",
                    H1ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage>
                    {
                        {
                            "AL",
                            new CalcResultH1ProjectedProducerMaterialTonnage
                            {
                                HouseholdTonnage = 200,
                                HouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                HouseholdTonnageWithoutRAM = 0,
                                PublicBinTonnage = 100,
                                PublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                PublicBinTonnageWithoutRAM = 0,
                                H2RamProportions = new RAMProportions { Red = 0.1m, Amber = 0.2m, Green = 0.3m, RedMedical = 0.4m, AmberMedical = 0.5m, GreenMedical = 0.6m },
                                ProjectedHouseholdTonnage = 50,
                                ProjectedHouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 15, RedMedicalTonnage = 20, AmberTonnage = 10, AmberMedicalTonnage = 0, GreenTonnage = 20, GreenMedicalTonnage = 0 },
                                ProjectedPublicBinTonnage = 100,
                                ProjectedPublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 25, RedMedicalTonnage = 10, AmberTonnage = 20, AmberMedicalTonnage = 0, GreenTonnage = 50, GreenMedicalTonnage = 0 },
                                TotalTonnage = 300
                            }
                        },
                        {
                            "GL",
                            new CalcResultH1ProjectedProducerMaterialTonnage
                            {
                                HouseholdTonnage = 100,
                                HouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                HouseholdTonnageWithoutRAM = 0,
                                PublicBinTonnage = 200,
                                PublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                PublicBinTonnageWithoutRAM = 0,
                                HouseholdDrinksContainerTonnage = 500,
                                HouseholdDrinksContainerRAMTonnage = new RAMTonnage{ RedTonnage = 0, RedMedicalTonnage = 100, AmberTonnage = 0, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                HouseholdDrinksContainerTonnageWithoutRAM = 400,
                                H2RamProportions = new RAMProportions { Red = 0.6m, Amber = 0.5m, Green = 0.4m, RedMedical = 0.3m, AmberMedical = 0.2m, GreenMedical = 0.1m },
                                ProjectedHouseholdTonnage = 100,
                                ProjectedHouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 15, RedMedicalTonnage = 20, AmberTonnage = 10, AmberMedicalTonnage = 0, GreenTonnage = 20, GreenMedicalTonnage = 0 },
                                ProjectedPublicBinTonnage = 50,
                                ProjectedPublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 25, RedMedicalTonnage = 10, AmberTonnage = 20, AmberMedicalTonnage = 0, GreenTonnage = 50, GreenMedicalTonnage = 0 },
                                ProjectedHouseholdDrinksContainerTonnage = 700,
                                ProjectedHouseholdDrinksContainerRAMTonnage = new RAMTonnage{ RedTonnage = 50, RedMedicalTonnage = 200, AmberTonnage = 200, AmberMedicalTonnage = 0, GreenTonnage = 50, GreenMedicalTonnage = 0 },
                                TotalTonnage = 300
                            }
                        },
                        {
                            "OT",
                            new CalcResultH1ProjectedProducerMaterialTonnage
                            {
                                HouseholdTonnage = 100,
                                HouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                HouseholdTonnageWithoutRAM = 0,
                                PublicBinTonnage = 200,
                                PublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                PublicBinTonnageWithoutRAM = 0,
                                H2RamProportions = new RAMProportions { Red = 0, Amber = 0, Green = 0, RedMedical = 0, AmberMedical = 0, GreenMedical = 0 },
                                ProjectedHouseholdTonnage = 100,
                                ProjectedHouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 15, RedMedicalTonnage = 20, AmberTonnage = 10, AmberMedicalTonnage = 0, GreenTonnage = 20, GreenMedicalTonnage = 0 },
                                ProjectedPublicBinTonnage = 50,
                                ProjectedPublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 25, RedMedicalTonnage = 10, AmberTonnage = 20, AmberMedicalTonnage = 0, GreenTonnage = 50, GreenMedicalTonnage = 0 },
                                TotalTonnage = 300
                            }
                        },
                    },
                },
            }.ToImmutableList();
        }

        private ImmutableList<CalcResultH1ProjectedProducer> GetCompleteH1ProjectedProducersList()
        {
            return new List<CalcResultH1ProjectedProducer>()
            {
                new CalcResultH1ProjectedProducer
                {
                    ProducerId = 202002,
                    SubsidiaryId = null,
                    Level = "1",
                    SubmissionPeriodCode = "2026-H1",
                    H1ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage>
                    {
                        {
                            "AL",
                            new CalcResultH1ProjectedProducerMaterialTonnage
                            {
                                HouseholdTonnage = 200,
                                HouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                HouseholdTonnageWithoutRAM = 0,
                                PublicBinTonnage = 100,
                                PublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                PublicBinTonnageWithoutRAM = 0,
                                H2RamProportions = new RAMProportions { Red = 0.1m, Amber = 0.2m, Green = 0.3m, RedMedical = 0.4m, AmberMedical = 0.5m, GreenMedical = 0.6m },
                                ProjectedHouseholdTonnage = 200,
                                ProjectedHouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                ProjectedPublicBinTonnage = 100,
                                ProjectedPublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                TotalTonnage = 300
                            }
                        },
                        {
                            "GL",
                            new CalcResultH1ProjectedProducerMaterialTonnage
                            {
                                HouseholdTonnage = 200,
                                HouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 60, RedMedicalTonnage = 80, AmberTonnage = 80, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                HouseholdTonnageWithoutRAM = 0,
                                PublicBinTonnage = 400,
                                PublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 100, RedMedicalTonnage = 40, AmberTonnage = 60, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                PublicBinTonnageWithoutRAM = 0,
                                HouseholdDrinksContainerTonnage = 1000,
                                HouseholdDrinksContainerRAMTonnage = new RAMTonnage{ RedTonnage = 800, RedMedicalTonnage = 200, AmberTonnage = 0, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                HouseholdDrinksContainerTonnageWithoutRAM = 0,
                                H2RamProportions = new RAMProportions { Red = 0.6m, Amber = 0.5m, Green = 0.4m, RedMedical = 0.3m, AmberMedical = 0.2m, GreenMedical = 0.1m },
                                ProjectedHouseholdTonnage = 200,
                                ProjectedHouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 60, RedMedicalTonnage = 80, AmberTonnage = 80, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                ProjectedPublicBinTonnage = 400,
                                ProjectedPublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 100, RedMedicalTonnage = 40, AmberTonnage = 60, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                ProjectedHouseholdDrinksContainerTonnage = 1000,
                                ProjectedHouseholdDrinksContainerRAMTonnage = new RAMTonnage{ RedTonnage = 800, RedMedicalTonnage = 200, AmberTonnage = 0, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                TotalTonnage = 800
                            }
                        }
                    },
                },
                new CalcResultH1ProjectedProducer
                {
                    ProducerId = 202002,
                    SubsidiaryId = null,
                    Level = "2",
                    SubmissionPeriodCode = "2026-H1",
                    H1ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage>
                    {
                        {
                            "GL",
                            new CalcResultH1ProjectedProducerMaterialTonnage
                            {
                                HouseholdTonnage = 100,
                                HouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                HouseholdTonnageWithoutRAM = 0,
                                PublicBinTonnage = 200,
                                PublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                PublicBinTonnageWithoutRAM = 0,
                                HouseholdDrinksContainerTonnage = 500,
                                HouseholdDrinksContainerRAMTonnage = new RAMTonnage{ RedTonnage = 400, RedMedicalTonnage = 100, AmberTonnage = 0, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                HouseholdDrinksContainerTonnageWithoutRAM = 0,
                                H2RamProportions = new RAMProportions { Red = 0.6m, Amber = 0.5m, Green = 0.4m, RedMedical = 0.3m, AmberMedical = 0.2m, GreenMedical = 0.1m },
                                ProjectedHouseholdTonnage = 100,
                                ProjectedPublicBinTonnage = 200,
                                ProjectedHouseholdDrinksContainerTonnage = 500,
                                ProjectedHouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                ProjectedPublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                ProjectedHouseholdDrinksContainerRAMTonnage = new RAMTonnage{ RedTonnage = 400, RedMedicalTonnage = 100, AmberTonnage = 0, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                TotalTonnage = 800
                            }
                        }
                    },
                },
                new CalcResultH1ProjectedProducer
                {
                    ProducerId = 202002,
                    SubsidiaryId = "CDE",
                    Level = "2",
                    SubmissionPeriodCode = "2026-H1",
                    H1ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage>
                    {
                        {
                            "AL",
                            new CalcResultH1ProjectedProducerMaterialTonnage
                            {
                                HouseholdTonnage = 200,
                                PublicBinTonnage = 100,
                                HouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                HouseholdTonnageWithoutRAM = 0,
                                PublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                PublicBinTonnageWithoutRAM = 0,
                                H2RamProportions = new RAMProportions { Red = 0.1m, Amber = 0.2m, Green = 0.3m, RedMedical = 0.4m, AmberMedical = 0.5m, GreenMedical = 0.6m },
                                ProjectedHouseholdTonnage = 200,
                                ProjectedPublicBinTonnage = 100,
                                ProjectedHouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                ProjectedPublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                TotalTonnage = 300
                            }
                        },
                        {
                            "GL",
                            new CalcResultH1ProjectedProducerMaterialTonnage
                            {
                                HouseholdTonnage = 100,
                                HouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                HouseholdTonnageWithoutRAM = 0,
                                PublicBinTonnage = 200,
                                PublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                PublicBinTonnageWithoutRAM = 0,
                                HouseholdDrinksContainerTonnage = 500,
                                HouseholdDrinksContainerRAMTonnage = new RAMTonnage{ RedTonnage = 400, RedMedicalTonnage = 100, AmberTonnage = 0, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                HouseholdDrinksContainerTonnageWithoutRAM = 0,
                                H2RamProportions = new RAMProportions { Red = 0.6m, Amber = 0.5m, Green = 0.4m, RedMedical = 0.3m, AmberMedical = 0.2m, GreenMedical = 0.1m },
                                ProjectedHouseholdTonnage = 100,
                                ProjectedPublicBinTonnage = 200,
                                ProjectedHouseholdDrinksContainerTonnage = 500,
                                ProjectedHouseholdRAMTonnage = new RAMTonnage{ RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                ProjectedPublicBinRAMTonnage = new RAMTonnage{ RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                ProjectedHouseholdDrinksContainerRAMTonnage = new RAMTonnage{ RedTonnage = 400, RedMedicalTonnage = 100, AmberTonnage = 0, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                TotalTonnage = 800
                            }
                        }
                    },
                },
            }.ToImmutableList();
        }
    }
}
