using System.Net;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Services.Abstractions;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Amqp.Transaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class BillingFileControllerTests
    {
        private readonly BillingFileController billingFileControllerUnderTest;

        private readonly Mock<IBillingFileService> billingFileServiceMock;

        public BillingFileControllerTests()
        {
            this.billingFileServiceMock = new Mock<IBillingFileService>();
            this.billingFileControllerUnderTest = new BillingFileController(this.billingFileServiceMock.Object);
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
            IActionResult result = await this.billingFileControllerUnderTest.GenerateBillingFile(
                generateBillingFileRequestDto,
                cancellationTokenSource.Token);

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
                this.billingFileServiceMock.Verify(
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
            this.billingFileServiceMock.Setup(
                    x => x.GenerateBillingFileAsync(
                        generateBillingFileRequestDto,
                        cancellationTokenSource.Token))
                .ReturnsAsync(serviceProcessResponseDto);

            // Act
            IActionResult result = await this.billingFileControllerUnderTest.GenerateBillingFile(
                generateBillingFileRequestDto,
                cancellationTokenSource.Token);

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
            var result = await billingFileControllerUnderTest.ProducerBillingInstructions(invalidRunId);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task ProducerBillingInstructions_ReturnsNotFound_WhenResponseIsNull()
        {
            // Arrange
            int validRunId = 10;
            billingFileServiceMock
                .Setup(s => s.GetProducersInstructionResponseAsync(validRunId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProducersInstructionResponse?)null);

            // Act
            var result = await billingFileControllerUnderTest.ProducerBillingInstructions(validRunId);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task ProducerBillingInstructions_ReturnsOk_WithValidResponse()
        {
            // Arrange
            int validRunId = 20;
            var expectedResponse = new ProducersInstructionResponse
            {
                ProducersInstructionDetails = new List<ProducersInstructionDetail>(),
                ProducersInstructionSummary = new ProducersInstructionSummary()
            };

            billingFileServiceMock
                .Setup(s => s.GetProducersInstructionResponseAsync(validRunId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await billingFileControllerUnderTest.ProducerBillingInstructions(validRunId);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(expectedResponse, okResult.Value);
        }
    }
}
