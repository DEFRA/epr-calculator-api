using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.Summary.LaDataPrepCosts;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.LaDataPrepCosts
{
    [TestClass]
    public class LaDataPrepCostsProducerTests
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly CalcResult _calcResult;
        private readonly int columnIndex = 275;

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
                ApplyModulation = false,
                CalcResultScaledupProducers = new CalcResultScaledupProducers(),
                CalcResultPartialObligations = new CalcResultPartialObligations(),
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost
                {
                    BadDebtValue = 6m,
                    LaDataPrepCharge = new() { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 },
                    CountryApportionment = new() { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 },
                    SaOperatingCost = new() { England = 0, Wales = 0, Scotland = 0, NorthernIreland = 0 },
                    SchemeSetupCost = new() { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 }
                },
                CalcResultDetail = new CalcResultDetail { RunId = 1, RelativeYear = new RelativeYear(2024) },
                CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData() { ByMaterial = [] },
                CalcResultLapcapData = new CalcResultLapcapData { ByMaterial = [] },
                CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment
                {
                    LaDisposalCost   = new() { England = 0.10M, Wales = 20M, Scotland = 0.15M, NorthernIreland = 0.15M },
                    LADataPrepCharge = new() { England = 0.10M, Wales = 20M, Scotland = 0.15M, NorthernIreland = 0.15M }
                },
                CalcResultSummary = new CalcResultSummary
                {
                    ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
                    {
                        new()
                        {
                            ProducerCommsFeesByMaterial =
                                new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>(),
                            ProducerDisposalFeesByMaterial =
                                new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>(),
                            ProducerId = "1",
                            ProducerName = "Test",
                            TotalProducerDisposalFeeWithBadDebtProvision = 100,
                            TotalProducerCommsFeeWithBadDebtProvision = 100,
                            SubsidiaryId = "1",
                            TwoCTotalProducerFeeForCommsCostsWithBadDebt = 10,
                            ProducerOverallPercentageOfCostsForOnePlus2A2B2C = 100,
                            LocalAuthorityDataPreparationCosts = new CalcResultSummaryBadDebtProvision
                            {
                                TotalProducerFeeWithoutBadDebtProvision = 100,
                                BadDebtProvision = 20,
                                TotalProducerFeeWithBadDebtProvision = 120,
                                EnglandTotalWithBadDebtProvision = 20,
                                WalesTotalWithBadDebtProvision = 20,
                                ScotlandTotalWithBadDebtProvision = 20,
                                NorthernIrelandTotalWithBadDebtProvision = 20
                            },
                            BillingInstructionSection = new CalcResultSummaryBillingInstruction {
                                SuggestedBillingInstruction = string.Empty
                            }
                        },
                    },
                    TotalFeeforLADisposalCostswithBadDebtprovision1 = 100,
                    TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A = 100,
                    CommsCostHeaderWithBadDebtFor2bTitle = 100,
                    TwoCCommsCostsByCountryWithBadDebtProvision = 100,
                },
                CalcResultCommsCostReportDetail = TestDataHelper.GetCalcResultCommsCostReportDetail(),
                CalcResultLateReportingTonnageData = Fixture.Create<CalcResultLateReportingTonnage>(),
                CalcResultProjectedProducers = new CalcResultProjectedProducers(),
            };
        }

        [TestCleanup]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [TestMethod]
        public void CanCallGetHeaders()
        {
            // Act
            var result = LaDataPrepCostsProducer.GetHeaders().ToList();
            var expectedResult = new List<CalcResultSummaryHeader>();
            expectedResult.AddRange([
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.TotalProducerFeeWithoutBadDebtProvision },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.BadDebtProvision },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.TotalProducerFeeWithBadDebtProvision },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.EnglandTotalWithBadDebtProvision },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.WalesTotalWithBadDebtProvision },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.ScotlandTotalWithBadDebtProvision },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.NorthernIrelandTotalWithBadDebtProvision }
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
        public void CanCallGetSummaryHeaders()
        {
            // Act
            var result = LaDataPrepCostsProducer.GetSummaryHeaders(columnIndex).ToList();

            var expectedResult = new List<CalcResultSummaryHeader>();
            expectedResult.AddRange([
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.LaDataPrepCostsWithoutBadDebtProvisionTitle },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.BadDebtProvisionTitle },
                new CalcResultSummaryHeader { Name = LaDataPrepCostsHeaders.LaDataPrepCostsWithBadDebtProvisionTitle }
            ]);

            // Assert
            Assert.AreEqual(expectedResult[0].Name, result[0].Name);
            Assert.AreEqual(expectedResult[1].Name, result[1].Name);
            Assert.AreEqual(expectedResult[2].Name, result[2].Name);
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
            Assert.AreEqual(100, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LocalAuthorityDataPreparationCosts!.TotalProducerFeeWithoutBadDebtProvision);
            Assert.AreEqual(6, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LocalAuthorityDataPreparationCosts!.BadDebtProvision);
            Assert.AreEqual(106, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LocalAuthorityDataPreparationCosts!.TotalProducerFeeWithBadDebtProvision);
            Assert.AreEqual(42.40m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LocalAuthorityDataPreparationCosts!.EnglandTotalWithBadDebtProvision);
            Assert.AreEqual(31.80m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LocalAuthorityDataPreparationCosts!.WalesTotalWithBadDebtProvision);
            Assert.AreEqual(21.20m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LocalAuthorityDataPreparationCosts!.ScotlandTotalWithBadDebtProvision);
            Assert.AreEqual(10.60m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].LocalAuthorityDataPreparationCosts!.NorthernIrelandTotalWithBadDebtProvision);
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
                    foreach (var subPeriod in new[] { "2025-H1", "2025-H2"}) {
                        _dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                        {
                            MaterialId = materialId,
                            ProducerDetailId = producerDetailId,
                            PackagingType = "HH",
                            SubmissionPeriod = subPeriod,
                            PackagingTonnage = (materialId * 50)
                        });
                        _dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                        {
                            MaterialId = materialId,
                            ProducerDetailId = producerDetailId,
                            PackagingType = "CW",
                            SubmissionPeriod = subPeriod,
                            PackagingTonnage = (materialId * 25)
                        });
                    }
                }
            }
            _dbContext.SaveChanges();
        }
    }
}
