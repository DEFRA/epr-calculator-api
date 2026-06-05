using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary.TotalBillBreakdown;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.TotalBillBreakdown
{
    /// <summary>
    /// Defines the <see cref="TotalBillBreakdownProducerTests" />
    /// </summary>
    [TestClass]
    public class TotalBillBreakdownProducerTests
    {
        /// <summary>
        /// Defines the _dbContext
        /// </summary>
        private readonly ApplicationDBContext _dbContext;

        /// <summary>
        /// Defines the _materials
        /// </summary>
        private readonly IEnumerable<MaterialDetail> _materials;

        /// <summary>
        /// Defines the _calcResult
        /// </summary>
        private readonly CalcResult _calcResult;

        private readonly int columnIndex = 289;

        /// <summary>
        /// Initializes a new instance of the <see cref="TotalBillBreakdownProducerTests"/> class.
        /// </summary>
        public TotalBillBreakdownProducerTests()
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

            _calcResult = TestDataHelper.GetCalcResult();
        }

        /// <summary>
        /// The TearDown
        /// </summary>
        [TestCleanup]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
        }

        /// <summary>
        /// The CanCallGetHeaders
        /// </summary>
        [TestMethod]
        public void CanCallGetHeaders()
        {
            // Act
            var result = TotalBillBreakdownProducer.GetHeaders().ToList();

            var expectedResult = new List<CalcResultSummaryHeader>();
            expectedResult.AddRange([
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.TotalProducerBillWithoutBadDebtProvision },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.BadDebtProvision },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.TotalProducerBillWithBadDebtProvision },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.EnglandTotalWithBadDebtProvision },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.WalesTotalWithBadDebtProvision },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.ScotlandTotalWithBadDebtProvision },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.NorthernIrelandTotalWithBadDebtProvision }
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

        /// <summary>
        /// The CanCallGetSummaryHeaders
        /// </summary>
        [TestMethod]
        public void CanCallGetSummaryHeaders()
        {
            // Act
            var result = TotalBillBreakdownProducer.GetSummaryHeaders(columnIndex).ToList();

            var expectedResult = new List<CalcResultSummaryHeader>();
            expectedResult.AddRange([
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.TotalProducerBillBreakdown, ColumnIndex = columnIndex }
            ]);

            // Assert
            Assert.AreEqual(expectedResult[0].Name, result[0].Name);
            Assert.AreEqual(expectedResult[0].ColumnIndex, result[0].ColumnIndex);
        }

        /// <summary>
        /// The CanCallSetValues
        /// </summary>
        [TestMethod]
        public void CanCallSetValues()
        {
            // Act
            TotalBillBreakdownProducer.SetValues(_calcResult.CalcResultSummary);

            // Assert
            Assert.AreEqual(17673.2373499970378m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.TotalProducerFeeWithoutBadDebtProvision);
            Assert.AreEqual(1060.39424099982226m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.BadDebtProvision);
            Assert.AreEqual(18733.6315909968600m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.TotalProducerFeeWithBadDebtProvision);
            Assert.AreEqual(9610.6053147004709m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.EnglandTotalWithBadDebtProvision);
            Assert.AreEqual(2653.2546023494487m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.WalesTotalWithBadDebtProvision);
            Assert.AreEqual(4576.19121409722784m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.ScotlandTotalWithBadDebtProvision);
            Assert.AreEqual(1893.58045984971257m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.NorthernIrelandTotalWithBadDebtProvision);
        }

        /// <summary>
        /// The CanCallSetValues
        /// </summary>
        [TestMethod]
        public void CanCallSetValues_NullValues()
        {
            // Arrange
            var data = _calcResult.CalcResultSummary;
            data.ProducerDisposalFees.ToList()[0].LocalAuthorityDisposalCostsSectionOne = null;
            data.ProducerDisposalFees.ToList()[0].CommunicationCostsSectionTwoA = null;
            data.ProducerDisposalFees.ToList()[0].CommunicationCostsSectionTwoB = null;
            data.ProducerDisposalFees.ToList()[0].TwoCTotalProducerFeeForCommsCostsWithoutBadDebt = 0;
            data.ProducerDisposalFees.ToList()[0].TwoCBadDebtProvision = 0;
            data.ProducerDisposalFees.ToList()[0].TwoCTotalProducerFeeForCommsCostsWithBadDebt = 0;
            data.ProducerDisposalFees.ToList()[0].TwoCEnglandTotalWithBadDebt = 0;
            data.ProducerDisposalFees.ToList()[0].TwoCWalesTotalWithBadDebt = 0;
            data.ProducerDisposalFees.ToList()[0].TwoCScotlandTotalWithBadDebt = 0;
            data.ProducerDisposalFees.ToList()[0].TwoCNorthernIrelandTotalWithBadDebt = 0;
            data.ProducerDisposalFees.ToList()[0].SchemeAdministratorOperatingCosts = null;
            data.ProducerDisposalFees.ToList()[0].LocalAuthorityDataPreparationCosts = null;
            data.ProducerDisposalFees.ToList()[0].OneOffSchemeAdministrationSetupCosts = null;

            // Act
            TotalBillBreakdownProducer.SetValues(data);

            // Assert
            Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.TotalProducerFeeWithoutBadDebtProvision);
            Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.BadDebtProvision);
            Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.TotalProducerFeeWithBadDebtProvision);
            Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.EnglandTotalWithBadDebtProvision);
            Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.WalesTotalWithBadDebtProvision);
            Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.ScotlandTotalWithBadDebtProvision);
            Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.NorthernIrelandTotalWithBadDebtProvision);
        }

        /// <summary>
        /// The CreateMaterials
        /// </summary>
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

        /// <summary>
        /// The CreateProducerDetail
        /// </summary>
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
                            PackagingTonnage = materialId * 50,
                        });
                        _dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
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

            _dbContext.SaveChanges();
        }
    }
}