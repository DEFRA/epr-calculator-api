using AutoFixture;
using EPR.Calculator.API.Builder.Summary.CommsCostTwoBTotalBill;
using EPR.Calculator.API.Builder.Summary;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests.Builder.Summary.CommsCostTwoBTotalBill
{
    [TestClass]
    public class CalcResultSummaryCommsCostTwoBTotalBillTests
    {
        private CalcResult _calcResult;
        private List<ProducerDetail> _producers;
        private List<CalcResultsProducerAndReportMaterialDetail> _allResults;
        private Fixture Fixture { get; init; } = new Fixture();

        [TestInitialize]
        public void Setup()
        {
            _producers = GetProducers();

            _calcResult = new CalcResult
            {
                CalcResultParameterOtherCost = TestDataHelper.GetCalcResultParameterOtherCost(),
                CalcResultDetail = TestDataHelper.GetCalcResultDetail(),
                CalcResultLaDisposalCostData = TestDataHelper.GetCalcResultLaDisposalCostData(),
                CalcResultLapcapData = TestDataHelper.GetCalcResultLapcapData(),
                CalcResultOnePlusFourApportionment = GetCalcResultOnePlusFourApportionment(),
                CalcResultParameterCommunicationCost = GetCalcResultParameterCommunicationCost(),
                CalcResultSummary = TestDataHelper.GetCalcResultSummary(),
                CalcResultCommsCostReportDetail = TestDataHelper.GetCalcResultCommsCostReportDetail(),
                CalcResultLateReportingTonnageData = GetCalcResultLateReportingTonnage()
            };

            // Set up consistent data
            _calcResult.CalcResultParameterOtherCost = Fixture.Create<CalcResultParameterOtherCost>();
            _calcResult.CalcResultParameterOtherCost.BadDebtProvision = new KeyValuePair<string, string>("10 Bad Debt Provision", "10.00%");

           _allResults = new List<CalcResultsProducerAndReportMaterialDetail>
            {
                new CalcResultsProducerAndReportMaterialDetail
                {
                    ProducerDetail = new ProducerDetail
                    {
                        Id = 1,
                        CalculatorRunId = 1,
                        SubsidiaryId = "1",
                        ProducerId = 1,
                        ProducerName = "Producer1"
                    },
                    ProducerReportedMaterial = new ProducerReportedMaterial
                    {
                        Id = 1,
                        MaterialId = 1,
                        ProducerDetailId = 1,
                        PackagingType = "HH",
                        PackagingTonnage = 100,
                        Material = new Material
                        {
                            Id = 1,
                            Code = "HH",
                            Name = "Material1",
                            Description = "Material1"
                        }
                    }
                },
                new CalcResultsProducerAndReportMaterialDetail
                {
                    ProducerDetail = new ProducerDetail
                    {
                        Id = 2,
                        CalculatorRunId = 1,
                        SubsidiaryId = "1",
                        ProducerId = 2,
                        ProducerName = "Producer2"
                    },
                    ProducerReportedMaterial = new ProducerReportedMaterial
                    {
                        Id = 2,
                        MaterialId = 1,
                        ProducerDetailId = 2,
                        PackagingType = "HH",
                        PackagingTonnage = 900,
                        Material = new Material
                        {
                            Id = 1,
                            Code = "HH",
                            Name = "Material1",
                            Description = "Material1"
                        }
                    }
                }
            };
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _producers = null;
            _allResults = null;
            _calcResult = null;
        }

        [TestMethod]
        public void GetCommsProducerFeeWithoutBadDebtFor2bTotalsRow_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 253.0m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithoutBadDebtFor2bTotalsRow(_calcResult, _producers, _allResults);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetCommsBadDebtProvisionFor2bTotalsRow_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 25.300m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsBadDebtProvisionFor2bTotalsRow(_calcResult, _producers, _allResults);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetCommsProducerFeeWithBadDebtFor2bTotalsRow_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 278.300m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithBadDebtFor2bTotalsRow(_calcResult, _producers, _allResults);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetCommsEnglandWithBadDebtTotalsRow_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 139.1500m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsEnglandWithBadDebtTotalsRow(_calcResult, _producers, _allResults);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetCommsWalesWithBadDebtTotalsRow_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 55.6600m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsWalesWithBadDebtTotalsRow(_calcResult, _producers, _allResults);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetCommsScotlandWithBadDebtTotalsRow_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 55.6600m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsScotlandWithBadDebtTotalsRow(_calcResult, _producers, _allResults);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetCommsNorthernIrelandWithBadDebtTotalsRow_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 27.8300m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsNorthernIrelandWithBadDebtTotalsRow(_calcResult, _producers, _allResults);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetCommsEnglandWithBadDebt_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 139.1500m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsEnglandWithBadDebt(_calcResult, _producers[0], _allResults);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetCommsWalesWithBadDebt_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 55.6600m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsWalesWithBadDebt(_calcResult, _producers[0], _allResults);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetCommsScotlandWithBadDebt_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 55.6600m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsScotlandWithBadDebt(_calcResult, _producers[0], _allResults);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetCommsNorthernIrelandWithBadDebt_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 27.8300m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsNorthernIrelandWithBadDebt(_calcResult, _producers[0], _allResults);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetCommsWithBadDebt_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 139.1500m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsWithBadDebt(_calcResult, _producers[0], _allResults, "England");

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetRegionApportionment_ShouldReturnCorrectValue()
        {
            // Arrange
            decimal expectedValue = 0.50m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetRegionApportionment(_calcResult, "England");

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetCommsBadDebtProvisionFor2b_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 25.300m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsBadDebtProvisionFor2b(_calcResult, _producers[0], _allResults);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetCommsProducerFeeWithBadDebtFor2b_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 278.300m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithBadDebtFor2b(_calcResult, _producers[0], _allResults);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void CalculateProducerFee_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 253.0m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.CalculateProducerFee(_calcResult, _producers[0], _allResults, false);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetCommsProducerFeeWithoutBadDebtFor2b_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 253.0m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithoutBadDebtFor2b(_calcResult, _producers[0], _allResults);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        private List<ProducerDetail> GetProducers()
        {
            var producers = Fixture.CreateMany<ProducerDetail>(2).ToList();
            producers[0].SubsidiaryId = "1";
            producers[0].CalculatorRunId = 1;
            producers[0].ProducerId = 1;

            producers[0].ProducerReportedMaterials.Add(new ProducerReportedMaterial
            {
                Id = 1,
                MaterialId = 1,
                ProducerDetailId = 1,
                PackagingType = "HH",
                PackagingTonnage = 100,
                Material = new Material
                {
                    Id = 1,
                    Code = "HH",
                    Name = "Material1",
                    Description = "Material1"
                }
            });

            return producers;
        }

        private CalcResultParameterCommunicationCost GetCalcResultParameterCommunicationCost()
        {
            return this.Fixture.Create<CalcResultParameterCommunicationCost>();
        }

        private CalcResultLateReportingTonnage GetCalcResultLateReportingTonnage()
        {
            return this.Fixture.Create<CalcResultLateReportingTonnage>();
        }

        private CalcResultOnePlusFourApportionment GetCalcResultOnePlusFourApportionment()
        {
            var calcResultOnePlusFourApportionment = this.Fixture.Create<CalcResultOnePlusFourApportionment>();

            // Ensure the lists have enough elements
            calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails = this.Fixture.CreateMany<CalcResultOnePlusFourApportionmentDetail>(5).ToList();

            // Set up consistent data
            calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Last().EnglandDisposalTotal = "50%";
            calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Last().WalesDisposalTotal = "20%";
            calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Last().ScotlandDisposalTotal = "20%";
            calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Last().NorthernIrelandDisposalTotal = "10%";

            return calcResultOnePlusFourApportionment;
        }
    }
}