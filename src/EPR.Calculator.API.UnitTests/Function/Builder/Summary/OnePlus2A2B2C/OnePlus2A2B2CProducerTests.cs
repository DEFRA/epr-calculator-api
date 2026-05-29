using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary.OnePlus2A2B2C;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.OnePlus2A2B2C
{
    [TestClass]
    public class OnePlus2A2B2CProducerTests
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly CalcResult _calcResult;
        private readonly int columnIndex = 266;

        public OnePlus2A2B2CProducerTests()
        {
            Fixture = new Fixture();
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _dbContext = new ApplicationDBContext(dbContextOptions);
            _dbContext.Database.EnsureCreated();

            CreateMaterials();
            CreateProducerDetail();

            _calcResult = TestDataHelper.GetCalcResult();
        }

        private Fixture Fixture { get; init; }

        [TestCleanup]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
        }

        [TestMethod]
        public void CanCallGetHeaders()
        {
            // Act
            var result = OnePlus2A2B2CProducer.GetHeaders().ToList();

            var expectedResult = new List<CalcResultSummaryHeader>();
            expectedResult.AddRange([
                new CalcResultSummaryHeader { Name = OnePlus2A2B2CHeaders.ProducerTotalWithBadDebtProvision },
                new CalcResultSummaryHeader { Name = OnePlus2A2B2CHeaders.ProducerPercentageOfOverallProducerCost },
            ]);

            // Assert
            Assert.AreEqual(expectedResult[0].Name, result[0].Name);
            Assert.AreEqual(expectedResult[1].Name, result[1].Name);
        }

        [TestMethod]
        public void CanCallGetSummaryHeaders()
        {
            // Act
            var result = OnePlus2A2B2CProducer.GetSummaryHeaders(columnIndex).ToList();

            var expectedResult = new List<CalcResultSummaryHeader>();
            expectedResult.AddRange([
                new CalcResultSummaryHeader { Name = OnePlus2A2B2CHeaders.TotalWithBadDebtProvision },
            ]);

            // Assert
            Assert.AreEqual(expectedResult[0].Name, result[0].Name);
        }

        [TestMethod]
        public void CanCallSetValues()
        {
            // Act
            OnePlus2A2B2CProducer.SetValues(_calcResult.CalcResultSummary);

            // Assert
            Assert.AreEqual(8895.914874216554m, _calcResult.CalcResultSummary.TotalOnePlus2A2B2CFeeWithBadDebtProvision);
            Assert.AreEqual(10491.1677668441235m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].ProducerTotalOnePlus2A2B2CWithBadDeptProvision);
            Assert.AreEqual(117.93242083791927397189538682m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].ProducerOverallPercentageOfCostsForOnePlus2A2B2C);
        }

        [TestMethod]
        public void CanCallSetValues_NullLocalAuthority()
        {
            // Act
            var data = _calcResult.CalcResultSummary;
            data.ProducerDisposalFees.ToList()[0].LocalAuthorityDisposalCostsSectionOne = null;
            data.ProducerDisposalFees.ToList()[0].CommunicationCostsSectionTwoA = null;
            data.ProducerDisposalFees.ToList()[0].CommunicationCostsSectionTwoB = null;

            data.TotalFeeforLADisposalCostswithBadDebtprovision1 = 0;
            data.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A = 0;
            data.CommsCostHeaderWithBadDebtFor2bTitle = 0;
            data.TwoCCommsCostsByCountryWithBadDebtProvision = 0;

            OnePlus2A2B2CProducer.SetValues(data);

            // Assert
            Assert.AreEqual(0, data.TotalOnePlus2A2B2CFeeWithBadDebtProvision);
            Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].ProducerTotalOnePlus2A2B2CWithBadDeptProvision);
            Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].ProducerOverallPercentageOfCostsForOnePlus2A2B2C);
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
                    Description = "Some",
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
                "Jumbo Box Store",
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