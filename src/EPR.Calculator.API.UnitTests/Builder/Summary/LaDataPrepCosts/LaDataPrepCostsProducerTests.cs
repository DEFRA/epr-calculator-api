namespace EPR.Calculator.API.UnitTests.Builder.Summary.LaDataPrepCosts
{
    using AutoFixture;
    using EPR.Calculator.API.Builder.Summary.LaDataPrepCosts;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.API.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;

    [TestClass]
    public class LaDataPrepCostsProducerTests
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly IEnumerable<MaterialDetail> _materials;
        private readonly CalcResult _calcResult;
        private readonly Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> _materialCostSummary;
        private readonly Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> _commsCostSummary;

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

            _materials = [
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
            ];

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
                            ProducerCommsFeesByMaterial =  new Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>(){ },
                            ProducerDisposalFeesByMaterial = new Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>(){ },
                            ProducerId ="1",
                            ProducerName ="Test",
                            TotalProducerDisposalFeeWithBadDebtProvision =100,
                            TotalProducerCommsFeeWithBadDebtProvision =100,
                            SubsidiaryId ="1",
                        }
                    }
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

            _materialCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>();
            _commsCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>();

            foreach (var material in _materials)
            {
                _materialCostSummary.Add(material, new CalcResultSummaryProducerDisposalFeesByMaterial
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
                });

                _commsCostSummary.Add(material, new CalcResultSummaryProducerCommsFeesCostByMaterial
                {
                    HouseholdPackagingWasteTonnage = 1000,
                    PriceperTonne = 0.6676m,
                    ProducerTotalCostWithoutBadDebtProvision = 607.52m,
                    BadDebtProvision = 36.45m,
                    ProducerTotalCostwithBadDebtProvision = 643.97m,
                    EnglandWithBadDebtProvision = 348.06m,
                    WalesWithBadDebtProvision = 78.46m,
                    ScotlandWithBadDebtProvision = 156.28m,
                    NorthernIrelandWithBadDebtProvision = 61.18m
                });
            }
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
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.TotalProducerFeeWithoutBadDebtProvision , ColumnIndex = 217 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.BadDebtProvision, ColumnIndex = 218 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.TotalProducerFeeWithBadDebtProvision, ColumnIndex = 219 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.EnglandTotalWithBadDebtProvision, ColumnIndex = 220 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.WalesTotalWithBadDebtProvision, ColumnIndex = 221 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.ScotlandTotalWithBadDebtProvision, ColumnIndex = 222 },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.NorthernIrelandTotalWithBadDebtProvision, ColumnIndex = 223 }
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
        public void CanCallGetLaDataPrepCostsProducerFeeWithoutBadDebtProvision()
        {
            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithoutBadDebtProvision(_dbContext.ProducerDetail, _materials, _calcResult, _materialCostSummary, _commsCostSummary);

            // Assert
            Assert.AreEqual((decimal)736.39, Math.Round(result, 2));
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsBadDebtProvision()
        {
            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsBadDebtProvision(_dbContext.ProducerDetail, _materials, _calcResult, _materialCostSummary, _commsCostSummary);

            // Assert
            Assert.AreEqual((decimal)44.18, Math.Round(result, 2));
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsProducerFeeWithBadDebtProvision()
        {
            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithBadDebtProvision(_dbContext.ProducerDetail, _materials, _calcResult, _materialCostSummary, _commsCostSummary);

            // Assert
            Assert.AreEqual((decimal)780.57, Math.Round(result, 2));
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionTotal()
        {
            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionTotal(_dbContext.ProducerDetail, _dbContext.ProducerDetail, _materials, _calcResult);

            // Assert
            Assert.AreEqual(100, result);
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsBadDebtProvisionTotal()
        {
            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsBadDebtProvisionTotal(_dbContext.ProducerDetail, _dbContext.ProducerDetail, _materials, _calcResult);

            // Assert
            Assert.AreEqual(6, result);
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsProducerFeeWithBadDebtProvisionTotal()
        {
            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithBadDebtProvisionTotal(_dbContext.ProducerDetail, _dbContext.ProducerDetail, _materials, _calcResult);

            // Assert
            Assert.AreEqual(106, result);
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsEnglandTotalWithBadDebtProvision()
        {
            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsEnglandTotalWithBadDebtProvision(_dbContext.ProducerDetail, _materials, _calcResult, _materialCostSummary, _commsCostSummary);

            // Assert
            Assert.AreEqual((decimal)0.78, Math.Round(result, 2));
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsEnglandOverallTotalWithBadDebtProvision()
        {
            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsEnglandOverallTotalWithBadDebtProvision(_dbContext.ProducerDetail, _dbContext.ProducerDetail, _materials, _calcResult);

            // Assert
            Assert.AreEqual((decimal)0.11, Math.Round(result, 2));
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsWalesTotalWithBadDebtProvision()
        {
            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsWalesTotalWithBadDebtProvision(_dbContext.ProducerDetail, _materials, _calcResult, _materialCostSummary, _commsCostSummary);

            // Assert
            Assert.AreEqual((decimal)156.11, Math.Round(result, 2));
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsWalesOverallTotalWithBadDebtProvision()
        {
            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsWalesOverallTotalWithBadDebtProvision(_dbContext.ProducerDetail, _dbContext.ProducerDetail, _materials, _calcResult);

            // Assert
            Assert.AreEqual((decimal)21.20, Math.Round(result, 2));
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsScotlandTotalWithBadDebtProvision()
        {
            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsScotlandTotalWithBadDebtProvision(_dbContext.ProducerDetail, _materials, _calcResult, _materialCostSummary, _commsCostSummary);

            // Assert
            Assert.AreEqual((decimal)1.17, Math.Round(result, 2));
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsScotlandOverallTotalWithBadDebtProvision()
        {
            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsScotlandOverallTotalWithBadDebtProvision(_dbContext.ProducerDetail, _dbContext.ProducerDetail, _materials, _calcResult);

            // Assert
            Assert.AreEqual((decimal)0.16, Math.Round(result, 2));
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsNorthernIrelandTotalWithBadDebtProvision()
        {
            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsNorthernIrelandTotalWithBadDebtProvision(_dbContext.ProducerDetail, _materials, _calcResult, _materialCostSummary, _commsCostSummary);

            // Assert
            Assert.AreEqual((decimal)1.17, Math.Round(result, 2));
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsNorthernIrelandOverallTotalWithBadDebtProvision()
        {
            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsNorthernIrelandOverallTotalWithBadDebtProvision(_dbContext.ProducerDetail, _dbContext.ProducerDetail, _materials, _calcResult);

            // Assert
            Assert.AreEqual((decimal)0.16, Math.Round(result, 2));
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
