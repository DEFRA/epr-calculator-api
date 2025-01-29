namespace EPR.Calculator.API.UnitTests.Builder.Summary.Common
{
    using AutoFixture;
    using EPR.Calculator.API.Builder.Summary.Common;
    using EPR.Calculator.API.Enums;
    using EPR.Calculator.API.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultSummaryUtilTests
    {
        private readonly CalcResult _calcResult;

        private Fixture Fixture { get; init; } = new Fixture();

        public CalcResultSummaryUtilTests()
        {
            _calcResult = new CalcResult
            {
                CalcResultParameterOtherCost = TestDataHelper.GetCalcResultParameterOtherCost(),
                CalcResultDetail = TestDataHelper.GetCalcResultDetail(),
                CalcResultLaDisposalCostData = TestDataHelper.GetCalcResultLaDisposalCostData(),
                CalcResultLapcapData = TestDataHelper.GetCalcResultLapcapData(),
                CalcResultOnePlusFourApportionment = TestDataHelper.GetCalcResultOnePlusFourApportionment(),
                CalcResultParameterCommunicationCost = GetCalcResultParameterCommunicationCost(),
                CalcResultSummary = TestDataHelper.GetCalcResultSummary(),
                CalcResultCommsCostReportDetail = TestDataHelper.GetCalcResultCommsCostReportDetail(),
                CalcResultLateReportingTonnageData = GetCalcResultLateReportingTonnage(),
            };
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
            var result = CalcResultSummaryUtil.GetHouseholdPackagingWasteTonnage(producer, material);

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
            var result = CalcResultSummaryUtil.GetHouseholdPackagingWasteTonnageProducerTotal(producers, material);

            // Assert
            Assert.AreEqual(3000.00m, result);
        }

        [TestMethod]
        public void CanGetManagedConsumerWasteTonnage()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetManagedConsumerWasteTonnage(producer, material);

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
            var result = CalcResultSummaryUtil.GetManagedConsumerWasteTonnageProducerTotal(producers, material);

            // Assert
            Assert.AreEqual(60.00m, result);
        }

        [TestMethod]
        public void CanGetNetReportedTonnage()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetNetReportedTonnage(producer, material);

            // Assert
            Assert.AreEqual(980.00m, result);
        }

        [TestMethod]
        public void CanGetNetReportedTonnageProducerTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetNetReportedTonnageProducerTotal(producers, material);

            // Assert
            Assert.AreEqual(2940.00m, result);
        }

        [TestMethod]
        public void CanGetPricePerTonne()
        {
            // Arrange
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetPricePerTonne(material, _calcResult);

            // Assert
            Assert.AreEqual(0.6676m, result);
        }

        [TestMethod]
        public void CanGetProducerDisposalFee()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p =>p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetProducerDisposalFee(producer, material, _calcResult);

            // Assert
            Assert.AreEqual(654.248000m, result);
        }

        [TestMethod]
        public void CanGetProducerDisposalFeeProducerTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetProducerDisposalFeeProducerTotal(producers, material, _calcResult);

            // Assert
            Assert.AreEqual(1962.744000m, result);
        }

        [TestMethod]
        public void CanGetBadDebtProvision()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetBadDebtProvision(producer, material, _calcResult);

            // Assert
            Assert.AreEqual(39.254880m, result);
        }

        [TestMethod]
        public void CanGetBadDebtProvisionProducerTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetBadDebtProvisionProducerTotal(producers, material, _calcResult);

            // Assert
            Assert.AreEqual(117.764640m, result);
        }

        [TestMethod]
        public void CanGetProducerDisposalFeeWithBadDebtProvision()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvision(producer, material, _calcResult);

            // Assert
            Assert.AreEqual(693.50288000m, result);
        }

        [TestMethod]
        public void CanGetProducerDisposalFeeWithBadDebtProvisionProducerTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvisionProducerTotal(producers, material, _calcResult);

            // Assert
            Assert.AreEqual(2080.50864000m, result);
        }

        [TestMethod]
        public void CanGetEnglandWithBadDebtProvision()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetEnglandWithBadDebtProvision(producer, material, _calcResult);

            // Assert
            Assert.AreEqual(374.8295162135948480m, result);
        }

        [TestMethod]
        public void CanGetWalesWithBadDebtProvision()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetWalesWithBadDebtProvision(producer, material, _calcResult);

            // Assert
            Assert.AreEqual(84.4902597789384960m, result);
        }

        [TestMethod]
        public void CanGetScotlandWithBadDebtProvision()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetScotlandWithBadDebtProvision(producer, material, _calcResult);

            // Assert
            Assert.AreEqual(168.2977700641839840m, result);
        }

        [TestMethod]
        public void CanGetNorthernIrelandWithBadDebtProvision()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetNorthernIrelandWithBadDebtProvision(producer, material, _calcResult);

            // Assert
            Assert.AreEqual(65.8853339432826720m, result);
        }

        [TestMethod]
        public void CanGetEnglandWithBadDebtProvisionProducerTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetEnglandWithBadDebtProvisionProducerTotal(producers, material, _calcResult);

            // Assert
            Assert.AreEqual(1124.4885486407845440m, result);
        }

        [TestMethod]
        public void CanGetWalesWithBadDebtProvisionProducerTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetWalesWithBadDebtProvisionProducerTotal(producers, material, _calcResult);

            // Assert
            Assert.AreEqual(253.4707793368154880m, result);
        }

        [TestMethod]
        public void CanGetScotlandWithBadDebtProvisionProducerTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetScotlandWithBadDebtProvisionProducerTotal(producers, material, _calcResult);

            // Assert
            Assert.AreEqual(504.8933101925519520m, result);
        }

        [TestMethod]
        public void CanGetNorthernIrelandWithBadDebtProvisionProducerTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetNorthernIrelandWithBadDebtProvisionProducerTotal(producers, material, _calcResult);

            // Assert
            Assert.AreEqual(197.6560018298480160m, result);
        }

        [TestMethod]
        public void CanGetCountryApportionmentPercentage()
        {
            // Arrange
            var expectedResult = new CalcResultLapcapDataDetails
            {
                Name = "1 Country Apportionment %s",
                EnglandDisposalCost = "54.04873246%",
                WalesDisposalCost = "12.18311592%",
                ScotlandDisposalCost = "24.26778243%",
                NorthernIrelandDisposalCost = "9.50036919%",
                TotalDisposalCost = "100.00000000%",
                EnglandCost = 54.04873246369677m,
                WalesCost = 12.183115924193945m,
                ScotlandCost = 24.267782426778243m,
                NorthernIrelandCost = 9.500369185331037m,
                TotalCost = 100,
                OrderId = 11
            };

            // Act
            var result = CalcResultSummaryUtil.GetCountryApportionmentPercentage(_calcResult);

            // Assert
            Assert.AreEqual(54.04873246369677m, result?.EnglandCost);
            Assert.AreEqual(12.183115924193945m, result?.WalesCost);
            Assert.AreEqual(24.267782426778243m, result?.ScotlandCost);
            Assert.AreEqual(9.500369185331037m, result?.NorthernIrelandCost);
        }

        [TestMethod]
        public void CanGetTotalProducerDisposalFee()
        {
            // Arrange
            var materialCostSummary = TestDataHelper.GetProducerDisposalFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetTotalProducerDisposalFee(materialCostSummary);

            // Assert
            Assert.AreEqual(5233.40m, result);
        }

        [TestMethod]
        public void CanGetTotalBadDebtProvision()
        {
            // Arrange
            var materialCostSummary = TestDataHelper.GetProducerDisposalFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetTotalBadDebtProvision(materialCostSummary);

            // Assert
            Assert.AreEqual(314.01m, result);
        }

        [TestMethod]
        public void CanGetTotalProducerDisposalFeeWithBadDebtProvision()
        {
            // Arrange
            var materialCostSummary = TestDataHelper.GetProducerDisposalFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary);

            // Assert
            Assert.AreEqual(5547.40m, result);
        }

        [TestMethod]
        public void CanGetEnglandTotal()
        {
            // Arrange
            var materialCostSummary = TestDataHelper.GetProducerDisposalFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetEnglandTotal(materialCostSummary);

            // Assert
            Assert.AreEqual(2998.30m, result);
        }

        [TestMethod]
        public void CanGetWalesTotal()
        {
            // Arrange
            var materialCostSummary = TestDataHelper.GetProducerDisposalFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetWalesTotal(materialCostSummary);

            // Assert
            Assert.AreEqual(675.85m, result);
        }

        [TestMethod]
        public void CanGetScotlandTotal()
        {
            // Arrange
            var materialCostSummary = TestDataHelper.GetProducerDisposalFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetScotlandTotal(materialCostSummary);

            // Assert
            Assert.AreEqual(1346.22m, result);
        }

        [TestMethod]
        public void CanGetNorthernIrelandTotal()
        {
            // Arrange
            var materialCostSummary = TestDataHelper.GetProducerDisposalFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetNorthernIrelandTotal(materialCostSummary);

            // Assert
            Assert.AreEqual(527.03m, result);
        }

        [TestMethod]
        public void CanGetTotal1Plus2ABadDebt()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var materials = TestDataHelper.GetMaterials();

            // Act
            var result = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, _calcResult);

            // Assert
            Assert.AreEqual(2080.50864000m, result);
        }

        [TestMethod]
        public void CanGetTotalProducerCommsFee()
        {
            // Arrange
            var commCostSummary = TestDataHelper.GetProducerCommsFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetTotalProducerCommsFee(commCostSummary);

            // Assert
            Assert.AreEqual(1358.98m, result);
        }

        [TestMethod]
        public void CanGetCommsTotalBadDebtProvision()
        {
            // Arrange
            var commCostSummary = TestDataHelper.GetProducerCommsFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetCommsTotalBadDebtProvision(commCostSummary);

            // Assert
            Assert.AreEqual(81.53m, result);
        }

        [TestMethod]
        public void CanGetTotalProducerCommsFeeWithBadDebtProvision()
        {
            // Arrange
            var commCostSummary = TestDataHelper.GetProducerCommsFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(commCostSummary);

            // Assert
            Assert.AreEqual(1440.51m, result);
        }

        [TestMethod]
        public void CanGetEnglandCommsTotal()
        {
            // Arrange
            var commCostSummary = TestDataHelper.GetProducerCommsFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetEnglandCommsTotal(commCostSummary);

            // Assert
            Assert.AreEqual(756.17m, result);
        }

        [TestMethod]
        public void CanGetWalesCommsTotal()
        {
            // Arrange
            var commCostSummary = TestDataHelper.GetProducerCommsFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetWalesCommsTotal(commCostSummary);

            // Assert
            Assert.AreEqual(190.86m, result);
        }

        [TestMethod]
        public void CanGetScotlandCommsTotal()
        {
            // Arrange
            var commCostSummary = TestDataHelper.GetProducerCommsFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetScotlandCommsTotal(commCostSummary);

            // Assert
            Assert.AreEqual(350.43m, result);
        }

        [TestMethod]
        public void CanGetNorthernIrelandCommsTotal()
        {
            // Arrange
            var commCostSummary = TestDataHelper.GetProducerCommsFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetNorthernIrelandCommsTotal(commCostSummary);

            // Assert
            Assert.AreEqual(143.06m, result);
        }

        [TestMethod]
        public void CanGetCommsCostHeaderWithoutBadDebtFor2bTitle()
        {
            // Act
            var result = CalcResultSummaryUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(_calcResult);

            // Assert
            Assert.AreEqual(2530, result);
        }

        [TestMethod]
        public void CanGetCommsCostHeaderBadDebtProvisionFor2bTitle()
        {
            _calcResult.CalcResultSummary.CommsCostHeaderWithoutBadDebtFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(_calcResult);
            // Act
            var result = CalcResultSummaryUtil.GetCommsCostHeaderBadDebtProvisionFor2bTitle(_calcResult,_calcResult.CalcResultSummary);

            // Assert
            Assert.AreEqual(151.80m, result);
        }

        [TestMethod]
        public void CanGetCommsCostHeaderWithBadDebtFor2bTitle()
        {
            _calcResult.CalcResultSummary.CommsCostHeaderWithoutBadDebtFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(_calcResult);
            _calcResult.CalcResultSummary.CommsCostHeaderBadDebtProvisionFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderBadDebtProvisionFor2bTitle(_calcResult, _calcResult.CalcResultSummary);

            // Act
            var result = CalcResultSummaryUtil.GetCommsCostHeaderWithBadDebtFor2bTitle(_calcResult.CalcResultSummary);

            // Assert
            Assert.AreEqual(2681.80m, result);
        }

        [TestMethod]
        public void CanGetCountryOnePlusFourApportionmentForEngland()
        {
            // Act
            var result = CalcResultSummaryUtil.GetCountryOnePlusFourApportionment(_calcResult, Countries.England);

            // Assert
            Assert.AreEqual(14.53m, result);
        }

        [TestMethod]
        public void CanGetCountryOnePlusFourApportionmentForWales()
        {
            // Act
            var result = CalcResultSummaryUtil.GetCountryOnePlusFourApportionment(_calcResult, Countries.Wales);

            // Assert
            Assert.AreEqual(20, result);
        }

        [TestMethod]
        public void CanGetCountryOnePlusFourApportionmentForScotland()
        {
            // Act
            var result = CalcResultSummaryUtil.GetCountryOnePlusFourApportionment(_calcResult, Countries.Scotland);

            // Assert
            Assert.AreEqual(0.15m, result);
        }

        [TestMethod]
        public void CanGetCountryOnePlusFourApportionmentForNorthernIreland()
        {
            // Act
            var result = CalcResultSummaryUtil.GetCountryOnePlusFourApportionment(_calcResult, Countries.NorthernIreland);

            // Assert
            Assert.AreEqual(0.15m, result);
        }

        [TestMethod]
        public void GetParamsOtherFourCountryApportionmentPercentageForEngland()
        {
            // Act
            var result = CalcResultSummaryUtil.GetParamsOtherFourCountryApportionmentPercentage(_calcResult, Countries.England);

            // Assert
            Assert.AreEqual(43.83561643835616m, result);
        }

        [TestMethod]
        public void GetParamsOtherFourCountryApportionmentPercentageForWales()
        {
            // Act
            var result = CalcResultSummaryUtil.GetParamsOtherFourCountryApportionmentPercentage(_calcResult, Countries.Wales);

            // Assert
            Assert.AreEqual(19.17808219178082m, result);
        }

        [TestMethod]
        public void GetParamsOtherFourCountryApportionmentPercentageForScotland()
        {
            // Act
            var result = CalcResultSummaryUtil.GetParamsOtherFourCountryApportionmentPercentage(_calcResult, Countries.Scotland);

            // Assert
            Assert.AreEqual(24.65753424657534m, result);
        }

        [TestMethod]
        public void GetParamsOtherFourCountryApportionmentPercentageForNorthernIreland()
        {
            // Act
            var result = CalcResultSummaryUtil.GetParamsOtherFourCountryApportionmentPercentage(_calcResult, Countries.NorthernIreland);

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
            var result = CalcResultSummaryUtil.GetReportedPublicBinTonnage(producer, material);

            // Assert
            Assert.AreEqual(20.00m, result);
        }

        [TestMethod]
        public void CanGetReportedPublicBinTonnageTotal()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "PL");

            // Act
            var result = CalcResultSummaryUtil.GetReportedPublicBinTonnageTotal(producer, material);

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
            var result = CalcResultSummaryUtil.GetHDCGlassTonnage(producer, material);

            // Assert
            Assert.AreEqual(50.00m, result);
        }

        [TestMethod]
        public void CanGetReportedHDCTonnageTotal()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "GL");

            // Act
            var result = CalcResultSummaryUtil.GetHDCGlassTonnageTotal(producer, material);

            // Assert
            Assert.AreEqual(150.00m, result);
        }



        private CalcResultParameterCommunicationCost GetCalcResultParameterCommunicationCost()
        {
            return this.Fixture.Create<CalcResultParameterCommunicationCost>();
        }

        private CalcResultLateReportingTonnage GetCalcResultLateReportingTonnage()
        {
            return this.Fixture.Create<CalcResultLateReportingTonnage>();
        }
    }
}