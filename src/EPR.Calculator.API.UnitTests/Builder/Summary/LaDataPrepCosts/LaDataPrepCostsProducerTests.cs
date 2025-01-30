namespace EPR.Calculator.API.UnitTests.Builder.Summary.LaDataPrepCosts
{
    using AutoFixture;
    using EPR.Calculator.API.Builder.Summary.LaDataPrepCosts;
    using Data;
    using Data.DataModels;
    using Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;

    [TestClass]
    public class LaDataPrepCostsProducerTests
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly CalcResult _calcResult;

        private Fixture Fixture { get; init; } = new Fixture();

        public LaDataPrepCostsProducerTests()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _dbContext = new ApplicationDBContext(dbContextOptions);
            _dbContext.Database.EnsureCreated();

            CreateMaterials();
            CreateProducerDetail();

            _calcResult = new CalcResult
            {
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost
                {
                    BadDebtProvision = new KeyValuePair<string, string>("key1", "6%"),
                    BadDebtValue = 6m,
                    Details = [
                        new CalcResultParameterOtherCostDetail
                        {
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
                        },
                        new CalcResultParameterOtherCostDetail
                        {
                            Name = "4 Country Apportionment %s",
                            OrderId = 1,
                            England = "40.00%",
                            EnglandValue = 40,
                            Wales = "30.00%",
                            WalesValue = 30,
                            Scotland = "20.00%",
                            ScotlandValue = 20,
                            NorthernIreland = "10.00%",
                            NorthernIrelandValue = 10,
                            Total = "100.00%",
                            TotalValue = 100
                        }
                    ],
                    Materiality = [
                        new CalcResultMateriality
                        {
                            Amount = "Amount £s",
                            AmountValue = 0,
                            Percentage = "%",
                            PercentageValue = 0,
                            SevenMateriality = "7 Materiality"
                        }
                    ],
                    Name = "Parameters - Other",
                    SaOperatingCost = [
                        new CalcResultParameterOtherCostDetail
                        {
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
                },
                CalcResultDetail = new CalcResultDetail() { },
                CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData()
                {
                    Name = Fixture.Create<string>(),
                    CalcResultLaDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>()
                    {
                        new CalcResultLaDisposalCostDataDetail()
                        {
                            DisposalCostPricePerTonne="20",
                            England="EnglandTest",
                            Wales="WalesTest",
                            Name="ScotlandTest",
                            Scotland="ScotlandTest",
                            Material = "Material1",
                            NorthernIreland = "NorthernIrelandTest",
                            Total = "TotalTest",
                            ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
                            ProducerReportedHouseholdTonnagePlusLateReportingTonnage = Fixture.Create<string>(),
                        },
                        new CalcResultLaDisposalCostDataDetail()
                        {
                            DisposalCostPricePerTonne="20",
                            England="EnglandTest",
                            Wales="WalesTest",
                            Name="Material1",
                            Scotland="ScotlandTest",
                            NorthernIreland = "NorthernIrelandTest",
                            Total = "TotalTest",
                            ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
                            ProducerReportedHouseholdTonnagePlusLateReportingTonnage = Fixture.Create<string>(),
                        },
                        new CalcResultLaDisposalCostDataDetail()
                        {
                            DisposalCostPricePerTonne="10",
                            England="EnglandTest",
                            Wales="WalesTest",
                            Name="Material2",
                            Scotland="ScotlandTest",
                            NorthernIreland = "NorthernIrelandTest",
                            Total = "TotalTest",
                            ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
                            ProducerReportedHouseholdTonnagePlusLateReportingTonnage = Fixture.Create<string>(),
                        }
                    }
                },
                CalcResultLapcapData = new CalcResultLapcapData()
                {
                    CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>()
                    {
                    }
                },
                CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment()
                {
                    Name = Fixture.Create<string>(),
                    CalcResultOnePlusFourApportionmentDetails =
                    [
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
                            Name="1 + 4 Apportionment %s",
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
                            Name="Test",
                        }
                    ]
                },
                CalcResultParameterCommunicationCost = Fixture.Create<CalcResultParameterCommunicationCost>(),
                CalcResultSummary = new CalcResultSummary
                {
                    ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>()
                    {
                        new()
                        {
                            ProducerCommsFeesByMaterial =
                                new Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>() { },
                            ProducerDisposalFeesByMaterial =
                                new Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>() { },
                            ProducerId = "1",
                            ProducerName = "Test",
                            TotalProducerDisposalFeeWithBadDebtProvision = 100,
                            TotalProducerCommsFeeWithBadDebtProvision = 100,
                            SubsidiaryId = "1",
                            TotalProducerFeeforLADisposalCostswithBadDebtprovision = 10,
                            TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision = 10,
                            TotalProducerFeeWithBadDebtFor2bComms = 10,
                            TwoCTotalProducerFeeForCommsCostsWithBadDebt = 10,
                            ProducerOverallPercentageOfCostsForOnePlus2A2B2C = 100,
                            LaDataPrepCostsTotalWithoutBadDebtProvisionSection4 = 100,
                            LaDataPrepCostsBadDebtProvisionSection4 = 20,
                            LaDataPrepCostsTotalWithBadDebtProvisionSection4 = 120,
                            LaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4 = 20,
                            LaDataPrepCostsWalesTotalWithBadDebtProvisionSection4 = 20,
                            LaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4 = 20,
                            LaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4 = 20,
                        }
                    },
                    TotalFeeforLADisposalCostswithBadDebtprovision1 = 100,
                    TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A = 100,
                    CommsCostHeaderWithBadDebtFor2bTitle = 100,
                    TwoCCommsCostsByCountryWithBadDebtProvision = 100,
                },
                CalcResultCommsCostReportDetail = new CalcResultCommsCost()
                {
                    CalcResultCommsCostCommsCostByMaterial =
                    [
                        new ()
                        {
                            CommsCostByMaterialPricePerTonne="0.42",
                            Name ="Aluminium",

                        },
                        new ()
                        {
                            CommsCostByMaterialPricePerTonne="0.3",
                            Name ="Glass",

                        }
                    ]
                },
                CalcResultLateReportingTonnageData = Fixture.Create<CalcResultLateReportingTonnage>(),
            };
        }

        [TestCleanup]
        public void TearDown()
        {
            _dbContext?.Database.EnsureDeleted();
        }

        [TestMethod]
        public void CanCallGetHeaders()
        {
            // Act
            var result = LaDataPrepCostsProducer.GetHeaders().ToList();

            var expectedResult = new List<CalcResultSummaryHeader>();
            expectedResult.AddRange([
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.TotalProducerFeeWithoutBadDebtProvision , ColumnIndex = 234 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.BadDebtProvision, ColumnIndex = 235 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.TotalProducerFeeWithBadDebtProvision, ColumnIndex = 236 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.EnglandTotalWithBadDebtProvision, ColumnIndex = 237 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.WalesTotalWithBadDebtProvision, ColumnIndex = 238 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.ScotlandTotalWithBadDebtProvision, ColumnIndex = 239 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.NorthernIrelandTotalWithBadDebtProvision, ColumnIndex = 240 }
            ]);

            // Assert
            Assert.AreEqual(expectedResult[0].Name, result[0].Name);
            Assert.AreEqual(expectedResult[0].ColumnIndex, result[0].ColumnIndex);
            Assert.AreEqual(expectedResult[1].Name, result[1].Name);
            Assert.AreEqual(expectedResult[1].ColumnIndex, result[1].ColumnIndex);
            Assert.AreEqual(expectedResult[2].Name, result[2].Name);
            Assert.AreEqual(expectedResult[2].ColumnIndex, result[2].ColumnIndex);
            Assert.AreEqual(expectedResult[3].Name, result[3].Name);
            Assert.AreEqual(expectedResult[3].ColumnIndex, result[3].ColumnIndex);
            Assert.AreEqual(expectedResult[4].Name, result[4].Name);
            Assert.AreEqual(expectedResult[4].ColumnIndex, result[4].ColumnIndex);
            Assert.AreEqual(expectedResult[5].Name, result[5].Name);
            Assert.AreEqual(expectedResult[5].ColumnIndex, result[5].ColumnIndex);
            Assert.AreEqual(expectedResult[6].Name, result[6].Name);
            Assert.AreEqual(expectedResult[6].ColumnIndex, result[6].ColumnIndex);
        }

        [TestMethod]
        public void CanCallGetSummaryHeaders()
        {
            // Act
            var result = LaDataPrepCostsProducer.GetSummaryHeaders().ToList();

            var expectedResult = new List<CalcResultSummaryHeader>();
            expectedResult.AddRange([
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.LaDataPrepCostsWithoutBadDebtProvisionTitle, ColumnIndex = 234 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.BadDebtProvisionTitle, ColumnIndex = 235 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.LaDataPrepCostsWithBadDebtProvisionTitle, ColumnIndex = 236 }
            ]);

            // Assert
            Assert.AreEqual(expectedResult[0].Name, result[0].Name);
            Assert.AreEqual(expectedResult[0].ColumnIndex, result[0].ColumnIndex);
            Assert.AreEqual(expectedResult[1].Name, result[1].Name);
            Assert.AreEqual(expectedResult[1].ColumnIndex, result[1].ColumnIndex);
            Assert.AreEqual(expectedResult[2].Name, result[2].Name);
            Assert.AreEqual(expectedResult[2].ColumnIndex, result[2].ColumnIndex);
        }

        [TestMethod]
        public void CanCallSetValues()
        { 
            // Act
            LaDataPrepCostsProducer.SetValues(_calcResult, _calcResult.CalcResultSummary);

            // Assert
            Assert.AreEqual(100, _calcResult.CalcResultSummary.LaDataPrepCostsTitleSection4);
            Assert.AreEqual(6, _calcResult.CalcResultSummary.LaDataPrepCostsBadDebtProvisionTitleSection4);
            Assert.AreEqual(106, _calcResult.CalcResultSummary.LaDataPrepCostsWithBadDebtProvisionTitleSection4);
            Assert.AreEqual(100, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LaDataPrepCostsTotalWithoutBadDebtProvisionSection4);
            Assert.AreEqual(6, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LaDataPrepCostsBadDebtProvisionSection4);
            Assert.AreEqual(106, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LaDataPrepCostsTotalWithBadDebtProvisionSection4);
            Assert.AreEqual(42.40m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4);
            Assert.AreEqual(31.80m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LaDataPrepCostsWalesTotalWithBadDebtProvisionSection4);
            Assert.AreEqual(21.20m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4);
            Assert.AreEqual(10.60m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4);
        }

        private void CreateMaterials()
        {
            var materialDictionary = new Dictionary<string, string>();
            materialDictionary.Add("AL", "Aluminium");
            materialDictionary.Add("FC", "Fibre composite");
            materialDictionary.Add("GL", "Glass");
            materialDictionary.Add("PC", "Paper or card");
            materialDictionary.Add("PL", "Plastic");
            materialDictionary.Add("ST", "Steel");
            materialDictionary.Add("WD", "Wood");
            materialDictionary.Add("OT", "Other materials");

            foreach (var materialKv in materialDictionary)
            {
                _dbContext.Material.Add(new Material
                {
                    Name = materialKv.Value,
                    Code = materialKv.Key,
                    Description = "Some"
                });
            }

            _dbContext.SaveChanges();
        }

        private void CreateProducerDetail()
        {
            var producerNames = new string[]
            {
                "Allied Packaging",
                "Beeline Materials",
                "Cloud Boxes",
                "Decking and Shed",
                "Electric Things",
                "French Flooring",
                "Good Fruit Co",
                "Happy Shopper",
                "Icicle Foods",
                "Jumbo Box Store"
            };

            var producerId = 1;
            foreach (var producerName in producerNames)
            {
                _dbContext.ProducerDetail.Add(new ProducerDetail
                {
                    ProducerId = producerId++,
                    SubsidiaryId = $"{producerId}-Sub",
                    ProducerName = producerName,
                    CalculatorRunId = 1,
                });
            }

            _dbContext.SaveChanges();

            for (int producerDetailId = 1; producerDetailId <= 10; producerDetailId++)
            {
                for (int materialId = 1; materialId < 9; materialId++)
                {
                    _dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                    {
                        MaterialId = materialId,
                        ProducerDetailId = producerDetailId,
                        PackagingType = "HH",
                        PackagingTonnage = (materialId * 100)
                    });
                    _dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                    {
                        MaterialId = materialId,
                        ProducerDetailId = producerDetailId,
                        PackagingType = "CW",
                        PackagingTonnage = (materialId * 50)
                    });
                }
            }
            _dbContext.SaveChanges();
        }
    }
}
