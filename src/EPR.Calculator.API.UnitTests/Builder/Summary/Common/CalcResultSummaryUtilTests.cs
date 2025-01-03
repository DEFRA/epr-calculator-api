namespace EPR.Calculator.API.UnitTests.Builder.Summary.Common
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using EPR.Calculator.API.Builder.Summary.Common;
    using EPR.Calculator.API.Constants;
    using EPR.Calculator.API.Data.DataModels;
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
                CalcResultParameterOtherCost = GetCalcResultParameterOtherCost(),
                CalcResultDetail = GetCalcResultDetail(),
                CalcResultLaDisposalCostData = GetCalcResultLaDisposalCostData(),
                CalcResultLapcapData = GetCalcResultLapcapData(),
                CalcResultOnePlusFourApportionment = GetCalcResultOnePlusFourApportionment(),
                CalcResultParameterCommunicationCost = GetCalcResultParameterCommunicationCost(),
                CalcResultSummary = GetCalcResultSummary(),
                CalcResultCommsCostReportDetail = GetCalcResultCommsCostReportDetail(),
                CalcResultLateReportingTonnageData = GetCalcResultLateReportingTonnage(),
            };
        }

        [TestMethod]
        public void CanGetNonTotalRowLevelIndex()
        {
            // Arrange
            var producerDisposalFeesLookup = GetProducerDisposalFees();
            var producer = GetProducers().First(p => p.Id == 1);

            // Act
            var result = CalcResultSummaryUtil.GetLevelIndex(producerDisposalFeesLookup, producer);

            // Assert
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void CanGetHouseholdPackagingWasteTonnage()
        {
            // Arrange
            var producer = GetProducers().First(p => p.Id == 1);
            var material = GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetHouseholdPackagingWasteTonnage(producer, material);

            // Assert
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void CanGetPricePerTonne()
        {
            // Arrange
            var material = GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetPricePerTonne(material, _calcResult);

            // Assert
            Assert.AreEqual(0.6676m, result);
        }

        [TestMethod]
        public void CanGetCountryApportionmentPercentage()
        {
            // Act
            var result = CalcResultSummaryUtil.GetCountryApportionmentPercentage(_calcResult);

            // Assert
            Assert.AreEqual(null, result);
        }

        [TestMethod]
        public void CanGetTotalProducerDisposalFee()
        {
            // Arrange
            var materialCostSummary = GetProducerDisposalFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetTotalProducerDisposalFee(materialCostSummary);

            // Assert
            Assert.AreEqual(5233.40m, result);
        }

        [TestMethod]
        public void CanGetTotalBadDebtProvision()
        {
            // Arrange
            var materialCostSummary = GetProducerDisposalFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetTotalBadDebtProvision(materialCostSummary);

            // Assert
            Assert.AreEqual(314.01m, result);
        }

        [TestMethod]
        public void CanGetTotalProducerDisposalFeeWithBadDebtProvision()
        {
            // Arrange
            var materialCostSummary = GetProducerDisposalFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary);

            // Assert
            Assert.AreEqual(5547.40m, result);
        }

        [TestMethod]
        public void CanGetEnglandTotal()
        {
            // Arrange
            var materialCostSummary = GetProducerDisposalFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetEnglandTotal(materialCostSummary);

            // Assert
            Assert.AreEqual(2998.30m, result);
        }

        [TestMethod]
        public void CanGetWalesTotal()
        {
            // Arrange
            var materialCostSummary = GetProducerDisposalFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetWalesTotal(materialCostSummary);

            // Assert
            Assert.AreEqual(675.85m, result);
        }

        [TestMethod]
        public void CanGetScotlandTotal()
        {
            // Arrange
            var materialCostSummary = GetProducerDisposalFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetScotlandTotal(materialCostSummary);

            // Assert
            Assert.AreEqual(1346.22m, result);
        }

        [TestMethod]
        public void CanGetNorthernIrelandTotal()
        {
            // Arrange
            var materialCostSummary = GetProducerDisposalFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetNorthernIrelandTotal(materialCostSummary);

            // Assert
            Assert.AreEqual(527.03m, result);
        }

        [TestMethod]
        public void CanGetTotalProducerCommsFee()
        {
            // Arrange
            var commCostSummary = GetProducerCommsFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetTotalProducerCommsFee(commCostSummary);

            // Assert
            Assert.AreEqual(1358.98m, result);
        }

        [TestMethod]
        public void CanGetCommsTotalBadDebtProvision()
        {
            // Arrange
            var commCostSummary = GetProducerCommsFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetCommsTotalBadDebtProvision(commCostSummary);

            // Assert
            Assert.AreEqual(81.53m, result);
        }

        [TestMethod]
        public void CanGetTotalProducerCommsFeeWithBadDebtProvision()
        {
            // Arrange
            var commCostSummary = GetProducerCommsFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(commCostSummary);

            // Assert
            Assert.AreEqual(1440.51m, result);
        }

        [TestMethod]
        public void CanGetEnglandCommsTotal()
        {
            // Arrange
            var commCostSummary = GetProducerCommsFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetEnglandCommsTotal(commCostSummary);

            // Assert
            Assert.AreEqual(756.17m, result);
        }

        [TestMethod]
        public void CanGetWalesCommsTotal()
        {
            // Arrange
            var commCostSummary = GetProducerCommsFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetWalesCommsTotal(commCostSummary);

            // Assert
            Assert.AreEqual(190.86m, result);
        }

        [TestMethod]
        public void CanGetScotlandCommsTotal()
        {
            // Arrange
            var commCostSummary = GetProducerCommsFeesByMaterial();

            // Act
            var result = CalcResultSummaryUtil.GetScotlandCommsTotal(commCostSummary);

            // Assert
            Assert.AreEqual(350.43m, result);
        }

        [TestMethod]
        public void CanGetNorthernIrelandCommsTotal()
        {
            // Arrange
            var commCostSummary = GetProducerCommsFeesByMaterial();

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
            // Act
            var result = CalcResultSummaryUtil.GetCommsCostHeaderBadDebtProvisionFor2bTitle(_calcResult);

            // Assert
            Assert.AreEqual(151.80m, result);
        }

        [TestMethod]
        public void GetCommsCostHeaderWithBadDebtFor2bTitle()
        {
            // Act
            var result = CalcResultSummaryUtil.GetCommsCostHeaderWithBadDebtFor2bTitle(_calcResult);

            // Assert
            Assert.AreEqual(2681.80m, result);
        }

        [TestMethod]
        public void GetGetCountryOnePlusFourApportionmentForEngland()
        {
            // Act
            var result = CalcResultSummaryUtil.GetCountryOnePlusFourApportionment(_calcResult, Countries.England);

            // Assert
            Assert.AreEqual(14.53m, result);
        }

        [TestMethod]
        public void GetGetCountryOnePlusFourApportionmentForWales()
        {
            // Act
            var result = CalcResultSummaryUtil.GetCountryOnePlusFourApportionment(_calcResult, Countries.Wales);

            // Assert
            Assert.AreEqual(20, result);
        }

        [TestMethod]
        public void GetGetCountryOnePlusFourApportionmentForScotland()
        {
            // Act
            var result = CalcResultSummaryUtil.GetCountryOnePlusFourApportionment(_calcResult, Countries.Scotland);

            // Assert
            Assert.AreEqual(0.15m, result);
        }

        [TestMethod]
        public void GetGetCountryOnePlusFourApportionmentForNorthernIreland()
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
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void GetParamsOtherFourCountryApportionmentPercentageForWales()
        {
            // Act
            var result = CalcResultSummaryUtil.GetParamsOtherFourCountryApportionmentPercentage(_calcResult, Countries.Wales);

            // Assert
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void GetParamsOtherFourCountryApportionmentPercentageForScotland()
        {
            // Act
            var result = CalcResultSummaryUtil.GetParamsOtherFourCountryApportionmentPercentage(_calcResult, Countries.Scotland);

            // Assert
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void GetParamsOtherFourCountryApportionmentPercentageForNorthernIreland()
        {
            // Act
            var result = CalcResultSummaryUtil.GetParamsOtherFourCountryApportionmentPercentage(_calcResult, Countries.NorthernIreland);

            // Assert
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void CanSetHeaders()
        {

        }

        [TestMethod]
        public void CanGetProducerDisposalFeesHeaders()
        {

        }

        [TestMethod]
        public void CanGetMaterialsBreakdownHeader()
        {

        }

        [TestMethod]
        public void CanGetColumnHeaders()
        {

        }

        private static CalcResultParameterOtherCost GetCalcResultParameterOtherCost()
        {
            return new CalcResultParameterOtherCost
            {
                BadDebtProvision = new KeyValuePair<string, string>("key1", "6%"),
                Details = [
                    new CalcResultParameterOtherCostDetail {
                        Name = "4 LA Data Prep Charge",
                        OrderId = 1,
                        England = "£40.00",
                        EnglandValue = 40,
                        Wales = "£30.00",
                        WalesValue = 30,
                        Scotland = "£20.00",
                        ScotlandValue = 20,
                        NorthernIreland = "£10.00",
                        NorthernIrelandValue = 10,
                        Total = "£100.00",
                        TotalValue = 100
                    }
                ],
                Materiality = [
                    new CalcResultMateriality {
                        Amount = "Amount £s",
                        AmountValue = 0,
                        Percentage = "%",
                        PercentageValue = 0,
                        SevenMateriality = "7 Materiality"
                    }
                ],
                Name = "Parameters - Other",
                SaOperatingCost = [
                    new CalcResultParameterOtherCostDetail {
                        Name = string.Empty,
                        OrderId = 0,
                        England = "England",
                        EnglandValue = 0,
                        Wales = "Wales",
                        WalesValue = 0,
                        Scotland = "Scotland",
                        ScotlandValue = 0,
                        NorthernIreland = "Northern Ireland",
                        NorthernIrelandValue = 0,
                        Total = "Total",
                        TotalValue = 0
                    }
                ],
                SchemeSetupCost = {
                    Name = "5 Scheme set up cost Yearly Cost",
                    OrderId = 1,
                    England = "£40.00",
                    EnglandValue = 40,
                    Wales = "£30.00",
                    WalesValue = 30,
                    Scotland = "£20.00",
                    ScotlandValue = 20,
                    NorthernIreland = "£10.00",
                    NorthernIrelandValue = 10,
                    Total = "£100.00",
                    TotalValue = 100
                }
            };
        }

        private static CalcResultDetail GetCalcResultDetail()
        {
            return new CalcResultDetail() { };
        }

        private CalcResultLaDisposalCostData GetCalcResultLaDisposalCostData()
        {
            return new CalcResultLaDisposalCostData()
            {
                Name = this.Fixture.Create<string>(),
                CalcResultLaDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>()
                {
                    new CalcResultLaDisposalCostDataDetail() {
                        Name = "Material",
                        Material = null,
                        England = "England",
                        Wales = "Wales",
                        Scotland = "Scotland",
                        NorthernIreland = "Northern Ireland",
                        Total = "Total",
                        ProducerReportedHouseholdPackagingWasteTonnage = "Producer Reported Household Packaging Waste Tonnage",
                        LateReportingTonnage = "Late Reporting Tonnage",
                        ProducerReportedHouseholdTonnagePlusLateReportingTonnage = "Producer Reported Household Tonnage + Late Reporting Tonnage",
                        DisposalCostPricePerTonne = "Disposal Cost Price Per Tonne",
                        OrderId = 1
                    },
                    new CalcResultLaDisposalCostDataDetail() {
                        Name = "Aluminium",
                        Material = null,
                        England = "£5,000.00",
                        Wales = "£1,750.00",
                        Scotland = "£2,000.00",
                        NorthernIreland = "£1,250.00",
                        Total = "£10,000.00",
                        ProducerReportedHouseholdPackagingWasteTonnage = "6980.000",
                        LateReportingTonnage = "8000.000",
                        ProducerReportedHouseholdTonnagePlusLateReportingTonnage = "14980.000",
                        DisposalCostPricePerTonne = "£0.6676",
                        OrderId = 2
                    },
                    new CalcResultLaDisposalCostDataDetail() {
                        Name = "Fibre composite",
                        Material = null,
                        England = "£7,500.00",
                        Wales = "£2,100.00",
                        Scotland = "£3,400.00",
                        NorthernIreland = "£1,750.00",
                        Total = "£14,750.00",
                        ProducerReportedHouseholdPackagingWasteTonnage = "11850.000",
                        LateReportingTonnage = "7000.000",
                        ProducerReportedHouseholdTonnagePlusLateReportingTonnage = "18850.000",
                        DisposalCostPricePerTonne = "£0.7825",
                        OrderId = 3
                    },
                    new CalcResultLaDisposalCostDataDetail() {
                        Name = "Glass",
                        Material = null,
                        England = "£45,000.00",
                        Wales = "£0.00",
                        Scotland = "£20,700.00",
                        NorthernIreland = "£4,500.00",
                        Total = "£70,200.00",
                        ProducerReportedHouseholdPackagingWasteTonnage = "4900.000",
                        LateReportingTonnage = "6000.000",
                        ProducerReportedHouseholdTonnagePlusLateReportingTonnage = "10900.000",
                        DisposalCostPricePerTonne = "£6.4404",
                        OrderId = 4
                    },
                    new CalcResultLaDisposalCostDataDetail() {
                        Name = "Paper or card",
                        Material = null,
                        England = "£12,500.00",
                        Wales = "£2,300.00",
                        Scotland = "£4,500.00",
                        NorthernIreland = "£3,400.00",
                        Total = "£22,700.00",
                        ProducerReportedHouseholdPackagingWasteTonnage = "4270.000",
                        LateReportingTonnage = "5000.000",
                        ProducerReportedHouseholdTonnagePlusLateReportingTonnage = "9270.000",
                        DisposalCostPricePerTonne = "£2.4488",
                        OrderId = 5
                    },
                    new CalcResultLaDisposalCostDataDetail() {
                        Name = "Plastic",
                        Material = null,
                        England = "£23,000.00",
                        Wales = "£4,500.00",
                        Scotland = "£6,700.00",
                        NorthernIreland = "£2,100.00",
                        Total = "£36,300.00",
                        ProducerReportedHouseholdPackagingWasteTonnage = "12805.000",
                        LateReportingTonnage = "4000.000",
                        ProducerReportedHouseholdTonnagePlusLateReportingTonnage = "16805.000",
                        DisposalCostPricePerTonne = "£2.1601",
                        OrderId = 6
                    },
                    new CalcResultLaDisposalCostDataDetail() {
                        Name = "Steel",
                        Material = null,
                        England = "£13,400.00",
                        Wales = "£0.00",
                        Scotland = "£7,800.00",
                        NorthernIreland = "£0.00",
                        Total = "£21,200.00",
                        ProducerReportedHouseholdPackagingWasteTonnage = "7700.000",
                        LateReportingTonnage = "3000.000",
                        ProducerReportedHouseholdTonnagePlusLateReportingTonnage = "10700.000",
                        DisposalCostPricePerTonne = "£1.9813",
                        OrderId = 7
                    },
                    new CalcResultLaDisposalCostDataDetail() {
                        Name = "Wood",
                        Material = null,
                        England = "£0.00",
                        Wales = "£12,000.00",
                        Scotland = "£0.00",
                        NorthernIreland = "£5,600.00",
                        Total = "£17,600.00",
                        ProducerReportedHouseholdPackagingWasteTonnage = "6800.000",
                        LateReportingTonnage = "2000.000",
                        ProducerReportedHouseholdTonnagePlusLateReportingTonnage = "8800.000",
                        DisposalCostPricePerTonne = "£2.0000",
                        OrderId = 8
                    },
                    new CalcResultLaDisposalCostDataDetail() {
                        Name = "Other materials",
                        Material = null,
                        England = "£3,400.00",
                        Wales = "£2,100.00",
                        Scotland = "£4,200.00",
                        NorthernIreland = "£700.00",
                        Total = "£10,400.00",
                        ProducerReportedHouseholdPackagingWasteTonnage = "7700.000",
                        LateReportingTonnage = "1000.000",
                        ProducerReportedHouseholdTonnagePlusLateReportingTonnage = "8700.000",
                        DisposalCostPricePerTonne = "£1.1954",
                        OrderId = 9
                    },
                    new CalcResultLaDisposalCostDataDetail() {
                        Name = "Total",
                        Material = null,
                        England = "£109,800.00",
                        Wales = "£24,750.00",
                        Scotland = "£49,300.00",
                        NorthernIreland = "£19,300.00",
                        Total = "£203,150.00",
                        ProducerReportedHouseholdPackagingWasteTonnage = "63005.000",
                        LateReportingTonnage = "36000.000",
                        ProducerReportedHouseholdTonnagePlusLateReportingTonnage = "99005.000",
                        DisposalCostPricePerTonne = null,
                        OrderId = 10
                    }
                }
            };
        }

        private static CalcResultLapcapData GetCalcResultLapcapData()
        {
            return new CalcResultLapcapData()
            {
                CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>() { }
            };
        }

        private CalcResultOnePlusFourApportionment GetCalcResultOnePlusFourApportionment()
        {
            return new CalcResultOnePlusFourApportionment()
            {
                Name = this.Fixture.Create<string>(),
                CalcResultOnePlusFourApportionmentDetails = [
                    new()
                    {
                        EnglandDisposalTotal="80",
                        NorthernIrelandDisposalTotal="70",
                        ScotlandDisposalTotal="30",
                        WalesDisposalTotal="20",
                        AllTotal=0.1M,
                        EnglandTotal=0.10M,
                        NorthernIrelandTotal=0.15M,
                        ScotlandTotal=0.15M,
                        WalesTotal=020M,
                        Name="Test",
                    },
                    new()
                    {
                        EnglandDisposalTotal="80",
                        NorthernIrelandDisposalTotal="70",
                        ScotlandDisposalTotal="30",
                        WalesDisposalTotal="20",
                        AllTotal=0.1M,
                        EnglandTotal=0.10M,
                        NorthernIrelandTotal=0.15M,
                        ScotlandTotal=0.15M,
                        WalesTotal=020M,
                        Name="Test",
                    },
                    new()
                    {
                        EnglandDisposalTotal="80",
                        NorthernIrelandDisposalTotal="70",
                        ScotlandDisposalTotal="30",
                        WalesDisposalTotal="20",
                        AllTotal=0.1M,
                        EnglandTotal=0.10M,
                        NorthernIrelandTotal=0.15M,
                        ScotlandTotal=0.15M,
                        WalesTotal=020M,
                        Name="Test",
                    },
                    new()
                    {
                        EnglandDisposalTotal="80",
                        NorthernIrelandDisposalTotal="70",
                        ScotlandDisposalTotal="30",
                        WalesDisposalTotal="20",
                        AllTotal=0.1M,
                        EnglandTotal=14.53M,
                        NorthernIrelandTotal=0.15M,
                        ScotlandTotal=0.15M,
                        WalesTotal=020M,
                        Name="Test",
                    },
                    new()
                    {
                        EnglandDisposalTotal="80",
                        NorthernIrelandDisposalTotal="70",
                        ScotlandDisposalTotal="30",
                        WalesDisposalTotal="20",
                        AllTotal=0.1M,
                        EnglandTotal=14.53M,
                        NorthernIrelandTotal=0.15M,
                        ScotlandTotal=0.15M,
                        WalesTotal=020M,
                        Name=OnePlus4ApportionmentColumnHeaders.OnePluseFourApportionment,
                    }
                ]
            };
        }

        private CalcResultParameterCommunicationCost GetCalcResultParameterCommunicationCost()
        {
            return this.Fixture.Create<CalcResultParameterCommunicationCost>();
        }

        private static CalcResultCommsCost GetCalcResultCommsCostReportDetail()
        {
            return new CalcResultCommsCost()
            {
                CalcResultCommsCostCommsCostByMaterial = [
                    new ()
                    {
                        CommsCostByMaterialPricePerTonne="0.42",
                        Name ="Material1",

                    },
                    new ()
                    {
                        CommsCostByMaterialPricePerTonne="0.3",
                        Name ="Material2",

                    }
                ],
                CommsCostByCountry = [
                    new()
                    {
                        Total= "Total"
                    },
                    new()
                    {
                        TotalValue= 2530
                    }
                ]
            };
        }

        private static CalcResultSummary GetCalcResultSummary()
        {
            return new CalcResultSummary
            {
                BadDebtProvisionFor1 = 6021.3677166M,
                BadDebtProvisionFor2A = 2098.887360M,
                BadDebtProvisionTitleSection3 = 3900.000000M,
                ProducerDisposalFees = GetProducerDisposalFees()
            };
        }

        private static List<CalcResultSummaryProducerDisposalFees> GetProducerDisposalFees()
        {
            return new List<CalcResultSummaryProducerDisposalFees>()
            {
                new CalcResultSummaryProducerDisposalFees()
                {
                    ProducerId = "1",
                    SubsidiaryId = "",
                    ProducerName = "Allied Packaging",
                    Level = "1",
                    isTotalRow = false,
                    TotalProducerDisposalFee = 4423.39438m,
                    BadDebtProvision = 265.4036628m,
                    TotalProducerDisposalFeeWithBadDebtProvision = 4688.7980428m,
                    EnglandTotal = 2534.2359097426884m,
                    WalesTotal = 571.2417008090152m,
                    ScotlandTotal = 1137.8673076088023m,
                    NorthernIrelandTotal = 445.4531246394942m,
                    TotalProducerCommsFee = 1290.778m,
                    BadDebtProvisionComms = 77.44668m,
                    TotalProducerCommsFeeWithBadDebtProvision = 1368.22468m,
                    EnglandTotalComms = 718.2251815154783m,
                    WalesTotalComms = 181.2690740598454m,
                    ScotlandTotalComms = 332.8499847265775m,
                    NorthernIrelandTotalComms = 135.88043969809883m,
                    TotalProducerFeeforLADisposalCostswoBadDebtprovision = 4423.39438m,
                    BadDebtProvisionFor1 = 265.4036628m,
                    TotalProducerFeeforLADisposalCostswithBadDebtprovision = 4688.7980428m,
                    EnglandTotalWithBadDebtProvision = 2534.2359097426884m,
                    WalesTotalWithBadDebtProvision = 571.2417008090152m,
                    ScotlandTotalWithBadDebtProvision = 1137.8673076088023m,
                    NorthernIrelandTotalWithBadDebtProvision = 445.4531246394942m,
                    TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision = 1290.778m,
                    BadDebtProvisionFor2A = 77.44668m,
                    TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision = 1368.22468m,
                    EnglandTotalWithBadDebtProvision2A = 718.2251815154783m,
                    WalesTotalWithBadDebtProvision2A = 181.2690740598454m,
                    ScotlandTotalWithBadDebtProvision2A = 332.8499847265775m,
                    NorthernIrelandTotalWithBadDebtProvision2A = 135.88043969809883m,
                    TwoCTotalProducerFeeForCommsCostsWithoutBadDebt = 1339.100071422903m,
                    TwoCBadDebtProvision = 80.34600428537418m,
                    TwoCTotalProducerFeeForCommsCostsWithBadDebt = 1419.446075708277m,
                    TwoCEnglandTotalWithBadDebt = 607.4748035870169m,
                    TwoCWalesTotalWithBadDebt = 300.7301007856519m,
                    TwoCScotlandTotalWithBadDebt = 360.87612094278234m,
                    TwoCNorthernIrelandTotalWithBadDebt = 150.36505039282596m,
                    PercentageofProducerReportedHHTonnagevsAllProducers = 5.6741528450123m,
                    ProducerTotalOnePlus2A2B2CWithBadDeptProvision = 10491.167766844124m,
                    ProducerOverallPercentageOfCostsForOnePlus2A2B2C = 4.7341913352015945m,
                    Total3SAOperatingCostwoBadDebtprovision = 3077.2243678810364m,
                    BadDebtProvisionFor3 = 184.6334620728622m,
                    Total3SAOperatingCostswithBadDebtprovision = 3261.8578299538985m,
                    EnglandTotalWithBadDebtProvision3 = 1712.2541832180282m,
                    WalesTotalWithBadDebtProvision3 = 432.1468228710047m,
                    ScotlandTotalWithBadDebtProvision3 = 793.5168432560496m,
                    NorthernIrelandTotalWithBadDebtProvision3 = 323.93998060881614m,
                    LaDataPrepCostsTotalWithoutBadDebtProvisionSection4 = 1727.9798373485821m,
                    LaDataPrepCostsBadDebtProvisionSection4 = 103.67879024091492m,
                    LaDataPrepCostsTotalWithBadDebtProvisionSection4 = 1831.658627589497m,
                    LaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4 = 802.9188504501905m,
                    LaDataPrepCostsWalesTotalWithBadDebtProvisionSection4 = 351.2769970719583m,
                    LaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4 = 451.6418533782321m,
                    LaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4 = 225.82092668911605m,
                    TotalProducerFeeWithoutBadDebtProvisionSection5 = 2970.7050628390007m,
                    BadDebtProvisionSection5 = 178.24230377034004m,
                    TotalProducerFeeWithBadDebtProvisionSection5 = 3148.947366609341m,
                    EnglandTotalWithBadDebtProvisionSection5 = 1652.983846106635m,
                    WalesTotalWithBadDebtProvisionSection5 = 417.1878943870084m,
                    ScotlandTotalWithBadDebtProvisionSection5 = 766.0489525279556m,
                    NorthernIrelandTotalWithBadDebtProvisionSection5 = 312.72667358774174m,
                    TotalProducerFeeWithoutBadDebtFor2bComms = 2844.0556305055156m,
                    BadDebtProvisionFor2bComms = 170.64333783033092m,
                    TotalProducerFeeWithBadDebtFor2bComms = 3014.6989683358465m,
                    EnglandTotalWithBadDebtFor2bComms = 1582.5125400804336m,
                    WalesTotalWithBadDebtFor2bComms = 399.4020123649648m,
                    ScotlandTotalWithBadDebtFor2bComms = 733.3901516568284m,
                    NorthernIrelandTotalWithBadDebtFor2bComms = 299.39426423361965m,
                    TotalProducerBillWithoutBadDebtProvision = 9897.32808192842m,
                    BadDebtProvisionForTotalProducerBill = 593.8396849157051m,
                    TotalProducerBillWithBadDebtProvision = 10491.167766844124m,
                    EnglandTotalWithBadDebtProvisionTotalBill = 5442.448434925617m,
                    WalesTotalWithBadDebtProvisionTotalBill = 1452.6428880194774m,
                    ScotlandTotalWithBadDebtProvisionTotalBill = 2564.98356493499m,
                    NorthernIrelandTotalWithBadDebtProvisionTotalBill = 1031.0928789640386m,
                    ProducerDisposalFeesByMaterial = GetProducerDisposalFeesByMaterial(),
                    ProducerCommsFeesByMaterial = GetProducerCommsFeesByMaterial()
                }
            };
        }

        private static Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> GetProducerDisposalFeesByMaterial()
        {
            return new Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>
            {
                {
                    new MaterialDetail
                    {
                        Id = 1,
                        Code = "AL",
                        Name = "Aluminium",
                        Description = "Aluminium"
                    },
                    new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 1000,
                        ManagedConsumerWasteTonnage = 90,
                        NetReportedTonnage = 910,
                        PricePerTonne = 0.6676m,
                        ProducerDisposalFee = 607.52m,
                        BadDebtProvision = 36.45m,
                        ProducerDisposalFeeWithBadDebtProvision = 643.97m,
                        EnglandWithBadDebtProvision = 348.06m,
                        WalesWithBadDebtProvision = 78.46m,
                        ScotlandWithBadDebtProvision = 156.28m,
                        NorthernIrelandWithBadDebtProvision = 61.18m
                    }
                },
                {
                    new MaterialDetail
                    {
                        Id = 2,
                        Code = "FC",
                        Name = "Fibre composite",
                        Description = "Fibre composite"
                    },
                    new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 2000,
                        ManagedConsumerWasteTonnage = 140,
                        NetReportedTonnage = 1860,
                        PricePerTonne = 0.7825m,
                        ProducerDisposalFee = 1455.45m,
                        BadDebtProvision = 87.33m,
                        ProducerDisposalFeeWithBadDebtProvision = 1542.78m,
                        EnglandWithBadDebtProvision = 833.85m,
                        WalesWithBadDebtProvision = 187.96m,
                        ScotlandWithBadDebtProvision = 374.40m,
                        NorthernIrelandWithBadDebtProvision = 146.57m
                    }
                },
                {
                    new MaterialDetail
                    {
                        Id = 3,
                        Code = "GL",
                        Name = "Glass",
                        Description = "Glass"
                    },
                    new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 500,
                        ManagedConsumerWasteTonnage = 150,
                        NetReportedTonnage = 350,
                        PricePerTonne = 6.4404m,
                        ProducerDisposalFee = 2254.14m,
                        BadDebtProvision = 135.25m,
                        ProducerDisposalFeeWithBadDebtProvision = 2389.39m,
                        EnglandWithBadDebtProvision = 1291.43m,
                        WalesWithBadDebtProvision = 291.10m,
                        ScotlandWithBadDebtProvision = 579.85m,
                        NorthernIrelandWithBadDebtProvision = 227
                    }
                },
                {
                    new MaterialDetail
                    {
                        Id = 4,
                        Code = "PC",
                        Name = "Paper or card",
                        Description = "Paper or card"
                    },
                    new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 20,
                        ManagedConsumerWasteTonnage = 2.200m,
                        NetReportedTonnage = 17.800m,
                        PricePerTonne = 2.4488m,
                        ProducerDisposalFee = 43.59m,
                        BadDebtProvision = 2.62m,
                        ProducerDisposalFeeWithBadDebtProvision = 46.20m,
                        EnglandWithBadDebtProvision = 24.97m,
                        WalesWithBadDebtProvision = 5.63m,
                        ScotlandWithBadDebtProvision = 11.21m,
                        NorthernIrelandWithBadDebtProvision = 4.39m
                    }
                },
                {
                    new MaterialDetail
                    {
                        Id = 5,
                        Code = "PL",
                        Name = "Plastic",
                        Description = "Plastic"
                    },
                    new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 5.000m,
                        ManagedConsumerWasteTonnage = 0.600m,
                        NetReportedTonnage = 4.400m,
                        PricePerTonne = 2.1601m,
                        ProducerDisposalFee = 9.50m,
                        BadDebtProvision = 0.57m,
                        ProducerDisposalFeeWithBadDebtProvision = 10.07m,
                        EnglandWithBadDebtProvision = 5.45m,
                        WalesWithBadDebtProvision = 1.23m,
                        ScotlandWithBadDebtProvision = 2.44m,
                        NorthernIrelandWithBadDebtProvision = 0.96m
                    }
                },
                {
                    new MaterialDetail
                    {
                        Id = 6,
                        Code = "ST",
                        Name = "Steel",
                        Description = "Steel"
                    },
                    new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 0.000m,
                        ManagedConsumerWasteTonnage = 0.000m,
                        NetReportedTonnage = 0.000m,
                        PricePerTonne = 1.9813m,
                        ProducerDisposalFee = 0.00m,
                        BadDebtProvision = 0.00m,
                        ProducerDisposalFeeWithBadDebtProvision = 0.00m,
                        EnglandWithBadDebtProvision = 0.00m,
                        WalesWithBadDebtProvision = 0.00m,
                        ScotlandWithBadDebtProvision = 0.00m,
                        NorthernIrelandWithBadDebtProvision = 0.00m
                    }
                },
                {
                    new MaterialDetail
                    {
                        Id = 7,
                        Code = "WD",
                        Name = "Wood",
                        Description = "Wood"
                    },
                    new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 500.000m,
                        ManagedConsumerWasteTonnage = 95.000m,
                        NetReportedTonnage = 405.000m,
                        PricePerTonne = 2.0000m,
                        ProducerDisposalFee = 810.00m,
                        BadDebtProvision = 48.60m,
                        ProducerDisposalFeeWithBadDebtProvision = 858.60m,
                        EnglandWithBadDebtProvision = 464.06m,
                        WalesWithBadDebtProvision = 104.60m,
                        ScotlandWithBadDebtProvision = 208.36m,
                        NorthernIrelandWithBadDebtProvision = 81.57m
                    }
                },
                {
                    new MaterialDetail
                    {
                        Id = 8,
                        Code = "OT",
                        Name = "Other materials",
                        Description = "Other materials"
                    },
                    new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 50.000m,
                        ManagedConsumerWasteTonnage = 5.500m,
                        NetReportedTonnage = 44.500m,
                        PricePerTonne = 1.1954m,
                        ProducerDisposalFee = 53.20m,
                        BadDebtProvision = 3.19m,
                        ProducerDisposalFeeWithBadDebtProvision = 56.39m,
                        EnglandWithBadDebtProvision = 30.48m,
                        WalesWithBadDebtProvision = 6.87m,
                        ScotlandWithBadDebtProvision = 13.68m,
                        NorthernIrelandWithBadDebtProvision = 5.36m
                    }
                }
            };
        }

        private static Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> GetProducerCommsFeesByMaterial()
        {
            return new Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>
            {
                {
                    new MaterialDetail
                    {
                        Id = 1,
                        Code = "AL",
                        Name = "Aluminium",
                        Description = "Aluminium"
                    },
                    new CalcResultSummaryProducerCommsFeesCostByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 1000,
                        PriceperTonne = 0.1916m,
                        ProducerTotalCostWithoutBadDebtProvision = 191.60m,
                        BadDebtProvision = 11.50m,
                        ProducerTotalCostwithBadDebtProvision = 203.10m,
                        EnglandWithBadDebtProvision = 106.61m,
                        WalesWithBadDebtProvision = 26.91m,
                        ScotlandWithBadDebtProvision = 49.41m,
                        NorthernIrelandWithBadDebtProvision = 20.17m
                    }
                },
                {
                    new MaterialDetail
                    {
                        Id = 2,
                        Code = "FC",
                        Name = "Fibre composite",
                        Description = "Fibre composite"
                    },
                    new CalcResultSummaryProducerCommsFeesCostByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 2000.000m,
                        PriceperTonne = 0.4032m,
                        ProducerTotalCostWithoutBadDebtProvision = 806.40m,
                        BadDebtProvision = 48.38m,
                        ProducerTotalCostwithBadDebtProvision = 854.78m,
                        EnglandWithBadDebtProvision = 448.70m,
                        WalesWithBadDebtProvision = 113.25m,
                        ScotlandWithBadDebtProvision = 207.94m,
                        NorthernIrelandWithBadDebtProvision = 84.89m
                    }
                },
                {
                    new MaterialDetail
                    {
                        Id = 3,
                        Code = "GL",
                        Name = "Glass",
                        Description = "Glass"
                    },
                    new CalcResultSummaryProducerCommsFeesCostByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 500.000m,
                        PriceperTonne = 0.4404m,
                        ProducerTotalCostWithoutBadDebtProvision = 220.20m,
                        BadDebtProvision = 13.21m,
                        ProducerTotalCostwithBadDebtProvision = 233.41m,
                        EnglandWithBadDebtProvision = 122.53m,
                        WalesWithBadDebtProvision = 30.92m,
                        ScotlandWithBadDebtProvision = 56.78m,
                        NorthernIrelandWithBadDebtProvision = 23.18m
                    }
                },
                {
                    new MaterialDetail
                    {
                        Id = 4,
                        Code = "PC",
                        Name = "Paper or card",
                        Description = "Paper or card"
                    },
                    new CalcResultSummaryProducerCommsFeesCostByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 20.000m,
                        PriceperTonne = 1.1100m,
                        ProducerTotalCostWithoutBadDebtProvision = 22.20m,
                        BadDebtProvision = 1.33m,
                        ProducerTotalCostwithBadDebtProvision = 23.53m,
                        EnglandWithBadDebtProvision = 12.35m,
                        WalesWithBadDebtProvision = 3.12m,
                        ScotlandWithBadDebtProvision = 5.72m,
                        NorthernIrelandWithBadDebtProvision = 2.34m
                    }
                },
                {
                    new MaterialDetail
                    {
                        Id = 5,
                        Code = "PL",
                        Name = "Plastic",
                        Description = "Plastic"
                    },
                    new CalcResultSummaryProducerCommsFeesCostByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 5.000m,
                        PriceperTonne = 0.5356m,
                        ProducerTotalCostWithoutBadDebtProvision = 2.68m,
                        BadDebtProvision = 0.16m,
                        ProducerTotalCostwithBadDebtProvision = 2.84m,
                        EnglandWithBadDebtProvision = 1.49m,
                        WalesWithBadDebtProvision = 0.38m,
                        ScotlandWithBadDebtProvision = 0.69m,
                        NorthernIrelandWithBadDebtProvision = 0.28m
                    }
                },
                {
                    new MaterialDetail
                    {
                        Id = 6,
                        Code = "ST",
                        Name = "Steel",
                        Description = "Steel"
                    },
                    new CalcResultSummaryProducerCommsFeesCostByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 0.000m,
                        PriceperTonne = 0.8879m,
                        ProducerTotalCostWithoutBadDebtProvision = 0.00m,
                        BadDebtProvision = 0.00m,
                        ProducerTotalCostwithBadDebtProvision = 0.00m,
                        EnglandWithBadDebtProvision = 0.00m,
                        WalesWithBadDebtProvision = 0.00m,
                        ScotlandWithBadDebtProvision = 0.00m,
                        NorthernIrelandWithBadDebtProvision = 0.00m
                    }
                },
                {
                    new MaterialDetail
                    {
                        Id = 7,
                        Code = "WD",
                        Name = "Wood",
                        Description = "Wood"
                    },
                    new CalcResultSummaryProducerCommsFeesCostByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 500.000m,
                        PriceperTonne = 0.1364m,
                        ProducerTotalCostWithoutBadDebtProvision = 68.20m,
                        BadDebtProvision = 4.09m,
                        ProducerTotalCostwithBadDebtProvision = 72.29m,
                        EnglandWithBadDebtProvision = 37.95m,
                        WalesWithBadDebtProvision = 9.58m,
                        ScotlandWithBadDebtProvision = 17.59m,
                        NorthernIrelandWithBadDebtProvision = 7.18m
                    }
                },
                {
                    new MaterialDetail
                    {
                        Id = 8,
                        Code = "OT",
                        Name = "Other materials",
                        Description = "Other materials"
                    },
                    new CalcResultSummaryProducerCommsFeesCostByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 50.000m,
                        PriceperTonne = 0.9540m,
                        ProducerTotalCostWithoutBadDebtProvision = 47.70m,
                        BadDebtProvision = 2.86m,
                        ProducerTotalCostwithBadDebtProvision = 50.56m,
                        EnglandWithBadDebtProvision = 26.54m,
                        WalesWithBadDebtProvision = 6.70m,
                        ScotlandWithBadDebtProvision = 12.30m,
                        NorthernIrelandWithBadDebtProvision = 5.02m
                    }
                }
            };
        }

        private CalcResultLateReportingTonnage GetCalcResultLateReportingTonnage()
        {
            return this.Fixture.Create<CalcResultLateReportingTonnage>();
        }

        private static List<MaterialDetail> GetMaterials()
        {
            return new List<MaterialDetail>
            {
                new MaterialDetail
                {
                    Id = 1,
                    Code = "AL",
                    Name = "Aluminium",
                    Description = "Aluminium"
                },
                new MaterialDetail
                {
                    Id = 2,
                    Code = "FC",
                    Name = "Fibre composite",
                    Description = "Fibre composite"
                },
                new MaterialDetail
                {
                    Id = 3,
                    Code = "GL",
                    Name = "Glass",
                    Description = "Glass"
                },
                new MaterialDetail
                {
                    Id = 4,
                    Code = "PC",
                    Name = "Paper or card",
                    Description = "Paper or card"
                },
                new MaterialDetail
                {
                    Id = 5,
                    Code = "PL",
                    Name = "Plastic",
                    Description = "Plastic"
                },
                new MaterialDetail
                {
                    Id = 6,
                    Code = "ST",
                    Name = "Steel",
                    Description = "Steel"
                },
                new MaterialDetail
                {
                    Id = 7,
                    Code = "WD",
                    Name = "Wood",
                    Description = "Wood"
                },
                new MaterialDetail
                {
                    Id = 8,
                    Code = "OT",
                    Name = "Other materials",
                    Description = "Other materials"
                }
            };
        }

        private static List<ProducerDetail> GetProducers()
        {
            return new List<ProducerDetail>
            {
                new ProducerDetail
                {
                    Id = 1,
                    ProducerName = "Allied Packaging",
                    CalculatorRunId = 1,
                    CalculatorRun = new CalculatorRun { Financial_Year = "2024-25", Name = "Test Run 1" }
                },
                new ProducerDetail
                {
                    Id = 2,
                    ProducerName = "Beeline Materials",
                    CalculatorRunId = 1,
                    CalculatorRun = new CalculatorRun { Financial_Year = "2024-25", Name = "Test Run 1" }
                },
                new ProducerDetail
                {
                    Id = 3,
                    ProducerName = "Cloud Boxes",
                    CalculatorRunId = 1,
                    CalculatorRun = new CalculatorRun { Financial_Year = "2024-25", Name = "Test Run 1" }
                }
            };
        }

        // dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial { Material = material, PackagingTonnage = 1000.00m, PackagingType = "HH", MaterialId = 1, ProducerDetail = producer });
        //dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial { Material = plasticMaterial, PackagingTonnage = 2000.00m, PackagingType = "HH", MaterialId = 2, ProducerDetail = producer });
        //dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial { Material = plasticMaterial, PackagingTonnage = 2000.00m, PackagingType = "CW", MaterialId = 2, ProducerDetail = producer });
    }
}