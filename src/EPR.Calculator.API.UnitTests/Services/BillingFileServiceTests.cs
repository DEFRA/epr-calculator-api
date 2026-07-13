using System.Net;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.API.UnitTests.Services
{
    [TestClass]
    public class BillingFileServiceTests
    {
        private const string TestUser = "TestUser";

        private ApplicationDBContext dbContext = null!;
        private Mock<IBlobStorageService> blobStorage = null!;
        private BillingFileService service = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            // A uniquely named in-memory database keeps every test fully isolated.
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase($"BillingFileServiceTests-{Guid.NewGuid()}")
                .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            this.dbContext = new ApplicationDBContext(options);
            this.dbContext.Database.EnsureCreated();

            this.blobStorage = new Mock<IBlobStorageService>();
            this.service = new BillingFileService(this.dbContext, this.blobStorage.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.dbContext.Database.EnsureDeleted();
            this.dbContext.Dispose();
        }

        [TestMethod]
        public async Task UpdateProducerBillingInstructions_ReturnsUnprocessable_WhenRunDoesNotExist()
        {
            // Arrange
            const int nonExistentRunId = 999;

            // Act
            var result = await this.service.UpdateProducerBillingInstructionsAsync(
                nonExistentRunId, TestUser, AcceptRequest(1), CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.UnprocessableContent, result.StatusCode);
            Assert.AreEqual(CommonResources.InvalidRunId, result.Message);
        }

        [TestMethod]
        public async Task UpdateProducerBillingInstructions_ReturnsUnprocessable_WhenRunClassificationIsInvalid()
        {
            // Arrange
            await SeedAsync(CreateRun(1, RunClassification.UNCLASSIFIED));

            // Act
            var result = await this.service.UpdateProducerBillingInstructionsAsync(
                1, TestUser, AcceptRequest(1), CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.UnprocessableContent, result.StatusCode);
            Assert.AreEqual(CommonResources.InvalidRunId, result.Message);
        }

        [TestMethod]
        public async Task UpdateProducerBillingInstructions_ReturnsUnprocessable_WhenOrganisationIdIsInvalid()
        {
            // Arrange
            await SeedAsync(
                CreateRun(1),
                CreateBillingInstruction(producerId: 1, runId: 1, suggestedInstruction: "Initial"));

            // Act - organisation 2 has no billing instruction row for this run
            var result = await this.service.UpdateProducerBillingInstructionsAsync(
                1, TestUser, AcceptRequest(1, 2), CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.UnprocessableContent, result.StatusCode);
            Assert.AreEqual(CommonResources.InvalidOrganisationId, result.Message);
        }

        [TestMethod]
        public async Task UpdateProducerBillingInstructions_AcceptsInstruction_WhenRequestIsValid()
        {
            // Arrange
            await SeedAsync(
                CreateRun(1),
                CreateBillingInstruction(producerId: 1, runId: 1, suggestedInstruction: "Initial"));

            // Act
            var result = await this.service.UpdateProducerBillingInstructionsAsync(
                1, TestUser, AcceptRequest(1), CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);

            var updated = await GetBillingInstructionAsync(producerId: 1, runId: 1);
            Assert.AreEqual(BillingStatus.Accepted.ToString(), updated.BillingInstructionAcceptReject);
            Assert.IsNotNull(updated.LastModifiedAcceptReject);
            Assert.AreEqual(TestUser, updated.LastModifiedAcceptRejectBy);
        }

        [TestMethod]
        public async Task UpdateProducerBillingInstructions_RejectsInstructionWithReason_WhenRequestIsValid()
        {
            // Arrange
            await SeedAsync(
                CreateRun(1),
                CreateBillingInstruction(producerId: 1, runId: 1, suggestedInstruction: "Initial"));

            // Act
            var result = await this.service.UpdateProducerBillingInstructionsAsync(
                1, TestUser, RejectRequest("Test", 1), CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);

            var updated = await GetBillingInstructionAsync(producerId: 1, runId: 1);
            Assert.AreEqual(BillingStatus.Rejected.ToString(), updated.BillingInstructionAcceptReject);
            Assert.AreEqual("Test", updated.ReasonForRejection);
            Assert.IsNotNull(updated.LastModifiedAcceptReject);
            Assert.AreEqual(TestUser, updated.LastModifiedAcceptRejectBy);
        }

        [TestMethod]
        public async Task UpdateProducerBillingInstructions_ClearsReasonForRejection_WhenStatusChangesFromRejectedToAccepted()
        {
            // Arrange
            await SeedAsync(
                CreateRun(1),
                CreateBillingInstruction(producerId: 1, runId: 1, suggestedInstruction: "Initial"));

            // Act - reject with a reason, then accept the same instruction
            await this.service.UpdateProducerBillingInstructionsAsync(
                1, TestUser, RejectRequest("Initial rejection reason", 1), CancellationToken.None);
            await this.service.UpdateProducerBillingInstructionsAsync(
                1, TestUser, AcceptRequest(1), CancellationToken.None);

            // Assert
            var updated = await GetBillingInstructionAsync(producerId: 1, runId: 1);
            Assert.AreEqual(BillingStatus.Accepted.ToString(), updated.BillingInstructionAcceptReject);
            Assert.IsNull(updated.ReasonForRejection);
        }

        [TestMethod]
        public async Task StartGeneratingBillingFile_ReturnsOk_WhenRunIsValid()
        {
            // Arrange
            var run = CreateRun(1);
            run.BillingRunStatus = BillingRunStatus.None;
            await SeedAsync(run);

            // Act
            var result = await this.service.StartGeneratingBillingFileAsync(1, TestUser, CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        [TestMethod]
        public async Task MoveBillingJsonFile_ReturnsFalse_WhenMetadataNotFound()
        {
            // Arrange
            const int runWithoutMetadata = 999;

            // Act
            var result = await this.service.MoveBillingJsonFile(runWithoutMetadata, CancellationToken.None);

            // Assert
            result.ShouldBeFalse();
            this.blobStorage.Verify(
                x => x.MoveBillingJsonToFss(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never());
        }

        [TestMethod]
        public async Task MoveBillingJsonFile_MovesFileAndReturnsTrue_WhenMetadataExists()
        {
            // Arrange
            const string billingJsonFileName = "test-billing.json";
            await SeedAsync(new CalculatorRunBillingFileMetadata
            {
                CalculatorRunId = 1,
                BillingCsvFileName = "ignored",
                BillingJsonFileName = billingJsonFileName,
                BillingFileCreatedDate = DateTime.UtcNow,
                BillingFileCreatedBy = "test",
            });
            this.blobStorage
                .Setup(x => x.MoveBillingJsonToFss(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await this.service.MoveBillingJsonFile(1, CancellationToken.None);

            // Assert
            result.ShouldBeTrue();
            this.blobStorage.Verify(
                x => x.MoveBillingJsonToFss(billingJsonFileName, It.IsAny<CancellationToken>()),
                Times.Once());
        }

        [TestMethod]
        public async Task GetProducerBillingInstructions_ReturnsRecordsWithTotals_WhenRunHasInstructions()
        {
            // Arrange
            const string runName = "Test Run";
            await SeedAsync(
                CreateRun(1, name: runName),
                CreateBillingInstruction(producerId: 1, runId: 1, suggestedInstruction: "Initial", suggestedInvoiceAmount: 100, acceptRejectStatus: "Accepted"));

            // Act
            var result = await this.service.GetProducerBillingInstructionsAsync(1, CreateRequest(), CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.HasCount(1, result.Records);
            Assert.AreEqual(1, result.TotalRecords);
            Assert.AreEqual(1, result.TotalAcceptedRecords);
            Assert.AreEqual(1, result.TotalInitialRecords);
            Assert.AreEqual(0, result.TotalDeltaRecords);
            Assert.AreEqual(0, result.TotalRejectedRecords);
            Assert.AreEqual(0, result.TotalRebillRecords);
            Assert.AreEqual(0, result.TotalCancelBillRecords);
            Assert.AreEqual(0, result.TotalNoActionRecords);
            Assert.AreEqual(1, result.PageNumber);
            Assert.AreEqual(10, result.PageSize);
            Assert.AreEqual(runName, result.RunName);
        }

        [TestMethod]
        public async Task GetProducerBillingInstructions_ReturnsEmpty_WhenRunHasNoInstructions()
        {
            // Arrange
            await SeedAsync(CreateRun(1));

            // Act
            var result = await this.service.GetProducerBillingInstructionsAsync(1, CreateRequest(), CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result.Records);
            Assert.AreEqual(0, result.TotalRecords);
        }

        [TestMethod]
        public async Task GetProducerBillingInstructions_ReturnsNull_WhenRunDoesNotExist()
        {
            // Arrange
            const int nonExistentRunId = 999999;

            // Act
            var result = await this.service.GetProducerBillingInstructionsAsync(nonExistentRunId, CreateRequest(), CancellationToken.None);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetProducerBillingInstructions_UsesFallbackProducerName_WhenNameMissingInCurrentRun()
        {
            // Arrange - the current run holds no organisation data, so the name must be resolved
            // from an earlier run's organisation snapshot for the same relative year.
            const int missingProducerId = 999;

            var organisationSnapshot = new CalculatorRunOrganisationDataMaster
            {
                RelativeYear = new RelativeYear(2024),
                EffectiveFrom = DateTime.UtcNow.AddDays(-5),
                CreatedBy = "test",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
            };
            var previousRun = CreateRun(2, name: "Previous Run Snapshot");
            previousRun.CalculatorRunOrganisationDataMaster = organisationSnapshot;

            await SeedAsync(
                CreateRun(1, RunClassification.INTERIM_RECALCULATION_RUN, "Current Run"),
                CreateBillingInstruction(producerId: missingProducerId, runId: 1, suggestedInstruction: "Initial", suggestedInvoiceAmount: 100, acceptRejectStatus: "Pending"),
                organisationSnapshot,
                previousRun,
                new CalculatorRunOrganisationDataDetail
                {
                    CalculatorRunOrganisationDataMaster = organisationSnapshot,
                    OrganisationId = missingProducerId,
                    OrganisationName = "Fallback Producer Name",
                });

            // Act
            var result = await this.service.GetProducerBillingInstructionsAsync(1, CreateRequest(), CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.HasCount(1, result.Records);
            Assert.AreEqual("Fallback Producer Name", result.Records[0].ProducerName);
        }

        [TestMethod]
        public async Task GetProducerBillingInstructions_UsesPendingStatus_WhenAcceptRejectIsNull()
        {
            // Arrange
            await SeedAsync(
                CreateRun(1),
                CreateBillingInstruction(producerId: 2, runId: 1, suggestedInstruction: "Invoice", suggestedInvoiceAmount: 200, acceptRejectStatus: null));

            // Act
            var result = await this.service.GetProducerBillingInstructionsAsync(1, CreateRequest(), CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.HasCount(1, result.Records);
            Assert.AreEqual("Pending", result.Records[0].BillingInstructionAcceptReject);
            Assert.AreEqual(1, result.TotalPendingRecords);
            Assert.AreEqual(0, result.TotalAcceptedRecords);
        }

        [TestMethod]
        public async Task GetProducerBillingInstructions_UsesNoActionStatus_WhenInstructionIsNoActionPlaceholder()
        {
            // Arrange
            await SeedAsync(
                CreateRun(1),
                CreateBillingInstruction(producerId: 3, runId: 1, suggestedInstruction: "-", suggestedInvoiceAmount: 300, acceptRejectStatus: null));

            // Act
            var result = await this.service.GetProducerBillingInstructionsAsync(1, CreateRequest(), CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.HasCount(1, result.Records);
            Assert.AreEqual("Noaction", result.Records[0].BillingInstructionAcceptReject);
            Assert.AreEqual(1, result.TotalNoActionRecords);
        }

        [TestMethod]
        public async Task GetProducerBillingInstructions_UsesCurrentYearInvoiceTotal_WhenInstructionIsCancel()
        {
            // Arrange
            await SeedAsync(
                CreateRun(1),
                CreateBillingInstruction(producerId: 4, runId: 1, suggestedInstruction: "cancel", suggestedInvoiceAmount: 400, acceptRejectStatus: "Accepted", currentYearInvoiceTotalToDate: 500));

            // Act
            var result = await this.service.GetProducerBillingInstructionsAsync(1, CreateRequest(), CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.HasCount(1, result.Records);
            Assert.AreEqual(500, result.Records[0].SuggestedInvoiceAmount);
        }

        [TestMethod]
        public async Task GetProducerBillingInstructions_FiltersByOrganisationId()
        {
            // Arrange
            await SeedAsync(
                CreateRun(1),
                CreateBillingInstruction(producerId: 5, runId: 1, suggestedInstruction: "Initial", suggestedInvoiceAmount: 100, acceptRejectStatus: "Accepted"),
                CreateBillingInstruction(producerId: 6, runId: 1, suggestedInstruction: "Delta", suggestedInvoiceAmount: 200, acceptRejectStatus: "Pending"));
            var request = CreateRequest(new ProducerBillingInstructionsSearchQueryDto { OrganisationId = 5 });

            // Act
            var result = await this.service.GetProducerBillingInstructionsAsync(1, request, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.HasCount(1, result.Records);
            Assert.AreEqual(5, result.Records[0].ProducerId);
        }

        [TestMethod]
        public async Task GetProducerBillingInstructions_FiltersByStatus()
        {
            // Arrange
            await SeedAsync(
                CreateRun(1),
                CreateBillingInstruction(producerId: 7, runId: 1, suggestedInstruction: "Initial", suggestedInvoiceAmount: 100, acceptRejectStatus: "Accepted"),
                CreateBillingInstruction(producerId: 8, runId: 1, suggestedInstruction: "Delta", suggestedInvoiceAmount: 200, acceptRejectStatus: "Pending"));
            var request = CreateRequest(new ProducerBillingInstructionsSearchQueryDto { Status = new List<string> { "Accepted" } });

            // Act
            var result = await this.service.GetProducerBillingInstructionsAsync(1, request, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.HasCount(1, result.Records);
            Assert.AreEqual("Accepted", result.Records[0].BillingInstructionAcceptReject);
        }

        [TestMethod]
        public async Task GetProducerBillingInstructions_FiltersByBillingInstruction_IncludingNoAction()
        {
            // Arrange
            await SeedAsync(
                CreateRun(1),
                CreateBillingInstruction(producerId: 9, runId: 1, suggestedInstruction: "-", suggestedInvoiceAmount: 100, acceptRejectStatus: "Accepted"),
                CreateBillingInstruction(producerId: 10, runId: 1, suggestedInstruction: "Initial", suggestedInvoiceAmount: 200, acceptRejectStatus: "Pending"));
            var request = CreateRequest(new ProducerBillingInstructionsSearchQueryDto { BillingInstruction = new List<string> { "Noaction" } });

            // Act
            var result = await this.service.GetProducerBillingInstructionsAsync(1, request, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.HasCount(1, result.Records);
            Assert.AreEqual("-", result.Records[0].SuggestedBillingInstruction);
        }

        private async Task SeedAsync(params object[] entities)
        {
            this.dbContext.AddRange(entities);
            await this.dbContext.SaveChangesAsync(CancellationToken.None);
        }

        private Task<ProducerResultFileSuggestedBillingInstruction> GetBillingInstructionAsync(int producerId, int runId) =>
            this.dbContext.ProducerResultFileSuggestedBillingInstruction
                .SingleAsync(x => x.ProducerId == producerId && x.CalculatorRunId == runId);

        private static CalculatorRun CreateRun(
            int id,
            RunClassification classification = RunClassification.INITIAL_RUN,
            string? name = null) => new()
            {
                Id = id,
                Name = name ?? $"Run {id}",
                RelativeYear = new RelativeYear(2024),
                CalculatorRunClassificationId = (int)classification,
            };

        private static ProducerResultFileSuggestedBillingInstruction CreateBillingInstruction(
            int producerId,
            int runId,
            string suggestedInstruction,
            decimal? suggestedInvoiceAmount = null,
            string? acceptRejectStatus = null,
            decimal? currentYearInvoiceTotalToDate = null) => new()
            {
                ProducerId = producerId,
                CalculatorRunId = runId,
                SuggestedBillingInstruction = suggestedInstruction,
                SuggestedInvoiceAmount = suggestedInvoiceAmount,
                BillingInstructionAcceptReject = acceptRejectStatus,
                CurrentYearInvoiceTotalToDate = currentYearInvoiceTotalToDate,
            };

        private static ProducerBillingInstructionsRequestDto CreateRequest(
            ProducerBillingInstructionsSearchQueryDto? searchQuery = null) => new()
            {
                PageNumber = 1,
                PageSize = 10,
                SearchQuery = searchQuery,
            };

        private static ProduceBillingInstuctionRequestDto AcceptRequest(params int[] organisationIds) => new()
        {
            Status = BillingStatus.Accepted.ToString(),
            OrganisationIds = organisationIds,
        };

        private static ProduceBillingInstuctionRequestDto RejectRequest(string reason, params int[] organisationIds) => new()
        {
            Status = BillingStatus.Rejected.ToString(),
            OrganisationIds = organisationIds,
            ReasonForRejection = reason,
        };
    }
}
