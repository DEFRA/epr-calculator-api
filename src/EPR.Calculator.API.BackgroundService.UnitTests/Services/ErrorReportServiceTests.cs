using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.Fixtures;
using EPR.Calculator.API.Data;
using EPR.CommonDataService.DataApi.Alignment;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Services
{
    /// <summary>
    ///     Unit tests for <see cref="ErrorReportService" />.
    ///     <para>
    ///         Error/warning detection now happens in DataApi (see <c>ProducerErrorDetectorTests</c>) - this
    ///         service's only job is persisting the <see cref="ProducerCalculationError" /> rows it's given.
    ///     </para>
    /// </summary>
    [TestClass]
    public class ErrorReportServiceTests
    {
        private ApplicationDBContext _dbContext = null!;
        private ErrorReportService _sut = null!;

        [TestInitialize]
        public void Setup()
        {
            var fixture = TestFixtures.New();
            _dbContext = fixture.Freeze<ApplicationDBContext>();

            _sut = fixture.Create<ErrorReportService>();
        }

        [TestMethod]
        public async Task PersistErrors_WritesOneErrorReportPerInputError()
        {
            // Arrange
            var runId = 300;
            var createdBy = "test user";

            var errors = new[]
            {
                new ProducerCalculationError { OrganisationId = 1, SubsidiaryId = "101", ErrorCode = "Missing POM Data", LeaverCode = "01", IsWarning = false },
                new ProducerCalculationError { OrganisationId = 2, SubsidiaryId = null, ErrorCode = "", LeaverCode = "", IsWarning = false },
                new ProducerCalculationError { OrganisationId = 3, SubsidiaryId = "303", ErrorCode = "some warning", LeaverCode = "some status", IsWarning = true }
            };

            // Act
            await _sut.PersistErrors(errors, runId, createdBy, CancellationToken.None);

            // Assert
            var reports = _dbContext.ErrorReports.ToList();
            Assert.AreEqual(3, reports.Count);

            foreach (var report in reports)
            {
                Assert.AreEqual(runId, report.CalculatorRunId);
                Assert.AreEqual(createdBy, report.CreatedBy);
            }

            Assert.IsTrue(reports.Any(r => r.ProducerId == 1 && r.SubsidiaryId == "101" && r.ErrorCode == "Missing POM Data" && r.LeaverCode == "01"));
            Assert.IsTrue(reports.Any(r => r.ProducerId == 2 && r.SubsidiaryId == null && r.ErrorCode == "" && r.LeaverCode == ""));
            Assert.IsTrue(reports.Any(r => r.ProducerId == 3 && r.SubsidiaryId == "303" && r.ErrorCode == "some warning" && r.LeaverCode == "some status"));
        }

        [TestMethod]
        public async Task PersistErrors_WhenNoErrors_WritesNothing()
        {
            // Act
            await _sut.PersistErrors([], 300, "test user", CancellationToken.None);

            // Assert
            Assert.AreEqual(0, _dbContext.ErrorReports.Count());
        }
    }
}
