using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.Summary.ThreeSa;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.ThreeSa
{
    [TestClass]
    public class ThreeSaCostsProducerTests
    {
        private ApplicationDBContext? _dbContext;
        private IEnumerable<MaterialDetail>? _materials;
        private CalcResult? _calcResult;
        private Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>? _materialCostSummary;
        private Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>? _commsCostSummary;

        [TestInitialize]
        public void TestInitialize()
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
                    SaOperatingCost  = new() { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 },
                    SchemeSetupCost  = new() { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 }
                },
                CalcResultDetail = new CalcResultDetail { RunId = 1, RelativeYear = new RelativeYear(2024) },
                CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData
                {
                    ByMaterial = new Dictionary<string, CalcResultLaDisposalCostDataDetail>
                    {
                        ["AL"] =
                            new CalcResultLaDisposalCostDataDetail
                            {
                                Cost = ByCountryCost.Empty,
                                HouseholdPackagingWasteTonnage = 0,
                                PublicBinTonnage = 0,
                                HouseholdDrinkContainersTonnage = 0
                            },
                        ["PL"] =
                            new CalcResultLaDisposalCostDataDetail
                            {
                                Cost = ByCountryCost.Empty,
                                HouseholdPackagingWasteTonnage = 0,
                                PublicBinTonnage = 0,
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
                            ProducerCommsFeesByMaterial =
                                new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>(),
                            ProducerDisposalFeesByMaterial =
                                new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>(),
                            ProducerId = "1",
                            ProducerName = "Test",
                            TotalProducerDisposalFeeWithBadDebtProvision = 100,
                            TotalProducerCommsFeeWithBadDebtProvision = 100,
                            SubsidiaryId = "1",
                            ProducerOverallPercentageOfCostsForOnePlus2A2B2C = 1,
                        },
                    },
                },
                CalcResultCommsCostReportDetail = TestDataHelper.GetCalcResultCommsCostReportDetail(),
                CalcResultLateReportingTonnageData = new() { ByMaterial = [] },
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
                    PricePerTonne = (total: 0.6676m, red: null, amber: null, green: null),
                    ProducerDisposalFee =(total: 607.52m, red: null, amber: null, green: null),
                    BadDebtProvision = 36.45m,
                    ProducerDisposalFeeWithBadDebtProvision = 643.97m,
                    EnglandWithBadDebtProvision = 348.06m,
                    WalesWithBadDebtProvision = 78.46m,
                    ScotlandWithBadDebtProvision = 156.28m,
                    NorthernIrelandWithBadDebtProvision = 61.18m,
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
                    NorthernIrelandWithBadDebtProvision = 61.18m,
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
            var result = ThreeSaCostsProducer.GetHeaders().ToList();

            var expectedResult = new List<CalcResultSummaryHeader>();
            expectedResult.AddRange([
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.TotalSaOperatingCostsWoTitleSection3 },
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.BadDebtProvisionSection3 },
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.SaOperatingCostsWithTitleSection3 },
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.EnglandTotalWithBadDebtProvisionSection3 },
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.WalesTotalWithBadDebtProvisionSection3 },
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.ScotlandTotalWithBadDebtProvisionSection3 },
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.NorthernIrelandTotalWithBadDebtProvisionSection3 }
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
            ThreeSaCostsProducer.GetProducerSetUpCostsSection3(_calcResult!, _calcResult!.CalcResultSummary);

            // Assert
            Assert.AreEqual(100, _calcResult.CalcResultSummary.SaOperatingCostsWoTitleSection3);
            Assert.AreEqual(6, _calcResult.CalcResultSummary.BadDebtProvisionTitleSection3);
            Assert.AreEqual(106, _calcResult.CalcResultSummary.SaOperatingCostsWithTitleSection3);
            Assert.AreEqual(1,
                _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0]
                    .SchemeAdministratorOperatingCosts!.TotalProducerFeeWithoutBadDebtProvision);
            Assert.AreEqual(0.06m,
                _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].SchemeAdministratorOperatingCosts!.BadDebtProvision);
            Assert.AreEqual(1.06m,
                _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0]
                    .SchemeAdministratorOperatingCosts!.TotalProducerFeeWithBadDebtProvision);
        }

        [TestMethod]
        public void CanCallGetSaSetupCostsEnglandOverallTotalWithBadDebtProvision()
        {
            ThreeSaCostsProducer.GetProducerSetUpCostsSection3(_calcResult!, _calcResult!.CalcResultSummary);
            // Act
            var result = ThreeSaCostsProducer.GetCountryTotalWithBadDebtProvision(
                _calcResult,
                _calcResult.CalcResultSummary.SaOperatingCostsWoTitleSection3,
                _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].ProducerOverallPercentageOfCostsForOnePlus2A2B2C,
                Countries.England
            );

            // Assert
            Assert.AreEqual(0.42m, Math.Round(result, 2));
        }

        [TestMethod]
        public void CanCallGetSaSetupCostsScotlandOverallTotalWithBadDebtProvision()
        {
            ThreeSaCostsProducer.GetProducerSetUpCostsSection3(_calcResult!, _calcResult!.CalcResultSummary);

            // Act
            var result = ThreeSaCostsProducer.GetCountryTotalWithBadDebtProvision(
                _calcResult,
                _calcResult.CalcResultSummary.SaOperatingCostsWoTitleSection3,
                _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].ProducerOverallPercentageOfCostsForOnePlus2A2B2C,
                Countries.Scotland
            );

            // Assert
            Assert.AreEqual(0.16m, Math.Round(result, 2));
        }

        [TestMethod]
        public void CanCallGetSaSetupCostsWalesOverallTotalWithBadDebtProvision()
        {
            ThreeSaCostsProducer.GetProducerSetUpCostsSection3(_calcResult!, _calcResult!.CalcResultSummary);

            // Act
            var result = ThreeSaCostsProducer.GetCountryTotalWithBadDebtProvision(
                _calcResult,
                _calcResult.CalcResultSummary.SaOperatingCostsWoTitleSection3,
                _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].ProducerOverallPercentageOfCostsForOnePlus2A2B2C,
                Countries.Wales
            );

            // Assert
            Assert.AreEqual(0.11m, Math.Round(result, 2));
        }

        private void CreateMaterials()
        {
            _materials = TestDataHelper.GetMaterials();

            _dbContext?.Material.AddRange(_materials.Select(m => new Material
            {
                Name = m.Name,
                Code = m.Code,
                Description = m.Description,
            }));
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
                _dbContext?.ProducerDetail.Add(new ProducerDetail
                {
                    ProducerId = producerId++,
                    SubsidiaryId = $"{producerId}-Sub",
                    ProducerName = producerName,
                    CalculatorRunId = 1,
                });
            }

            _dbContext?.SaveChanges();

            for (int producerDetailId = 1; producerDetailId <= 10; producerDetailId++)
            {
                for (int materialId = 1; materialId < 9; materialId++)
                {
                    foreach (var subPeriod in new[] { "2025-H1", "2025-H2"}) {
                        _dbContext?.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                        {
                            MaterialId = materialId,
                            ProducerDetailId = producerDetailId,
                            PackagingType = "HH",
                            SubmissionPeriod = subPeriod,
                            PackagingTonnage = materialId * 50,
                        });
                        _dbContext?.ProducerReportedMaterial.Add(new ProducerReportedMaterial
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

            _dbContext?.SaveChanges();
        }
    }
}
