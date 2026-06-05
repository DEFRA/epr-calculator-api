using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.Summary.SaSetupCosts;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.SaSetupCosts
{
    [TestClass]
    public class SaSetupCostsProducerTests
    {
        private readonly ApplicationDBContext dbContext;
        private readonly IEnumerable<MaterialDetail> _materials;
        private readonly CalcResult _calcResult;
        private readonly Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> _materialCostSummary;
        private readonly Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> _commsCostSummary;

        private Fixture Fixture { get; init; } = new Fixture();

        public SaSetupCostsProducerTests()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();

            CreateMaterials();
            CreateProducerDetail();

            _materials = [
                new MaterialDetail
                {
                    Id = 1,
                    Code = "AL",
                    Name = "Aluminium",
                    Description = "Aluminium",
                },
                new MaterialDetail
                {
                    Id = 2,
                    Code = "FC",
                    Name = "Fibre composite",
                    Description = "Fibre composite",
                },
                new MaterialDetail
                {
                    Id = 3,
                    Code = "GL",
                    Name = "Glass",
                    Description = "Glass",
                },
                new MaterialDetail
                {
                    Id = 4,
                    Code = "PC",
                    Name = "Paper or card",
                    Description = "Paper or card",
                },
                new MaterialDetail
                {
                    Id = 5,
                    Code = "PL",
                    Name = "Plastic",
                    Description = "Plastic",
                },
                new MaterialDetail
                {
                    Id = 6,
                    Code = "ST",
                    Name = "Steel",
                    Description = "Steel",
                },
                new MaterialDetail
                {
                    Id = 7,
                    Code = "WD",
                    Name = "Wood",
                    Description = "Wood",
                },
                new MaterialDetail
                {
                    Id = 8,
                    Code = "OT",
                    Name = "Other materials",
                    Description = "Other materials",
                }
            ];

            _calcResult = new CalcResult
            {
                ApplyModulation = false,
                CalcResultScaledupProducers = new CalcResultScaledupProducers(),
                CalcResultPartialObligations = new CalcResultPartialObligations(),
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost
                {
                    BadDebtValue = 6m,
                    LaDataPrepCharge = new() { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 },
                    SaOperatingCost  = new() { England =  0, Wales =  0, Scotland =  0, NorthernIreland =  0 },
                    SchemeSetupCost  = new() { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 }
                },
                CalcResultDetail = new CalcResultDetail { RunId = 1, RelativeYear = new RelativeYear(2024) },
                CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData
                {
                    ByMaterial = new Dictionary<string, CalcResultLaDisposalCostDataDetail>
                    {
                        ["Material1"] =
                            new CalcResultLaDisposalCostDataDetail
                            {
                                Cost = ByCountryCost.Empty,
                                HouseholdPackagingWasteTonnage = Fixture.Create<decimal>(),
                                PublicBinTonnage = Fixture.Create<decimal>(),
                                HouseholdDrinkContainersTonnage = 0
                            },
                        ["Material2"] =
                            new CalcResultLaDisposalCostDataDetail
                            {
                                Cost = ByCountryCost.Empty,
                                HouseholdPackagingWasteTonnage = Fixture.Create<decimal>(),
                                PublicBinTonnage = Fixture.Create<decimal>(),
                                HouseholdDrinkContainersTonnage = 0
                            }
                    }
                },
                CalcResultLapcapData = new CalcResultLapcapData
                {
                    ByMaterial = []
                },
                CalcResultOnePlusFourApportionment = TestDataHelper.GetCalcResultOnePlusFourApportionment(),
                CalcResultSummary = new CalcResultSummary
                {
                    ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
                    {
                        new()
                        {
                            ProducerCommsFeesByMaterial =  new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>(),
                            ProducerDisposalFeesByMaterial = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>(),
                            ProducerId ="1",
                            ProducerName ="Test",
                            TotalProducerDisposalFeeWithBadDebtProvision =100,
                            TotalProducerCommsFeeWithBadDebtProvision =100,
                            SubsidiaryId ="1",
                            ProducerOverallPercentageOfCostsForOnePlus2A2B2C = 1,
                        },
                    },
                },
                CalcResultCommsCostReportDetail = TestDataHelper.GetCalcResultCommsCostReportDetail(),
                CalcResultLateReportingTonnageData = Fixture.Create<CalcResultLateReportingTonnage>(),
                CalcResultProjectedProducers = new CalcResultProjectedProducers(),
            };

            _materialCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>();
            _commsCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>();

            foreach (var material in _materials)
            {
                _materialCostSummary.Add(material, new CalcResultSummaryProducerDisposalFeesByMaterial
                {
                    HouseholdPackagingWasteTonnage = 1000,
                    SelfManagedConsumerWasteTonnage = 90,
                    NetReportedTonnage = (total: 910, red: null, amber: null, green: null),
                    PricePerTonne =  (total: 0.6676m, red: null, amber: null, green: null),
                    ProducerDisposalFee =  (total: 607.52m, red: null, amber: null, green: null),
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
            dbContext.Database.EnsureDeleted();
        }

        [TestMethod]
        public void CanCallGetHeaders()
        {
            // Act
            var result = SaSetupCostsProducer.GetHeaders().ToList();

            var expectedResult = new List<CalcResultSummaryHeader>();
            expectedResult.AddRange([
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.ProducerOneOffFeeWithoutBadDebtProvision },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.BadDebtProvision },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.ProducerOneOffFeeWithBadDebtProvision },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.EnglandTotalWithBadDebtProvision },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.WalesTotalWithBadDebtProvision },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.ScotlandTotalWithBadDebtProvision },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.NorthernIrelandTotalWithBadDebtProvision }
            ]);

            // Assert
            Assert.AreEqual(expectedResult[0].Name, result[0].Name);
            Assert.AreEqual(expectedResult[1].Name, result[1].Name);
            Assert.AreEqual(expectedResult[2].Name, result[2].Name);
            Assert.AreEqual(expectedResult[3].Name, result[3].Name);
            Assert.AreEqual(expectedResult[4].Name, result[4].Name);
            Assert.AreEqual(expectedResult[5].Name, result[5].Name);
            Assert.AreEqual(expectedResult[6].Name, result[6].Name);
        }

        [TestMethod]
        public void CanCallSaSetupCostsProducerFeeWithoutBadDebtProvision()
        {
            // Act
            SaSetupCostsProducer.GetProducerSetUpCosts(_calcResult, _calcResult.CalcResultSummary);

            // Assert
            Assert.AreEqual(100, _calcResult.CalcResultSummary.SaSetupCostsTitleSection5);
            Assert.AreEqual(6, _calcResult.CalcResultSummary.SaSetupCostsBadDebtProvisionTitleSection5);
            Assert.AreEqual(106, _calcResult.CalcResultSummary.SaSetupCostsWithBadDebtProvisionTitleSection5);
            Assert.AreEqual(1, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].OneOffSchemeAdministrationSetupCosts!.TotalProducerFeeWithoutBadDebtProvision);
            Assert.AreEqual(0.06m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].OneOffSchemeAdministrationSetupCosts!.BadDebtProvision);
            Assert.AreEqual(1.06m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].OneOffSchemeAdministrationSetupCosts!.TotalProducerFeeWithBadDebtProvision);
        }

        [TestMethod]
        public void CanCallGetSaSetupCostsEnglandOverallTotalWithBadDebtProvision()
        {
            SaSetupCostsProducer.GetProducerSetUpCosts(_calcResult, _calcResult.CalcResultSummary);
            // Act
            var result = SaSetupCostsProducer.GetCountryTotalWithBadDebtProvision(_calcResult, _calcResult.CalcResultSummary.SaSetupCostsTitleSection5, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.England);

            // Assert
            Assert.AreEqual(0.42m, Math.Round(result, 2));
        }

        [TestMethod]
        public void CanCallGetSaSetupCostsScotlandOverallTotalWithBadDebtProvision()
        {
            SaSetupCostsProducer.GetProducerSetUpCosts(_calcResult, _calcResult.CalcResultSummary);

            // Act
            var result = SaSetupCostsProducer.GetCountryTotalWithBadDebtProvision(_calcResult, _calcResult.CalcResultSummary.SaSetupCostsTitleSection5, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.Scotland);

            // Assert
            Assert.AreEqual(0.16m, Math.Round(result, 2));
        }

        [TestMethod]
        public void CanCallGetSaSetupCostsWalesOverallTotalWithBadDebtProvision()
        {
            SaSetupCostsProducer.GetProducerSetUpCosts(_calcResult, _calcResult.CalcResultSummary);

            // Act
            var result = SaSetupCostsProducer.GetCountryTotalWithBadDebtProvision(_calcResult, _calcResult.CalcResultSummary.SaSetupCostsTitleSection5, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.Wales);

            // Assert
            Assert.AreEqual(0.11m, Math.Round(result, 2));
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
                dbContext.Material.Add(new Material
                {
                    Name = materialKv.Value,
                    Code = materialKv.Key,
                    Description = "Some",
                });
            }

            dbContext.SaveChanges();
        }

        private void CreateProducerDetail()
        {
            var producerNames = new[]
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
                "Jumbo Box Store",
            };

            var producerId = 1;
            foreach (var producerName in producerNames)
            {
                dbContext.ProducerDetail.Add(new ProducerDetail
                {
                    ProducerId = producerId++,
                    SubsidiaryId = $"{producerId}-Sub",
                    ProducerName = producerName,
                    CalculatorRunId = 1,
                });
            }

            dbContext.SaveChanges();

            for (int producerDetailId = 1; producerDetailId <= 10; producerDetailId++)
            {
                for (int materialId = 1; materialId < 9; materialId++)
                {
                    foreach (var subPeriod in new[] { "2025-H1", "2025-H2"}) {
                        dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                        {
                            MaterialId = materialId,
                            ProducerDetailId = producerDetailId,
                            PackagingType = "HH",
                            SubmissionPeriod = subPeriod,
                            PackagingTonnage = materialId * 50,
                        });
                        dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                        {
                            MaterialId = materialId,
                            ProducerDetailId = producerDetailId,
                            PackagingType = "CW",
                            SubmissionPeriod = subPeriod,
                            PackagingTonnage = materialId * 25,
                        });
                    }
                }
            }

            dbContext.SaveChanges();
        }
    }
}
