using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.Builder;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class ProjectedProducersServiceTest
    {
        private IFixture _fixture = null!;
        private ApplicationDBContext _dbContext = null!;
        private ProjectedProducersService _sut = null!;

        [TestInitialize]
        public void Init()
        {
            _fixture = TestFixtures.New();
            _dbContext = _fixture.Freeze<ApplicationDBContext>();
            _sut = _fixture.Create<ProjectedProducersService>();
        }

        [TestCleanup]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
        }

        [TestMethod]
        public async Task StoreProjectedProducers_WorksAsExpected()
        {
            TestDataHelper.SeedDatabaseForInitialRun(_dbContext);

            ProducerReportedMaterial mkProducerReportedMaterial(string submissionPeriod, string material, string packagingType, decimal total, decimal? r, decimal? a)
            {
                return new ProducerReportedMaterial
                {
                    //MaterialId = int.Parse(material), // TODO look up materialId
                    //ProducerDetailId = { get; set; } // TODO how to get this?
                    PackagingType = packagingType,
                    PackagingTonnage = total,
                    PackagingTonnageRed = r,
                    PackagingTonnageAmber = a,
                    PackagingTonnageGreen = 0m,
                    PackagingTonnageRedMedical = null,
                    PackagingTonnageAmberMedical = null,
                    PackagingTonnageGreenMedical = null,
                    SubmissionPeriod = submissionPeriod,
                    ProducerDetail = null,
                    Material = null
                };
            }

            var producer1 = new ProducerDetail{
                ProducerId = 1,
                SubsidiaryId = null
            };
            producer1.ProducerReportedMaterials.Add(mkProducerReportedMaterial(submissionPeriod: "2025-H1", material: "ST", packagingType: "PB", total:   7, r:   2, a: 5 ));
            producer1.ProducerReportedMaterials.Add(mkProducerReportedMaterial(submissionPeriod: "2025-H2", material: "PL", packagingType: "HH", total:  12, r:   0, a: 11));
            producer1.ProducerReportedMaterials.Add(mkProducerReportedMaterial(submissionPeriod: "2025-H1", material: "ST", packagingType: "HH", total: 201, r: 201, a: 0 ));
            var producer2 = new ProducerDetail{
                ProducerId = 1,
                SubsidiaryId = "A"
            };
            producer2.ProducerReportedMaterials.Add(mkProducerReportedMaterial(submissionPeriod: "2025-H2", material: "ST", packagingType: "PB", total:   5, r:   1, a: 4 ));
            producer2.ProducerReportedMaterials.Add(mkProducerReportedMaterial(submissionPeriod: "2025-H2", material: "PL", packagingType: "HH", total:  10, r:   0, a: 10));
            producer2.ProducerReportedMaterials.Add(mkProducerReportedMaterial(submissionPeriod: "2025-H2", material: "ST", packagingType: "HH", total: 200, r: 200, a: 0 ));
            var producers = new List<L1Producer>
            {
                new L1Producer(1, [producer1, producer2])
            };

            await _sut.StoreProjectedProducers(1, producers);

            var stored = await _dbContext.ProducerReportedMaterialProjected.ToImmutableListAsync();
            stored.Count.ShouldBe(6);
        }
    }
}
