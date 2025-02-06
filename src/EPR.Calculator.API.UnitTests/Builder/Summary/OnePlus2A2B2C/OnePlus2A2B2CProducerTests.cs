namespace EPR.Calculator.API.UnitTests.Builder.Summary.OnePlus2A2B2C
{
    using AutoFixture;
    using EPR.Calculator.API.Builder.Summary.OnePlus2A2B2C;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.API.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OnePlus2A2B2CProducerTests
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly CalcResult _calcResult;

        public OnePlus2A2B2CProducerTests()
        {
            this.Fixture = new Fixture();
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
                CalcResultLateReportingTonnageData = Fixture.Create<CalcResultLateReportingTonnage>(),
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost
                {
                    BadDebtProvision = new KeyValuePair<string, string>("key1", "6%"),
                    BadDebtValue = 6m,
                    Details =
                    [
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
                        }
                    ],
                    Materiality =
                    [
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
                    SaOperatingCost =
                    [
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
                    SchemeSetupCost =
                    {
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
                            DisposalCostPricePerTonne = "20",
                            England = "EnglandTest",
                            Wales = "WalesTest",
                            Name = "ScotlandTest",
                            Scotland = "ScotlandTest",
                            NorthernIreland = "NorthernIrelandTest",
                            Material = "Material1",
                            ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
                            ReportedPublicBinTonnage = Fixture.Create<string>(),
                            Total = Fixture.Create<string>(),
                        },
                        new CalcResultLaDisposalCostDataDetail()
                        {
                            DisposalCostPricePerTonne = "20",
                            England = "EnglandTest",
                            Wales = "WalesTest",
                            Name = "Material1",
                            Scotland = "ScotlandTest",
                            NorthernIreland = "NorthernIrelandTest",
                            ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
                            ReportedPublicBinTonnage = Fixture.Create<string>(),
                            Total = Fixture.Create<string>(),
                        },
                        new CalcResultLaDisposalCostDataDetail()
                        {
                            DisposalCostPricePerTonne = "10",
                            England = "EnglandTest",
                            Wales = "WalesTest",
                            Name = "Material2",
                            Scotland = "ScotlandTest",
                            NorthernIreland = "NorthernIrelandTest",
                            ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
                            ReportedPublicBinTonnage = Fixture.Create<string>(),
                            Total = Fixture.Create<string>(),
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
                            EnglandDisposalTotal = "80",
                            NorthernIrelandDisposalTotal = "70",
                            ScotlandDisposalTotal = "30",
                            WalesDisposalTotal = "20",
                            AllTotal = 0.1M,
                            EnglandTotal = 0.10M,
                            NorthernIrelandTotal = 0.15M,
                            ScotlandTotal = 0.15M,
                            WalesTotal = 020M,
                            Name = "1 + 4 Apportionment %s",
                        },
                        new()
                        {
                            EnglandDisposalTotal = "80",
                            NorthernIrelandDisposalTotal = "70",
                            ScotlandDisposalTotal = "30",
                            WalesDisposalTotal = "20",
                            AllTotal = 0.1M,
                            EnglandTotal = 0.10M,
                            NorthernIrelandTotal = 0.15M,
                            ScotlandTotal = 0.15M,
                            WalesTotal = 020M,
                            Name = "Test",
                        },
                        new()
                        {
                            EnglandDisposalTotal = "80",
                            NorthernIrelandDisposalTotal = "70",
                            ScotlandDisposalTotal = "30",
                            WalesDisposalTotal = "20",
                            AllTotal = 0.1M,
                            EnglandTotal = 0.10M,
                            NorthernIrelandTotal = 0.15M,
                            ScotlandTotal = 0.15M,
                            WalesTotal = 020M,
                            Name = "Test",
                        },
                        new()
                        {
                            EnglandDisposalTotal = "80",
                            NorthernIrelandDisposalTotal = "70",
                            ScotlandDisposalTotal = "30",
                            WalesDisposalTotal = "20",
                            AllTotal = 0.1M,
                            EnglandTotal = 14.53M,
                            NorthernIrelandTotal = 0.15M,
                            ScotlandTotal = 0.15M,
                            WalesTotal = 020M,
                            Name = "Test",
                        },
                        new()
                        {
                            EnglandDisposalTotal = "80",
                            NorthernIrelandDisposalTotal = "70",
                            ScotlandDisposalTotal = "30",
                            WalesDisposalTotal = "20",
                            AllTotal = 0.1M,
                            EnglandTotal = 14.53M,
                            NorthernIrelandTotal = 0.15M,
                            ScotlandTotal = 0.15M,
                            WalesTotal = 020M,
                            Name = "Test",
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
                                new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>() { },
                            ProducerDisposalFeesByMaterial =
                                new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>() { },
                            ProducerId = "1",
                            ProducerName = "Test",
                            TotalProducerDisposalFeeWithBadDebtProvision = 100,
                            TotalProducerCommsFeeWithBadDebtProvision = 100,
                            SubsidiaryId = "1",
                            TotalProducerFeeforLADisposalCostswithBadDebtprovision = 10,
                            TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision = 10,
                            TotalProducerFeeWithBadDebtFor2bComms = 10,
                            TwoCTotalProducerFeeForCommsCostsWithBadDebt = 10
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
                        new()
                        {
                            CommsCostByMaterialPricePerTonne = "0.42",
                            Name = "Aluminium",

                        },
                        new()
                        {
                            CommsCostByMaterialPricePerTonne = "0.3",
                            Name = "Glass",

                        }
                    ]
                }
            };
        }

        private Fixture Fixture { get; init; }

        [TestCleanup]
        public void TearDown()
        {
            _dbContext?.Database.EnsureDeleted();
        }

        [TestMethod]
        public void CanCallGetHeaders()
        {
            // Act
            var result = OnePlus2A2B2CProducer.GetHeaders().ToList();
            var columnIndex = 242;

            var expectedResult = new List<CalcResultSummaryHeader>();
            expectedResult.AddRange([
                new CalcResultSummaryHeader { Name = OnePlus2A2B2CHeaders.ProducerTotalWithBadDebtProvision, ColumnIndex = columnIndex },
                new CalcResultSummaryHeader { Name = OnePlus2A2B2CHeaders.ProducerPercentageOfOverallProducerCost, ColumnIndex = columnIndex+1 },
            ]);

            // Assert
            Assert.AreEqual(expectedResult[0].Name, result[0].Name);
            Assert.AreEqual(expectedResult[0].ColumnIndex, result[0].ColumnIndex);
            Assert.AreEqual(expectedResult[1].Name, result[1].Name);
            Assert.AreEqual(expectedResult[1].ColumnIndex, result[1].ColumnIndex);
        }

        [TestMethod]
        public void CanCallGetSummaryHeaders()
        {
            // Act
            var result = OnePlus2A2B2CProducer.GetSummaryHeaders().ToList();

            var expectedResult = new List<CalcResultSummaryHeader>();
            expectedResult.AddRange([
                new CalcResultSummaryHeader { Name = OnePlus2A2B2CHeaders.TotalWithBadDebtProvision, ColumnIndex = 242 },
            ]);

            // Assert
            Assert.AreEqual(expectedResult[0].Name, result[0].Name);
            Assert.AreEqual(expectedResult[0].ColumnIndex, result[0].ColumnIndex);
        }

        [TestMethod]
        public void CanCallSetValues()
        {
            // Act
            OnePlus2A2B2CProducer.SetValues(_calcResult.CalcResultSummary);

            // Assert
            Assert.AreEqual(400, _calcResult.CalcResultSummary.TotalOnePlus2A2B2CFeeWithBadDebtProvision);
            Assert.AreEqual(40, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].ProducerTotalOnePlus2A2B2CWithBadDeptProvision);
            Assert.AreEqual(10.0m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].ProducerOverallPercentageOfCostsForOnePlus2A2B2C);
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
