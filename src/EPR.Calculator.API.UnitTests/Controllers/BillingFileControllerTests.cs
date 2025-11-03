using System.Net;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Services.Abstractions;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class BillingFileControllerTests
    {
        private readonly BillingFileController billingFileControllerUnderTest;

        private readonly Mock<IBillingFileService> billingFileServiceMock;

        private readonly Mock<IStorageService> storageServiceMock;

        private readonly ApplicationDBContext context;

        public BillingFileControllerTests()
        {
            billingFileServiceMock = new Mock<IBillingFileService>();
            storageServiceMock = new Mock<IStorageService>();

            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            context = new ApplicationDBContext(dbContextOptions);
            context.Database.EnsureCreated();

            billingFileControllerUnderTest = new BillingFileController(billingFileServiceMock.Object, storageServiceMock.Object, context);
        }

        [TestMethod]
        [DataRow(-1)]
        [DataRow(0)]
        public async Task GenerateBillingFileMethod_ShouldReturnNotFound_AndNotMakeCallToService_WhenCalculatorRunIdIsInValid(
            int calculatorRunId)
        {
            // Arrange
            GenerateBillingFileRequestDto generateBillingFileRequestDto = new()
            {
                CalculatorRunId = calculatorRunId,
            };
            using CancellationTokenSource cancellationTokenSource = new();

            // Act
#pragma warning disable CS0618 // 'Type or member is obsolete'
            IActionResult result = await billingFileControllerUnderTest.GenerateBillingFile(
                generateBillingFileRequestDto,
                cancellationTokenSource.Token);
#pragma warning restore CS0618

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Should().BeOfType<NotFoundObjectResult>();
                var notFoundResult = result as NotFoundObjectResult;
                notFoundResult.Should().NotBeNull();
                notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
                notFoundResult.Value.Should().NotBeNull();

                // Verify
                billingFileServiceMock.Verify(
                    x => x.GenerateBillingFileAsync(
                        generateBillingFileRequestDto,
                        cancellationTokenSource.Token),
                    Times.Never());
            }
        }

        [TestMethod]
        [DataRow(HttpStatusCode.Accepted, "Request accepted for processing.")]
        [DataRow(HttpStatusCode.NotFound, "The requested resource could not be found.")]
        [DataRow(HttpStatusCode.UnprocessableContent, "The billing file generation alreeady requested for requested run id.")]
        public async Task GenerateBillingFileMethod_ShouldReturnObjectResult_AndMakeCallToService_WhenCalculatorRunIdIsValid(
            HttpStatusCode httpStatusCode,
            string message)
        {
            // Arrange
            GenerateBillingFileRequestDto generateBillingFileRequestDto = new()
            {
                CalculatorRunId = 101,
            };
            using CancellationTokenSource cancellationTokenSource = new();
            ServiceProcessResponseDto serviceProcessResponseDto = new()
            {
                StatusCode = httpStatusCode,
                Message = message,
            };

            // Setup
            billingFileServiceMock.Setup(
                    x => x.GenerateBillingFileAsync(
                        generateBillingFileRequestDto,
                        cancellationTokenSource.Token))
                .ReturnsAsync(serviceProcessResponseDto);

            // Act
#pragma warning disable CS0618 // 'Type or member is obsolete'
            IActionResult result = await billingFileControllerUnderTest.GenerateBillingFile(
                generateBillingFileRequestDto,
                cancellationTokenSource.Token);
#pragma warning restore CS0618

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Should().BeOfType<ObjectResult>();
                var objectResult = result as ObjectResult;
                objectResult.Should().NotBeNull();
                objectResult.StatusCode.Should().Be((int)httpStatusCode);
                objectResult.Value.Should().NotBeNull();
                objectResult.Value.Should().Be(message);
            }
        }

        [TestMethod]
        public async Task ProducerBillingInstructions_ReturnsBadRequest_WhenRunIdIsInvalid()
        {
            // Arrange
            int invalidRunId = 0;

            // Act
            var result = await billingFileControllerUnderTest.ProducerBillingInstructions(invalidRunId) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        [TestMethod]
        public async Task ProducerBillingInstructions_ReturnsNotFound_WhenResponseIsNull()
        {
            // Arrange
            int validRunId = 10;
            billingFileServiceMock
                .Setup(s => s.GetProducersInstructionResponseAsync(validRunId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProducersInstructionResponse());

            // Act
            var result = await billingFileControllerUnderTest.ProducerBillingInstructions(validRunId) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [TestMethod]
        public async Task ProducerBillingInstructions_ReturnsOk_WithValidResponse()
        {
            // Arrange
            int validRunId = 20;
            var expectedResponse = new ProducersInstructionResponse
            {
                ProducersInstructionDetails = new List<ProducersInstructionDetail>(),
                ProducersInstructionSummary = new ProducersInstructionSummary(),
            };

            billingFileServiceMock
                .Setup(s => s.GetProducersInstructionResponseAsync(validRunId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await billingFileControllerUnderTest.ProducerBillingInstructions(validRunId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(expectedResponse, okResult.Value);
        }

        [TestMethod]
        public async Task DownloadBillingFile_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            billingFileControllerUnderTest.ModelState.AddModelError("key", "error");

            // Act
            var result = await billingFileControllerUnderTest.DownloadCsvBillingFile(1);

            // Assert
            result.Should().BeOfType<BadRequest<IEnumerable<ModelError>>>();
        }

        [TestMethod]
        public async Task DownloadBillingFile_ReturnsNotFound_WhenNoMetadataFound()
        {
            // Arrange
            int runId = 9999; // Use a runId that does not exist in the in-memory DB

            // Act
            var result = await billingFileControllerUnderTest.DownloadCsvBillingFile(runId);

            // Assert
            result.Should().BeOfType<NotFound<string>>();
        }

        [TestMethod]
        public async Task DownloadBillingFile_ReturnsProblem_WhenStorageThrows()
        {
            // Arrange
            int runId = 123;
            var billingMeta = new CalculatorRunBillingFileMetadata()
            {
                CalculatorRunId = runId,
                BillingCsvFileName = "csvfile.json",
                BillingJsonFileName = "jsonfile.json",
                BillingFileCreatedBy = "user",
                BillingFileCreatedDate = DateTime.UtcNow,
            };

            var csvMeta = new CalculatorRunCsvFileMetadata()
            {
                FileName = "csvfile.json",
                BlobUri = "C:\\dev\\file.json",
                CalculatorRunId = runId,
            };
            context.CalculatorRunBillingFileMetadata.Add(billingMeta);
            context.CalculatorRunCsvFileMetadata.Add(csvMeta);
            context.SaveChanges();

            storageServiceMock
                .Setup(x => x.DownloadFile("csvfile.json", "C:\\dev\\file.json"))
                .ThrowsAsync(new Exception("fail"));

            // Act
            var result = await billingFileControllerUnderTest.DownloadCsvBillingFile(runId);

            // Assert
            result.Should().BeOfType<ProblemHttpResult>();

            // Tidy Up
            context.CalculatorRunBillingFileMetadata.Remove(billingMeta);
            context.CalculatorRunCsvFileMetadata.Remove(csvMeta);
            context.SaveChanges();
        }

        [TestMethod]
        public async Task DownloadBillingFile_ReturnsFileResult_WhenSuccess()
        {
            // Arrange
            int runId = 456;
            var billingMeta = new CalculatorRunBillingFileMetadata
            {
                CalculatorRunId = runId,
                BillingCsvFileName = "csvfile.json",
                BillingJsonFileName = "file2.json",
                BillingFileCreatedBy = "user",
                BillingFileCreatedDate = DateTime.UtcNow,
            };
            var csvMeta = new CalculatorRunCsvFileMetadata
            {
                FileName = "csvfile.json",
                BlobUri = "C:\\dev\\csvfile.json",
                CalculatorRunId = runId,
            };
            context.CalculatorRunBillingFileMetadata.Add(billingMeta);
            context.CalculatorRunCsvFileMetadata.Add(csvMeta);
            context.SaveChanges();

            var expectedResult = Mock.Of<IResult>();
            storageServiceMock
                .Setup(x => x.DownloadFile("csvfile.json", "C:\\dev\\csvfile.json"))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await billingFileControllerUnderTest.DownloadCsvBillingFile(runId);

            // Assert
            result.Should().BeSameAs(expectedResult);

            // Tidy Up
            context.CalculatorRunBillingFileMetadata.Remove(billingMeta);
            context.CalculatorRunCsvFileMetadata.Remove(csvMeta);
            context.SaveChanges();
        }
    }
}
