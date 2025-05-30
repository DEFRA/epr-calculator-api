using System.Net;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Services;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests.Services
{
    [TestClass]
    public class BillingFileServiceTests : InMemoryApplicationDbContext
    {
        private readonly BillingFileService billingFileServiceUnderTest;

        private readonly Mock<IStorageService> mockIStorageService;

        public BillingFileServiceTests()
        {
            this.mockIStorageService = new Mock<IStorageService>();

            this.billingFileServiceUnderTest = new BillingFileService(
                this.DbContext,
                this.mockIStorageService.Object);
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
        public async Task ProducerBillingInstructionsAsync_ShouldReturnUnprocessableContent_WhenCalculatorRunNotFound()
        {
            // Arrange
            var requestDto = new ProduceBillingInstuctionRequestDto
            {
                Status = BillingStatus.Accepted.ToString(),
                OrganisationIds = new List<int> { 1, 2, 3 },
            };

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
                OrganisationIds = new List<int> { 2, 3 },
            };

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
                OrganisationIds = new List<int> { 1, 2 },
            };

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
                OrganisationIds = new List<int> { 1, 2 },
                ReasonForRejection = "Test",
            };

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
    }
}
