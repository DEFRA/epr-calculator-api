using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary.CommsCostTwoA;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.Builder.Summary.Common;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.CommsCostTwoA
{
    [TestClass]
  public class CalcResultSummaryCommsCostTwoATests
  {
        private readonly List<ProducerDetail> producers;
        private readonly MaterialDetail material;
        private readonly CalcResult calcResult;
        private readonly IEnumerable<CalcResultScaledupProducer> scaledupProducers;
        private readonly IEnumerable<CalcResultPartialObligation> partialObligations;

        private Fixture Fixture { get; init; } = new Fixture();

        public CalcResultSummaryCommsCostTwoATests()
        {
            material = GetMaterial();
            producers = GetProducers();
            scaledupProducers = new List<CalcResultScaledupProducer>();
            partialObligations = new List<CalcResultPartialObligation>();

            calcResult = new CalcResult
            {
                ApplyModulation = false,
                CalcResultScaledupProducers = new CalcResultScaledupProducers(),
                CalcResultPartialObligations = new CalcResultPartialObligations(),
                CalcResultParameterOtherCost = TestDataHelper.GetCalcResultParameterOtherCost(),
                CalcResultDetail = TestDataHelper.GetCalcResultDetail(),
                CalcResultLaDisposalCostData = TestDataHelper.GetCalcResultLaDisposalCostData(),
                CalcResultLapcapData = TestDataHelper.GetCalcResultLapcapData(),
                CalcResultOnePlusFourApportionment = TestDataHelper.GetCalcResultOnePlusFourApportionment(),
                CalcResultSummary = TestDataHelper.GetCalcResultSummary(),
                CalcResultCommsCostReportDetail = TestDataHelper.GetCalcResultCommsCostReportDetail(),
                CalcResultLateReportingTonnageData = this.GetCalcResultLateReportingTonnage(),
                CalcResultProjectedProducers = new CalcResultProjectedProducers()
            };
        }

        public static List<Dictionary<string, CalcResultScaledupProducerTonnage>> GetScaledUpProducers()
        {
            return new List<Dictionary<string, CalcResultScaledupProducerTonnage>>
            {
                new Dictionary<string, CalcResultScaledupProducerTonnage>
                {
                    ["10001"] =
                        new CalcResultScaledupProducerTonnage
                        {
                            ReportedHouseholdPackagingWasteTonnage = 0,
                            ReportedPublicBinTonnage = 0,
                            TotalReportedTonnage = 0,
                            ReportedSelfManagedConsumerWasteTonnage = 0,
                            NetReportedTonnage = 0,
                            ScaledupReportedHouseholdPackagingWasteTonnage = 0,
                            ScaledupReportedPublicBinTonnage = 0,
                            ScaledupTotalReportedTonnage = 0,
                            ScaledupReportedSelfManagedConsumerWasteTonnage = 0,
                            ScaledupNetReportedTonnage = 0,
                        }
                }
            };
        }

        [TestMethod]
        public void GetEnglandWithBadDebtProvisionForCommsTotal_WhenNoProducers_ShouldReturn0()
        {
            // Arrange
            producers.Clear();

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetEnglandWithBadDebtProvisionForCommsTotal(CalcResultSummaryUtilTests.ProjectedMaterialsLookup(producers), producers, material, calcResult);

            // Assert
            Assert.AreEqual(0m, totalCost);
        }

        [TestMethod]
        public void GetEnglandWithBadDebtProvisionForCommsTotal_ShouldReturnCorrectTotal()
        {
            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetEnglandWithBadDebtProvisionForCommsTotal(CalcResultSummaryUtilTests.ProjectedMaterialsLookup(producers), producers, material, calcResult);

            // Assert
            Assert.AreEqual(569.856m, totalCost);
        }

        [TestMethod]
        public void GetWalesWithBadDebtProvisionForCommsTotal_ShouldReturnCorrectTotal()
        {
            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetWalesWithBadDebtProvisionForCommsTotal(CalcResultSummaryUtilTests.ProjectedMaterialsLookup(producers), producers, material, calcResult);

            // Assert
            Assert.AreEqual(142.464m, totalCost);
        }

        [TestMethod]
        public void GetScotlandWithBadDebtProvisionForCommsTotal_ShouldReturnCorrectTotal()
        {
            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetScotlandWithBadDebtProvisionForCommsTotal(CalcResultSummaryUtilTests.ProjectedMaterialsLookup(producers), producers, material, calcResult);

            // Assert
            Assert.AreEqual(213.696m, totalCost);
        }

        [TestMethod]
        public void GetNorthernIrelandWithBadDebtProvisionForCommsTotal_ShouldReturnCorrectTotal()
        {
            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetNorthernIrelandWithBadDebtProvisionForCommsTotal(CalcResultSummaryUtilTests.ProjectedMaterialsLookup(producers), producers, material, calcResult);

            // Assert
            Assert.AreEqual(498.624m, totalCost);
        }

        [TestMethod]
        public void GetProducerTotalCostWithoutBadDebtProvisionTotal_ShouldReturnCorrectTotal()
        {
            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostWithoutBadDebtProvisionTotal(CalcResultSummaryUtilTests.ProjectedMaterialsLookup(producers), producers, material, calcResult);

            // Assert
            Assert.AreEqual(1344.00m, totalCost);
        }

        [TestMethod]
        public void GetBadDebtProvisionForCommsCostTotal_ShouldReturnCorrectTotal()
        {
            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetBadDebtProvisionForCommsCostTotal(CalcResultSummaryUtilTests.ProjectedMaterialsLookup(producers), producers, material, calcResult);

            // Assert
            Assert.AreEqual(80.64m, totalCost);
        }

        [TestMethod]
        public void GetEnglandWithBadDebtProvisionForComms_ShouldReturnCorrectTotal()
        {
            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetEnglandWithBadDebtProvisionForComms(CalcResultSummaryUtilTests.ProjectedMaterialsLookup(producers), producers[0], material, calcResult);

            // Assert
            Assert.AreEqual(213.696m, totalCost);
        }

        [TestMethod]
        public void GetWalesWithBadDebtProvisionForComms_ShouldReturnCorrectTotal()
        {
            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetWalesWithBadDebtProvisionForComms(CalcResultSummaryUtilTests.ProjectedMaterialsLookup(producers), producers[0], material, calcResult);

            // Assert
            Assert.AreEqual(53.424m, totalCost);
        }

        [TestMethod]
        public void GetScotlandWithBadDebtProvisionForComms_ShouldReturnCorrectTotal()
        {
            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetScotlandWithBadDebtProvisionForComms(CalcResultSummaryUtilTests.ProjectedMaterialsLookup(producers), producers[0], material, calcResult);

            // Assert
            Assert.AreEqual(80.136m, totalCost);
        }

        [TestMethod]
        public void GetNorthernIrelandWithBadDebtProvisionForComms_ShouldReturnCorrectTotal()
        {
            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetNorthernIrelandWithBadDebtProvisionForComms(CalcResultSummaryUtilTests.ProjectedMaterialsLookup(producers), producers[0], material, calcResult);

            // Assert
            Assert.AreEqual(186.984m, totalCost);
        }

        [TestMethod]
        public void GetPriceperTonneForComms_ShouldReturnCorrectTotal()
        {
            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(material, calcResult);

            // Assert
            Assert.AreEqual(0.42m, totalCost);
        }

        [TestMethod]
        public void GetPriceperTonneForComms_WhenNoMaterialMatch_ShouldReturn0()
        {
            // Arrange
            material.Code = "Aluminium";

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(material, calcResult);

            // Assert
            Assert.AreEqual(0m, totalCost);
        }

        [TestMethod]
        public void GetProducerTotalCostwithBadDebtProvision_ShouldReturnCorrectTotal()
        {
            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostwithBadDebtProvision(CalcResultSummaryUtilTests.ProjectedMaterialsLookup(producers), producers[0], material, calcResult);

            // Assert
            Assert.AreEqual(534.2400m, totalCost);
        }

        [TestMethod]
        public void GetProducerTotalCostWithoutBadDebtProvision_ShouldReturnCorrectTotal()
        {
            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostWithoutBadDebtProvision(CalcResultSummaryUtilTests.ProjectedMaterialsLookup(producers), producers[0], material, calcResult);

            // Assert
            Assert.AreEqual(504.00m, totalCost);
        }

        [TestMethod]
        public void GetProducerTotalCostwithBadDebtProvisionTotal_ShouldReturnCorrectTotal()
        {
            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostwithBadDebtProvisionTotal(CalcResultSummaryUtilTests.ProjectedMaterialsLookup(producers), producers, material, calcResult);

            // Assert
            Assert.AreEqual(1424.6400m, totalCost);
        }

        [TestMethod]
        public void GetTotalReportedTonnageTotalForHDCShouldReturnCorrectTotal()
        {
            // Arrange
            var material = GetHDCMaterial();

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetTotalReportedTonnageTotal(CalcResultSummaryUtilTests.ProjectedMaterialsLookup(producers), producers, material);

            // Assert
            Assert.AreEqual(280, totalCost);
        }

        [TestMethod]
        public void GetTotalReportedTonnageTotalShouldReturnCorrectTotal()
        {
            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetTotalReportedTonnageTotal(CalcResultSummaryUtilTests.ProjectedMaterialsLookup(producers), producers, material);

            // Assert
            Assert.AreEqual(3200, totalCost);
        }

        private static List<ProducerDetail> GetProducers()
        {
            var producers = TestDataHelper.GetProducers();

            foreach (var subPeriod in new[] {"2025-H1", "2025-H2"})
            {
                producers[0].ProducerReportedMaterials.Add(new ProducerReportedMaterial
                {
                    MaterialId = 1,
                    ProducerDetailId = 1,
                    PackagingType = "AL",
                    PackagingTonnage = 50,
                    SubmissionPeriod = subPeriod,
                    Material = new Material
                    {
                        Id = 1,
                        Code = "HH",
                        Name = "Material1",
                        Description = "Material1",
                    },
                });

                producers[0].ProducerReportedMaterials.Add(new ProducerReportedMaterial
                {
                    MaterialId = 3,
                    ProducerDetailId = 2,
                    PackagingType = "HDC",
                    PackagingTonnage = 50,
                    SubmissionPeriod = subPeriod,
                    Material = new Material
                    {
                        Id = 3,
                        Code = "GL",
                        Name = "Material2",
                        Description = "Material2",
                    },
                });

                producers[0].ProducerReportedMaterials.Add(new ProducerReportedMaterial
                {
                    MaterialId = 1,
                    ProducerDetailId = 3,
                    PackagingType = "PB",
                    PackagingTonnage = 100,
                    SubmissionPeriod = subPeriod,
                    Material = new Material
                    {
                        Id = 1,
                        Code = "AL",
                        Name = "Material1",
                        Description = "Material1",
                    },
                });
            }

            return producers;
        }

        private static MaterialDetail GetMaterial()
        {
            var material = new MaterialDetail
            {
                Id = 1,
                Code = "AL",
                Name = "Material1",
                Description = "Material1",
            };
            return material;
        }

        private static MaterialDetail GetHDCMaterial()
        {
            var material = new MaterialDetail
            {
                Id = 3,
                Code = "GL",
                Name = "Material2",
                Description = "Material2",
            };
            return material;
        }

        private CalcResultLateReportingTonnage GetCalcResultLateReportingTonnage()
        {
            return Fixture.Create<CalcResultLateReportingTonnage>();
        }
    }
}
