using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.Common
{
    [TestClass]
    public class CalcResultSummaryUtilTests
    {
        private readonly CalcResult calcResult;

        private Fixture Fixture { get; init; } = new Fixture();

        public CalcResultSummaryUtilTests()
        {
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
                CalcResultProjectedProducers = new CalcResultProjectedProducers(),
            };
        }

        public static ILookup<(int, string?), ProducerReportedMaterialProjected> ProjectedMaterialsLookup(List<ProducerDetail> producers)
        {
            // This allows us to retrofit into existing test setup, but ProducerReportedMaterials normally
            // refers to pre-processed data, which is _not_ what we want to display in the ResultsSummary
            ProducerReportedMaterialProjected ToProjected(ProducerReportedMaterial rm)
            {
                return new ProducerReportedMaterialProjected
                {
                    MaterialId = rm.MaterialId,
                    ProducerDetailId = rm.ProducerDetailId,
                    PackagingType = rm.PackagingType,
                    PackagingTonnage = rm.PackagingTonnage,
                    PackagingTonnageRed = rm.PackagingTonnageRed,
                    PackagingTonnageAmber = rm.PackagingTonnageAmber,
                    PackagingTonnageGreen = rm.PackagingTonnageGreen,
                    PackagingTonnageRedMedical = rm.PackagingTonnageRedMedical,
                    PackagingTonnageAmberMedical = rm.PackagingTonnageAmberMedical,
                    PackagingTonnageGreenMedical = rm.PackagingTonnageGreenMedical,
                    SubmissionPeriod = rm.SubmissionPeriod
                };
            }
            return producers
                .SelectMany(p => p.ProducerReportedMaterials.Select(rm => (Key: (p.ProducerId, p.SubsidiaryId), Rm: ToProjected(rm))))
                .ToLookup(x => x.Key, x => x.Rm);
        }

        [TestMethod]
        public void CanGetNonTotalRowLevelIndex()
        {
            // Arrange
            var producerDisposalFeesLookup = TestDataHelper.GetProducerDisposalFees();
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);

            // Act
            var result = CalcResultSummaryUtil.GetLevelIndex(producerDisposalFeesLookup, producer);

            // Assert
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void CanGetHouseholdPackagingWasteTonnage()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetTonnage(ProjectedMaterialsLookup(new List<ProducerDetail> { producer } ), producer, material, PackagingTypes.Household);

            // Assert
            Assert.AreEqual(1000.00m, result);
        }

        [TestMethod]
        public void CanGetPublicBinTonnage()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "PL");

            // Act
            var result = CalcResultSummaryUtil.GetTonnage(ProjectedMaterialsLookup(new List<ProducerDetail> { producer } ), producer, material, PackagingTypes.PublicBin);

            // Assert
            Assert.AreEqual(20.00m, result);
        }

        [TestMethod]
        public void CanGetHouseholdDrinksContainersTonnage()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "GL");

            // Act
            var result = CalcResultSummaryUtil.GetTonnage(ProjectedMaterialsLookup(new List<ProducerDetail> { producer } ), producer, material, PackagingTypes.HouseholdDrinksContainers);

            // Assert
            Assert.AreEqual(20.00m, result);
        }

        [TestMethod]
        public void CanGetReportedTonnage()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetReportedTonnage(ProjectedMaterialsLookup(new List<ProducerDetail> { producer } ), producer, material);

            // Assert
            Assert.AreEqual(1000.00m, result);
        }

        [TestMethod]
        public void CanGetHouseholdPackagingWasteTonnageProducerTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetTonnageTotal(ProjectedMaterialsLookup(producers), producers, material, PackagingTypes.Household);

            // Assert
            Assert.AreEqual(3000.00m, result);
        }

        [TestMethod]
        public void CanGetPublicBinTonnageProducerTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "PL");

            // Act
            var result = CalcResultSummaryUtil.GetTonnageTotal(ProjectedMaterialsLookup(producers), producers, material, PackagingTypes.PublicBin);

            // Assert
            Assert.AreEqual(60.00m, result);
        }

        [TestMethod]
        public void CanGetReportedTonnageProducerTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetReportedTonnageTotal(ProjectedMaterialsLookup(producers), producers, material);

            // Assert
            Assert.AreEqual(3000.00m, result);
        }

        [TestMethod]
        public void CanGetHouseholdDrinksContainersTonnageProducerTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "GL");

            // Act
            var result = CalcResultSummaryUtil.GetTonnageTotal(ProjectedMaterialsLookup(producers), producers, material, PackagingTypes.HouseholdDrinksContainers);

            // Assert
            Assert.AreEqual(60.00m, result);
        }

        [TestMethod]
        public void CanGetManagedConsumerWasteTonnage()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetTonnage(ProjectedMaterialsLookup( new List<ProducerDetail>{ producer }), producer, material, PackagingTypes.ConsumerWaste);

            // Assert
            Assert.AreEqual(20.00m, result);
        }

        [TestMethod]
        public void CanGetManagedConsumerWasteTonnageProducerTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetTonnageTotal(ProjectedMaterialsLookup(producers), producers, material, PackagingTypes.ConsumerWaste);

            // Assert
            Assert.AreEqual(60.00m, result);
        }

        [TestMethod]
        public void CanGetPricePerTonne_NonMatchingMaterial()
        {
            // Arrange
            var material = Fixture.Create<MaterialDetail>();

            // Act
            var result = CalcResultSummaryUtil.GetPricePerTonne(material, calcResult);

            // Assert
            Assert.AreEqual((total: null, red: null, amber: null, green: null), result);
        }

        [TestMethod]
        public void CanGetPricePerTonne()
        {
            // Arrange
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetPricePerTonne(material, calcResult);

            // Assert
            Assert.AreEqual((total: 0.5889m, red: null, amber: null, green: null), result);
        }

        [TestMethod]
        public void CanGetProducerDisposalFee()
        {
            // Arrange
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetProducerDisposalFee(material, calcResult, SelfManagedConsumerWasteData.Zero);

            // Assert
            Assert.AreEqual((total: 0m, red: null, amber: null, green: null), result);
        }

        [TestMethod]
        public void CanGetProducerDisposalFee_WithModulation()
        {
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            calcResult.CalcResultModulation = new ModulationResult
            {
                GreenFactor = 2,
                RedFactor   = 4,
                MaterialModulation = new Dictionary<MaterialDetail, MaterialModulation>
                {
                    [material] = mkMaterialModulation(adc: 100, rdc: 120,  gdc: 77.1423m, at: 90, rt: 220, gt: 550, rAtAdc: 22000, gAtAdc: 55000),
                }
            };

            var smcw = new SelfManagedConsumerWasteData
            {
                SelfManagedConsumerWasteTonnage = 0,
                ActionedSelfManagedConsumerWasteTonnage = (0, 0m, 0m, 0m),
                ResidualSelfManagedConsumerWasteTonnage = 0,
                NetReportedTonnage = (null, 1m, 2m, 3m)
            };

            var result = CalcResultSummaryUtil.GetProducerDisposalFee(material, calcResult, smcw);

            Assert.AreEqual((total: 551.4269m, red: 120, amber: 200, green: 231.4269m), result);
        }


        [TestMethod]
        public void GetBadDebtProvision_ValidPercentage_WithPercent()
        {
            var result = CalcResultSummaryUtil.GetBadDebtProvision(calcResult, 200m);
            Assert.AreEqual(12m, result);
        }

        [TestMethod]
        public void GetProducerDisposalFeeWithBadDebtProvision_AddsPercentage()
        {
            var result = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvision(calcResult, 100m);
            Assert.AreEqual(106m, result);
        }

        [TestMethod]
        public void GetCountryBadDebtProvision()
        {
            Assert.AreEqual(57.291656411518582328328821072m, CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.England, 100m));
            Assert.AreEqual(12.914102879645582082205267049m, CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.Wales  , 100m));
        }

        [TestMethod]
        public void CanGetCommsCostHeaderWithoutBadDebtFor2bTitle()
        {
            // Act
            var result = CalcResultSummaryUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(calcResult);

            // Assert
            Assert.AreEqual(2531, result);
        }

        [TestMethod]
        public void CanGetCommsCostHeaderBadDebtProvisionFor2bTitle()
        {
            calcResult.CalcResultSummary.CommsCostHeaderWithoutBadDebtFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(calcResult);

            // Act
            var result = CalcResultSummaryUtil.GetCommsCostHeaderBadDebtProvisionFor2bTitle(calcResult, calcResult.CalcResultSummary);

            // Assert
            Assert.AreEqual(151.86m, result);
        }

        [TestMethod]
        public void CanGetCommsCostHeaderWithBadDebtFor2bTitle()
        {
            calcResult.CalcResultSummary.CommsCostHeaderWithoutBadDebtFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(calcResult);
            calcResult.CalcResultSummary.CommsCostHeaderBadDebtProvisionFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderBadDebtProvisionFor2bTitle(calcResult, calcResult.CalcResultSummary);

            // Act
            var result = CalcResultSummaryUtil.GetCommsCostHeaderWithBadDebtFor2bTitle(calcResult.CalcResultSummary);

            // Assert
            Assert.AreEqual(2682.86m, result);
        }

        [TestMethod]
        public void CanGetCountryOnePlusFourApportionmentForEngland()
        {
            // Act
            var result = CalcResultSummaryUtil.GetCountryOnePlusFourApportionment(calcResult, Countries.England);

            // Assert
            Assert.AreEqual(40m, result);
        }

        [TestMethod]
        public void CanGetCountryOnePlusFourApportionmentForWales()
        {
            // Act
            var result = CalcResultSummaryUtil.GetCountryOnePlusFourApportionment(calcResult, Countries.Wales);

            // Assert
            Assert.AreEqual(10m, result);
        }

        [TestMethod]
        public void CanGetCountryOnePlusFourApportionmentForScotland()
        {
            // Act
            var result = CalcResultSummaryUtil.GetCountryOnePlusFourApportionment(calcResult, Countries.Scotland);

            // Assert
            Assert.AreEqual(15m, result);
        }

        [TestMethod]
        public void CanGetCountryOnePlusFourApportionmentForNorthernIreland()
        {
            // Act
            var result = CalcResultSummaryUtil.GetCountryOnePlusFourApportionment(calcResult, Countries.NorthernIreland);

            // Assert
            Assert.AreEqual(35m, result);
        }

        [TestMethod]
        public void CanGetDefaultOnePlusFourApportionment()
        {
            // Act
            var result = CalcResultSummaryUtil.GetCountryOnePlusFourApportionment(calcResult, (Countries)(-1));

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void GetParamsOtherFourCountryApportionmentPercentageForEngland()
        {
            // Act
            var result = CalcResultSummaryUtil.GetParamsOtherFourCountryApportionmentPercentage(calcResult, Countries.England);

            // Assert
            Assert.AreEqual(43.83561643835616m, result);
        }

        [TestMethod]
        public void GetParamsOtherFourCountryApportionmentPercentageForWales()
        {
            // Act
            var result = CalcResultSummaryUtil.GetParamsOtherFourCountryApportionmentPercentage(calcResult, Countries.Wales);

            // Assert
            Assert.AreEqual(19.17808219178082m, result);
        }

        [TestMethod]
        public void GetParamsOtherFourCountryApportionmentPercentageForScotland()
        {
            // Act
            var result = CalcResultSummaryUtil.GetParamsOtherFourCountryApportionmentPercentage(calcResult, Countries.Scotland);

            // Assert
            Assert.AreEqual(24.65753424657534m, result);
        }

        [TestMethod]
        public void GetParamsOtherFourCountryApportionmentPercentageForNorthernIreland()
        {
            // Act
            var result = CalcResultSummaryUtil.GetParamsOtherFourCountryApportionmentPercentage(calcResult, Countries.NorthernIreland);

            // Assert
            Assert.AreEqual(12.32876712328767m, result);
        }

        [TestMethod]
        public void CanGetReportedPublicBinTonnage()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 2);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "PL");

            // Act
            var result = CalcResultSummaryUtil.GetTonnage(ProjectedMaterialsLookup(new List<ProducerDetail> { producer } ), producer, material, PackagingTypes.PublicBin);

            // Assert
            Assert.AreEqual(20.00m, result);
        }

        [TestMethod]
        public void CanGetReportedPublicBinTonnageTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "PL");

            // Act
            var result = CalcResultSummaryUtil.GetTonnageTotal(ProjectedMaterialsLookup(producers), producers, material, PackagingTypes.PublicBin);

            // Assert
            Assert.AreEqual(60.00m, result);
        }

        [TestMethod]
        public void CanGetReportedHDCTonnage()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "GL");

            // Act
            var result = CalcResultSummaryUtil.GetTonnage(ProjectedMaterialsLookup(new List<ProducerDetail> { producer } ), producer, material, PackagingTypes.HouseholdDrinksContainers);

            // Assert
            Assert.AreEqual(20.00m, result);
        }

        [TestMethod]
        public void CanGetReportedHDCTonnageTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "GL");

            // Act
            var result = CalcResultSummaryUtil.GetTonnageTotal(ProjectedMaterialsLookup(producers), producers, material, PackagingTypes.HouseholdDrinksContainers);

            // Assert
            Assert.AreEqual(60.00m, result);
        }

        private CalcResultLateReportingTonnage GetCalcResultLateReportingTonnage()
        {
            return Fixture.Create<CalcResultLateReportingTonnage>();
        }

        private MaterialModulation mkMaterialModulation(decimal adc, decimal rdc, decimal gdc, decimal at, decimal rt, decimal gt, decimal rAtAdc, decimal gAtAdc)
        {
            return new MaterialModulation
            {
                AmberMaterialDisposalCost = adc,
                RedMaterialDisposalCost   = rdc,
                GreenMaterialDisposalCost = gdc,
                AmberMaterialTonnages     = at,
                RedMaterialTonnages       = rt,
                GreenMaterialTonnages     = gt,
                TotalRedMaterialAtAmberDisposalCost   = rAtAdc,
                TotalGreenMaterialAtAmberDisposalCost = gAtAdc
            };
        }
    }
}
