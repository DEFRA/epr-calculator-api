using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.Fixtures;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataTypes;
using EPR.CommonDataService.DataApi.Alignment;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Services
{
    /// <summary>
    ///     Unit tests for <see cref="ErrorReportService" />.
    ///     <para>
    ///         DataApi computes every error/warning unconditionally, flagging whether it found a
    ///         current-year POM match (<see cref="ProducerCalculationError.HasPomMatch" />) - it can't see
    ///         billing history, so it doesn't know whether a no-POM-match row is still worth showing. This
    ///         service makes that call (keep it if the organisation was invoiced in a previous run this
    ///         financial year), then rolls up a holding-company-level error for any producer whose
    ///         surviving errors are all subsidiary-scoped, then persists the result.
    ///     </para>
    /// </summary>
    [TestClass]
    public class ErrorReportServiceTests
    {
        private ApplicationDBContext _dbContext = null!;
        private Mock<IInvoicedProducerService> _invoicedProducerService = null!;
        private ErrorReportService _sut = null!;

        [TestInitialize]
        public void Setup()
        {
            var fixture = TestFixtures.New();
            _dbContext = fixture.Freeze<ApplicationDBContext>();
            _invoicedProducerService = fixture.Freeze<Mock<IInvoicedProducerService>>();

            _invoicedProducerService
                .Setup(s => s.GetInvoicedProducers(It.IsAny<RelativeYear>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ImmutableList<InvoicedProducer>.Empty);

            _sut = fixture.Create<ErrorReportService>();
        }

        [TestMethod]
        public async Task PersistErrors_KeepsErrorWithPomMatch_RegardlessOfInvoiceStatus()
        {
            // Org 2's own error is holding-level (SubsidiaryId null), so no roll-up gets added for it -
            // isolates this test to just the one row under test.
            var errors = new[] { CreateError(2, null, "Missing POM Data", "01", isWarning: false, hasPomMatch: true) };

            await _sut.PersistErrors(errors, 300, "test user", new RelativeYear(2025), CancellationToken.None);

            var reports = _dbContext.ErrorReports.ToList();
            Assert.AreEqual(1, reports.Count);
            Assert.IsTrue(reports.Any(r => r.ProducerId == 2 && r.SubsidiaryId == null && r.ErrorCode == "Missing POM Data" && r.LeaverCode == "01"));
        }

        [TestMethod]
        public async Task PersistErrors_KeepsErrorWithNoPomMatch_WhenOrganisationWasInvoiced()
        {
            _invoicedProducerService
                .Setup(s => s.GetInvoicedProducers(It.IsAny<RelativeYear>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ImmutableList.Create(CreateInvoicedProducer(1)));

            var errors = new[] { CreateError(1, null, "some synapse error", "16", isWarning: false, hasPomMatch: false) };

            await _sut.PersistErrors(errors, 300, "test user", new RelativeYear(2025), CancellationToken.None);

            var reports = _dbContext.ErrorReports.ToList();
            Assert.AreEqual(1, reports.Count);
            Assert.IsTrue(reports.Any(r => r.ProducerId == 1 && r.ErrorCode == "some synapse error"));
        }

        [TestMethod]
        public async Task PersistErrors_DropsErrorWithNoPomMatch_WhenOrganisationWasNotInvoiced()
        {
            var errors = new[] { CreateError(1, null, "some synapse error", "16", isWarning: false, hasPomMatch: false) };

            await _sut.PersistErrors(errors, 300, "test user", new RelativeYear(2025), CancellationToken.None);

            Assert.AreEqual(0, _dbContext.ErrorReports.Count());
        }

        [TestMethod]
        public async Task PersistErrors_AddsHoldingCompanyRollup_WhenSurvivingErrorsAreAllSubsidiaryScoped()
        {
            var errors = new[] { CreateError(1, "101", "Missing POM Data", "01", isWarning: false, hasPomMatch: true) };

            await _sut.PersistErrors(errors, 300, "test user", new RelativeYear(2025), CancellationToken.None);

            var reports = _dbContext.ErrorReports.ToList();
            Assert.AreEqual(2, reports.Count, "Expected the subsidiary error plus a holding-company roll-up.");
            Assert.IsTrue(reports.Any(r => r.ProducerId == 1 && r.SubsidiaryId == "101"));
            Assert.IsTrue(reports.Any(r => r.ProducerId == 1 && r.SubsidiaryId == null && r.ErrorCode == "" && r.LeaverCode == ""));
        }

        [TestMethod]
        public async Task PersistErrors_DoesNotAddHoldingCompanyRollup_WhenAHoldingLevelErrorAlreadyExists()
        {
            var errors = new[]
            {
                CreateError(1, "101", "Missing POM Data", "01", isWarning: false, hasPomMatch: true),
                CreateError(1, null, "some synapse error", "16", isWarning: false, hasPomMatch: true)
            };

            await _sut.PersistErrors(errors, 300, "test user", new RelativeYear(2025), CancellationToken.None);

            var reports = _dbContext.ErrorReports.ToList();
            Assert.AreEqual(2, reports.Count, "Should not add a roll-up on top of an existing holding-level error.");
        }

        [TestMethod]
        public async Task PersistErrors_DoesNotOrphanRollup_WhenOnlySubsidiaryErrorWasFilteredOut()
        {
            // The producer's only error has no POM match and wasn't invoiced, so it's dropped entirely -
            // the roll-up must be computed after that filter, or this would leave a holding-level error
            // with no visible reason behind it.
            var errors = new[] { CreateError(1, "101", "some synapse error", "16", isWarning: false, hasPomMatch: false) };

            await _sut.PersistErrors(errors, 300, "test user", new RelativeYear(2025), CancellationToken.None);

            Assert.AreEqual(0, _dbContext.ErrorReports.Count());
        }

        [TestMethod]
        public async Task PersistErrors_KeepsWarnings()
        {
            var errors = new[] { CreateError(1, "101", "some warning", "01", isWarning: true, hasPomMatch: true) };

            await _sut.PersistErrors(errors, 300, "test user", new RelativeYear(2025), CancellationToken.None);

            var reports = _dbContext.ErrorReports.ToList();
            Assert.IsTrue(reports.Any(r => r.ProducerId == 1 && r.SubsidiaryId == "101" && r.ErrorCode == "some warning"));
        }

        [TestMethod]
        public async Task PersistErrors_WhenNoErrors_WritesNothing()
        {
            await _sut.PersistErrors([], 300, "test user", new RelativeYear(2025), CancellationToken.None);

            Assert.AreEqual(0, _dbContext.ErrorReports.Count());
        }

        [TestMethod]
        public async Task PersistErrors_SetsCalculatorRunIdAndCreatedBy()
        {
            var errors = new[] { CreateError(1, "101", "Missing POM Data", "01", isWarning: false, hasPomMatch: true) };

            await _sut.PersistErrors(errors, 300, "test user", new RelativeYear(2025), CancellationToken.None);

            var report = _dbContext.ErrorReports.Single(r => r.SubsidiaryId == "101");
            Assert.AreEqual(300, report.CalculatorRunId);
            Assert.AreEqual("test user", report.CreatedBy);
        }

        private static ProducerCalculationError CreateError(int orgId, string? subId, string errorCode, string leaverCode, bool isWarning, bool hasPomMatch) =>
            new()
            {
                OrganisationId = orgId,
                SubsidiaryId = subId,
                ErrorCode = errorCode,
                LeaverCode = leaverCode,
                IsWarning = isWarning,
                HasPomMatch = hasPomMatch
            };

        private static InvoicedProducer CreateInvoicedProducer(int producerId) => new()
        {
            CalculatorRunId = 0,
            CalculatorName = "ignored",
            ProducerId = producerId,
            ProducerName = "ignored",
            TradingName = null,
            MaterialId = 0,
            BillingInstructionId = null,
            InvoicedNetTonnage = null,
            CurrentYearInvoicedTotalAfterThisRun = null
        };
    }
}
