using System.Net;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Exceptions;
using EPR.Calculator.API.Services;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests.Services
{
    [TestClass]
    public class BillingFileServiceTests : InMemoryApplicationDbContext
    {
        private readonly BillingFileService billingFileServiceUnderTest;

        private readonly Mock<IStorageService> mockIStorageService;

        private readonly Mock<IBlobStorageService2> mockBlobStorageService2;

        private readonly Mock<IConfiguration> mockConfiguration;

        public BillingFileServiceTests()
        {
            this.mockIStorageService = new Mock<IStorageService>();
            this.mockBlobStorageService2 = new Mock<IBlobStorageService2>();
            this.mockConfiguration = new Mock<IConfiguration>();

            this.billingFileServiceUnderTest = new BillingFileService(
                this.DbContext,
                this.mockIStorageService.Object,
                this.mockBlobStorageService2.Object,
                this.mockConfiguration.Object);
        }

        [TestMethod]
        public async Task GenerateBillingFileAsyncMethod_ShouldReturnNotFound_WhenCalculatorRunDoesNotExist()
        {
            // Arrange
            GenerateBillingFileRequestDto generateBillingFileRequestDto = new()
            {
                CalculatorRunId = int.MaxValue,
            };
            using CancellationTokenSource cancellationTokenSource = new();

            // Act
            ServiceProcessResponseDto result = await this.billingFileServiceUnderTest.GenerateBillingFileAsync(
                generateBillingFileRequestDto,
                cancellationTokenSource.Token);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
                result.Message.Should().Be(CommonResources.ResourceNotFoundErrorMessage);

                // Verify
                this.mockIStorageService.Verify(
                    x => x.IsBlobExistsAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        cancellationTokenSource.Token),
                    Times.Never());
            }
        }

        [TestMethod]
        public async Task GenerateBillingFileAsyncMethod_ShouldReturnUnprocessableContent_WhenGenerateBillingAlreadyRequested()
        {
            // Arrange
            CalculatorRun calculatorRun = this.DbContext.CalculatorRuns.First();
            calculatorRun.CalculatorRunClassificationId = (int)RunClassification.INITIAL_RUN;
            calculatorRun.HasBillingFileGenerated = true;
            await this.DbContext.SaveChangesAsync();

            GenerateBillingFileRequestDto generateBillingFileRequestDto = new()
            {
                CalculatorRunId = calculatorRun.Id,
            };
            using CancellationTokenSource cancellationTokenSource = new();

            // Act
            ServiceProcessResponseDto result = await this.billingFileServiceUnderTest.GenerateBillingFileAsync(
                generateBillingFileRequestDto,
                cancellationTokenSource.Token);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.StatusCode.Should().Be(HttpStatusCode.UnprocessableContent);
                result.Message.Should().Be(string.Format(CommonResources.GenerateBillingFileAlreadyRequest, generateBillingFileRequestDto.CalculatorRunId));

                // Verify
                this.mockIStorageService.Verify(
                    x => x.IsBlobExistsAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        cancellationTokenSource.Token),
                    Times.Never());
            }
        }

        [TestMethod]
        public async Task GenerateBillingFileAsyncMethod_ShouldReturnUnprocessableContent_WhenItsNotInitialRun()
        {
            // Arrange
            CalculatorRun calculatorRun = this.DbContext.CalculatorRuns.First();
            calculatorRun.CalculatorRunClassificationId = (int)RunClassification.UNCLASSIFIED;
            calculatorRun.HasBillingFileGenerated = false;
            await this.DbContext.SaveChangesAsync();

            GenerateBillingFileRequestDto generateBillingFileRequestDto = new()
            {
                CalculatorRunId = calculatorRun.Id,
            };
            using CancellationTokenSource cancellationTokenSource = new();

            // Act
            ServiceProcessResponseDto result = await this.billingFileServiceUnderTest.GenerateBillingFileAsync(
                generateBillingFileRequestDto,
                cancellationTokenSource.Token);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.StatusCode.Should().Be(HttpStatusCode.UnprocessableContent);
                result.Message.Should().Be(string.Format(CommonResources.NotAValidClassificationStatus, generateBillingFileRequestDto.CalculatorRunId));

                // Verify
                this.mockIStorageService.Verify(
                    x => x.IsBlobExistsAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        cancellationTokenSource.Token),
                    Times.Never());
            }
        }

        [TestMethod]
        public async Task GenerateBillingFileAsyncMethod_ShouldReturnUnprocessableContent_WhenCsvFileMetadataRecordNotFound()
        {
            // Arrange
            CalculatorRun calculatorRun = this.DbContext.CalculatorRuns.Last();
            calculatorRun.CalculatorRunClassificationId = (int)RunClassification.INITIAL_RUN;
            calculatorRun.HasBillingFileGenerated = false;
            await this.DbContext.SaveChangesAsync();

            GenerateBillingFileRequestDto generateBillingFileRequestDto = new()
            {
                CalculatorRunId = calculatorRun.Id,
            };
            using CancellationTokenSource cancellationTokenSource = new();

            // Act
            ServiceProcessResponseDto result = await this.billingFileServiceUnderTest.GenerateBillingFileAsync(
                generateBillingFileRequestDto,
                cancellationTokenSource.Token);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.StatusCode.Should().Be(HttpStatusCode.UnprocessableContent);
                result.Message.Should().Be(string.Format(CommonResources.CsvFileMetadataNotFoundErrorMessage, generateBillingFileRequestDto.CalculatorRunId));

                // Verify
                this.mockIStorageService.Verify(
                    x => x.IsBlobExistsAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        cancellationTokenSource.Token),
                    Times.Never());
            }
        }

        [TestMethod]
        public async Task GenerateBillingFileAsyncMethod_ShouldReturnUnprocessableContent_WhenBlobNotFound()
        {
            // Arrange
            CalculatorRun calculatorRun = this.DbContext.CalculatorRuns.First();
            calculatorRun.CalculatorRunClassificationId = (int)RunClassification.INITIAL_RUN;
            calculatorRun.HasBillingFileGenerated = false;
            var fileName = "1-Calc RunName_Results File_20241111.csv";
            var blobUri = $"https://example.com/{fileName}";

            if (!this.DbContext.CalculatorRunCsvFileMetadata.Any())
            {
                this.DbContext.CalculatorRunCsvFileMetadata.Add(new CalculatorRunCsvFileMetadata
                {
                    CalculatorRunId = calculatorRun.Id,
                    FileName = fileName,
                    BlobUri = blobUri,
                });
            }

            await this.DbContext.SaveChangesAsync();

            GenerateBillingFileRequestDto generateBillingFileRequestDto = new()
            {
                CalculatorRunId = calculatorRun.Id,
            };
            using CancellationTokenSource cancellationTokenSource = new();

            // Setup
            this.mockIStorageService.Setup(
                   x => x.IsBlobExistsAsync(
                       fileName,
                       blobUri,
                       cancellationTokenSource.Token)).ReturnsAsync(false);

            // Act
            ServiceProcessResponseDto result = await this.billingFileServiceUnderTest.GenerateBillingFileAsync(
                generateBillingFileRequestDto,
                cancellationTokenSource.Token);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.StatusCode.Should().Be(HttpStatusCode.UnprocessableContent);
                result.Message.Should().Be(string.Format(CommonResources.BlobNotFoundErrorMessage, generateBillingFileRequestDto.CalculatorRunId));

                // Verify
                this.mockIStorageService.Verify(
                    x => x.IsBlobExistsAsync(
                        fileName,
                        blobUri,
                        cancellationTokenSource.Token),
                    Times.Once());
            }
        }

        [TestMethod]
        public async Task GenerateBillingFileAsyncMethod_ShouldReturnAccepted_AndUpdateHasBillingFileGeneratedToTrue_WhenRequestIsValid()
        {
            // Arrange
            CalculatorRun calculatorRun = this.DbContext.CalculatorRuns.First();
            calculatorRun.CalculatorRunClassificationId = (int)RunClassification.INITIAL_RUN;
            calculatorRun.HasBillingFileGenerated = false;

            var fileName = "1-Calc RunName_Results File_20241111.csv";
            var blobUri = $"https://example.com/{fileName}";

            if (!this.DbContext.CalculatorRunCsvFileMetadata.Any())
            {
                this.DbContext.CalculatorRunCsvFileMetadata.Add(new CalculatorRunCsvFileMetadata
                {
                    CalculatorRunId = calculatorRun.Id,
                    FileName = fileName,
                    BlobUri = blobUri,
                });
            }

            await this.DbContext.SaveChangesAsync();
            GenerateBillingFileRequestDto generateBillingFileRequestDto = new()
            {
                CalculatorRunId = calculatorRun.Id,
            };
            using CancellationTokenSource cancellationTokenSource = new();

            // Setup
            this.mockIStorageService.Setup(
                   x => x.IsBlobExistsAsync(
                       fileName,
                       blobUri,
                       cancellationTokenSource.Token)).ReturnsAsync(true);

            // Act
            ServiceProcessResponseDto result = await this.billingFileServiceUnderTest.GenerateBillingFileAsync(
                generateBillingFileRequestDto,
                cancellationTokenSource.Token);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.StatusCode.Should().Be(HttpStatusCode.Accepted);
                result.Message.Should().Be(CommonResources.RequestAcceptedMessage);
                calculatorRun = await this.DbContext.CalculatorRuns.SingleAsync(x => x.Id == generateBillingFileRequestDto.CalculatorRunId, cancellationTokenSource.Token);
                calculatorRun.HasBillingFileGenerated.Should().BeTrue();

                // Verify
                this.mockIStorageService.Verify(
                    x => x.IsBlobExistsAsync(
                        fileName,
                        blobUri,
                        cancellationTokenSource.Token),
                    Times.Once());
            }
        }

        [TestMethod]
        public async Task GetProducersInstructionResponseAsync_ThrowsKeyNotFound_WhenRunNotFound()
        {
            // Arrange
            var runId = 999;

            // Act + Assert
            await Assert.ThrowsExceptionAsync<KeyNotFoundException>(() =>
                billingFileServiceUnderTest.GetProducersInstructionResponseAsync(runId, CancellationToken.None));
        }

        [TestMethod]
        public async Task GetProducersInstructionResponseAsync_ThrowsUnprocessableEntity_WhenInvalidClassification()
        {
            // Arrange
            var runId = 2;

            // Act + Assert
            await Assert.ThrowsExceptionAsync<UnprocessableEntityException>(() =>
                billingFileServiceUnderTest.GetProducersInstructionResponseAsync(runId, CancellationToken.None));
        }

        [TestMethod]
        public async Task GetProducersInstructionResponseAsync_ReturnsNull_WhenNoInstructions()
        {
            // Arrange
            var runId = 3;

            // Act
            var result = await billingFileServiceUnderTest.GetProducersInstructionResponseAsync(runId, CancellationToken.None);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetProducersInstructionResponseAsync_ReturnsResponse_WhenValid()
        {
            // Arrange
            var runId = 4;

            this.DbContext.ProducerDetail.Add(new ProducerDetail
            {
                ProducerId = 101,
                CalculatorRunId = runId,
                ProducerName = "Acme Co",
                TradingName = "Acme Trading",
            });

            this.DbContext.ProducerResultFileSuggestedBillingInstruction.Add(new ProducerResultFileSuggestedBillingInstruction
            {
                ProducerId = 101,
                CalculatorRunId = runId,
                SuggestedBillingInstruction = "Invoice",
                SuggestedInvoiceAmount = 123.45m,
                BillingInstructionAcceptReject = "Accepted",
            });

            await this.DbContext.SaveChangesAsync();

            // Act
            var result = await this.billingFileServiceUnderTest.GetProducersInstructionResponseAsync(runId, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.ProducersInstructionDetails.Count);
            Assert.AreEqual("Accepted", result.ProducersInstructionDetails.First().Status.ToString());

            this.DbContext.ProducerResultFileSuggestedBillingInstruction.RemoveRange(this.DbContext.ProducerResultFileSuggestedBillingInstruction);
            await this.DbContext.SaveChangesAsync();
        }

        [TestMethod]
        public async Task ProducerBillingInstructionsAsync_ShouldReturnUnprocessableContent_WhenCalculatorRunNotFound()
        {
            // Arrange
            var requestDto = new ProduceBillingInstuctionRequestDto
            {
                Status = BillingStatus.Accepted.ToString(),
                OrganisationIds = new List<int> { 1, 2, 3 },
            };
            CalculatorRun calculatorRun = this.DbContext.CalculatorRuns.First();
            calculatorRun.CalculatorRunClassificationId = (int)RunClassification.INITIAL_RUN;
            await this.DbContext.SaveChangesAsync();

            // Act
            var result = await this.billingFileServiceUnderTest.UpdateProducerBillingInstructionsAsync(100, "TestUser", requestDto, CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.UnprocessableContent, result.StatusCode);
            Assert.AreEqual(ErrorMessages.InvalidRunId, result.Message);
        }

        [TestMethod]
        public async Task ProducerBillingInstructionsAsync_ShouldReturnUnprocessableContent_WhenRunStatusIsInvalid()
        {
            // Arrange
            var requestDto = new ProduceBillingInstuctionRequestDto
            {
                Status = BillingStatus.Accepted.ToString(),
                OrganisationIds = new List<int> { 1, 2, 3 },
            };
            CalculatorRun calculatorRun = this.DbContext.CalculatorRuns.First();
            calculatorRun.CalculatorRunClassificationId = (int)RunClassification.UNCLASSIFIED;
            await this.DbContext.SaveChangesAsync();

            // Act
            var result = await this.billingFileServiceUnderTest.UpdateProducerBillingInstructionsAsync(2, "TestUser", requestDto, CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.UnprocessableContent, result.StatusCode);
            Assert.AreEqual(ErrorMessages.InvalidRunId, result.Message);
        }

        [TestMethod]
        public async Task ProducerBillingInstructionsAsync_ShouldReturnUnprocessableContent_WhenOrganisationIdIsInvalid()
        {
            // Arrange
            var requestDto = new ProduceBillingInstuctionRequestDto
            {
                Status = BillingStatus.Accepted.ToString(),
                OrganisationIds = new List<int> { 1, 2 },
            };
            CalculatorRun calculatorRun = this.DbContext.CalculatorRuns.First();
            calculatorRun.CalculatorRunClassificationId = (int)RunClassification.INITIAL_RUN;
            await this.DbContext.SaveChangesAsync();

            // Act
            var result = await this.billingFileServiceUnderTest.UpdateProducerBillingInstructionsAsync(1, "TestUser", requestDto, CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.UnprocessableContent, result.StatusCode);
            Assert.AreEqual(ErrorMessages.InvalidOrganisationId, result.Message);
        }

        [TestMethod]
        public async Task ProducerBillingInstructionsAsync_ShouldReturnOk_WhenAcceptedUpdateSuccessful()
        {
            // Arrange
            var requestDto = new ProduceBillingInstuctionRequestDto
            {
                Status = BillingStatus.Accepted.ToString(),
                OrganisationIds = new List<int> { 1 },
            };
            CalculatorRun calculatorRun = this.DbContext.CalculatorRuns.First();
            calculatorRun.CalculatorRunClassificationId = (int)RunClassification.INITIAL_RUN;
            await this.DbContext.SaveChangesAsync();

            // Act
            var result = await this.billingFileServiceUnderTest.UpdateProducerBillingInstructionsAsync(1, "TestUser", requestDto, CancellationToken.None);

            // Assert
            var updatedRecord = this.DbContext.ProducerResultFileSuggestedBillingInstruction.FirstOrDefault();
            Assert.AreEqual(updatedRecord?.BillingInstructionAcceptReject, BillingStatus.Accepted.ToString());
            Assert.IsNotNull(updatedRecord?.LastModifiedAcceptReject);
            Assert.IsNotNull(updatedRecord?.LastModifiedAcceptRejectBy);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [TestMethod]
        public async Task ProducerBillingInstructionsAsync_ShouldReturnOk_WhenRejectionUpdateSuccessful()
        {
            // Arrange
            var requestDto = new ProduceBillingInstuctionRequestDto
            {
                Status = BillingStatus.Rejected.ToString(),
                OrganisationIds = new List<int> { 1 },
                ReasonForRejection = "Test",
            };
            CalculatorRun calculatorRun = this.DbContext.CalculatorRuns.First();
            calculatorRun.CalculatorRunClassificationId = (int)RunClassification.INITIAL_RUN;
            await this.DbContext.SaveChangesAsync();

            // Act
            var result = await this.billingFileServiceUnderTest.UpdateProducerBillingInstructionsAsync(1, "TestUser", requestDto, CancellationToken.None);

            // Assert
            var updatedRecord = this.DbContext.ProducerResultFileSuggestedBillingInstruction.FirstOrDefault();
            Assert.AreEqual(updatedRecord?.BillingInstructionAcceptReject, BillingStatus.Rejected.ToString());
            Assert.AreEqual(updatedRecord?.ReasonForRejection, requestDto.ReasonForRejection);
            Assert.IsNotNull(updatedRecord?.LastModifiedAcceptReject);
            Assert.IsNotNull(updatedRecord?.LastModifiedAcceptRejectBy);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [TestMethod]
        public async Task ProducerBillingInstructionsAcceptAllAsync_ShouldReturnUnprocessableContent_WhenCalculatorRunNotFound()
        {
            // Act
            var result = await this.billingFileServiceUnderTest.UpdateProducerBillingInstructionsAcceptAllAsync(100, "TestUser", CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.UnprocessableContent, result.StatusCode);
            Assert.AreEqual(ErrorMessages.InvalidRunId, result.Message);
        }

        [TestMethod]
        public async Task ProducerBillingInstructionsAcceptAllAsync_ShouldReturnUnprocessableContent_WhenRunStatusIsInvalid()
        {
            // Act
            var result = await this.billingFileServiceUnderTest.UpdateProducerBillingInstructionsAcceptAllAsync(2, "TestUser", CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.UnprocessableContent, result.StatusCode);
            Assert.AreEqual(ErrorMessages.InvalidRunStatusForAcceptAll, result.Message);
        }

        [TestMethod]
        public async Task ProducerBillingInstructionsAcceptAllAsync_ShouldReturnOk_WhenAcceptedUpdateSuccessful()
        {
            // Act
            var result = await this.billingFileServiceUnderTest.UpdateProducerBillingInstructionsAcceptAllAsync(1, "TestUser", CancellationToken.None);

            // Assert
            var updatedRecord = this.DbContext.ProducerResultFileSuggestedBillingInstruction.FirstOrDefault();
            Assert.AreEqual(updatedRecord?.BillingInstructionAcceptReject, BillingStatus.Accepted.ToString());
            Assert.IsNotNull(updatedRecord?.LastModifiedAcceptReject);
            Assert.IsNotNull(updatedRecord?.LastModifiedAcceptRejectBy);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        [TestMethod]
        public async Task MoveBillingJsonFile_ShouldReturnFalse_WhenMetadataNotFound()
        {
            // Arrange
            int runId = 999; // Use a runId that does not exist in the metadata table
            using var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var result = await this.billingFileServiceUnderTest.MoveBillingJsonFile(runId, cancellationTokenSource.Token);

            // Assert
            result.Should().BeFalse();
            this.mockBlobStorageService2.Verify(
                x => x.MoveBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }

        [TestMethod]
        public async Task MoveBillingJsonFile_ShouldReturnTrue_WhenMoveSucceeds()
        {
            // Arrange
            int runId = 1;
            var billingJsonFileName = "test-billing.json";
            var sourceContainer = "source-container";
            var targetContainer = "target-container";
            using var cancellationTokenSource = new CancellationTokenSource();

            // Add metadata to the in-memory DB
            this.DbContext.CalculatorRunBillingFileMetadata.Add(new CalculatorRunBillingFileMetadata
            {
                CalculatorRunId = runId,
                BillingJsonFileName = billingJsonFileName,
                BillingFileCreatedDate = DateTime.UtcNow,
                BillingFileCreatedBy = "test",
            });
            await this.DbContext.SaveChangesAsync();

            // Mock configuration for container names
            var blobStorageSettings = new Dictionary<string, string>
            {
                { "BlobStorage:BillingFileJsonContainerName", sourceContainer },
                { "BlobStorage:BillingFileJsonForFssContainerName", targetContainer },
            };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(blobStorageSettings.Select(kv => new KeyValuePair<string, string?>(kv.Key, kv.Value)))
                .Build();

            this.mockConfiguration.Setup(x => x.GetSection("BlobStorage")).Returns(config.GetSection("BlobStorage"));

            // Mock blob move
            this.mockBlobStorageService2
                .Setup(x => x.MoveBlobAsync(sourceContainer, targetContainer, billingJsonFileName))
                .ReturnsAsync(true);

            // Act
            var result = await this.billingFileServiceUnderTest.MoveBillingJsonFile(runId, cancellationTokenSource.Token);

            // Assert
            result.Should().BeTrue();
            this.mockBlobStorageService2.Verify(
                x => x.MoveBlobAsync(sourceContainer, targetContainer, billingJsonFileName),
                Times.Once());
        }

        [TestMethod]
        public async Task GetProducerBillingInstructionsAsync_ReturnsRecords_WhenValid()
        {
            // Arrange
            var runId = 9200;
            var runName = $"{runId} - potato";
            var financialYear = this.DbContext.FinancialYears.SingleOrDefault(y => y.Name == "2024-25");
            financialYear = financialYear ?? new CalculatorRunFinancialYear { Name = "2024-25" };

            this.DbContext.CalculatorRuns.Add(new CalculatorRun
            {
                Id = runId,
                Name = runName,
                Financial_Year = financialYear,
                CalculatorRunClassificationId = (int)RunClassification.INITIAL_RUN,
                HasBillingFileGenerated = true,
            });
            this.DbContext.ProducerDetail.Add(new ProducerDetail
            {
                ProducerId = 1,
                CalculatorRunId = runId,
                ProducerName = "Producer1",
            });
            this.DbContext.ProducerResultFileSuggestedBillingInstruction.Add(new ProducerResultFileSuggestedBillingInstruction
            {
                ProducerId = 1,
                CalculatorRunId = runId,
                SuggestedBillingInstruction = "Invoice",
                SuggestedInvoiceAmount = 100,
                BillingInstructionAcceptReject = "Accepted",
            });
            await this.DbContext.SaveChangesAsync();

            var requestDto = new ProducerBillingInstructionsRequestDto
            {
                PageNumber = 1,
                PageSize = 10,
            };

            // Act
            var result = await this.billingFileServiceUnderTest.GetProducerBillingInstructionsAsync(runId, requestDto, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Records.Count);
            Assert.AreEqual(1, result.TotalRecords);
            Assert.AreEqual(1, result.TotalRecordsByStatus.FirstOrDefault(x => x.Status == "Accepted")!.TotalRecords);
            Assert.AreEqual(1, result.PageNumber);
            Assert.AreEqual(10, result.PageSize);
            Assert.AreEqual(runName, result.RunName);

            this.DbContext.ProducerResultFileSuggestedBillingInstruction.RemoveRange(this.DbContext.ProducerResultFileSuggestedBillingInstruction);
            await this.DbContext.SaveChangesAsync();
        }

        [TestMethod]
        public async Task GetProducerBillingInstructionsAsync_ReturnsEmpty_WhenNoRecords()
        {
            // Arrange
            var runId = 1;
            var requestDto = new ProducerBillingInstructionsRequestDto
            {
                PageNumber = 1,
                PageSize = 10,
            };

            // Act
            var result = await this.billingFileServiceUnderTest.GetProducerBillingInstructionsAsync(runId, requestDto, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Records.Count);
            Assert.AreEqual(0, result.TotalRecords);
        }

        [TestMethod]
        public async Task GetProducerBillingInstructionsAsync_ReturnsNull_WhenRunDoesNotExist()
        {
            // Arrange
            var nonExistingRunId = 999999; // Use a runId that does not exist in the DB
            var requestDto = new ProducerBillingInstructionsRequestDto
            {
                PageNumber = 1,
                PageSize = 10,
            };

            // Act
            var result = await this.billingFileServiceUnderTest.GetProducerBillingInstructionsAsync(nonExistingRunId, requestDto, CancellationToken.None);

            // Assert
            Assert.IsNull(result);
        }
    }
}
