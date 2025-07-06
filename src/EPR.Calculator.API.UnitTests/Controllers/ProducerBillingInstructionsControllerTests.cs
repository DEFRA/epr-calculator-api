using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class ProducerBillingInstructionsControllerTests
    {
        private readonly Mock<IBillingFileService> mockBillingFileService;
        private readonly ProducerBillingInstructionsController controller;

        public ProducerBillingInstructionsControllerTests()
        {
            this.mockBillingFileService = new Mock<IBillingFileService>();
            this.controller = new ProducerBillingInstructionsController(this.mockBillingFileService.Object);
        }

        [TestMethod]
        public async Task ProducerBillingInstructions_ReturnsBadRequest_WhenValidationFails()
        {
            // Arrange
            var runId = 1;
            var requestDto = new ProducerBillingInstructionsRequestDto { PageNumber = 0 }; // Invalid
            this.controller.ModelState.AddModelError("PageNumber", "PageNumber must be 1 or greater.");

            // Act
            var result = await this.controller.ProducerBillingInstructions(runId, requestDto, CancellationToken.None);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, objectResult.StatusCode);
            Assert.IsTrue(objectResult.Value is IEnumerable<ModelError> errors && errors.Any());
        }

        [TestMethod]
        public async Task ProducerBillingInstructions_ReturnsNotFound_WhenRunIdNotFound()
        {
            // Arrange
            var runId = 9999;
            var requestDto = new ProducerBillingInstructionsRequestDto { PageNumber = 1, PageSize = 10 };
            this.mockBillingFileService
                .Setup(x => x.GetProducerBillingInstructionsAsync(runId, requestDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProducerBillingInstructionsResponseDto?)null);

            // Act
            var result = await this.controller.ProducerBillingInstructions(runId, requestDto, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task ProducerBillingInstructions_ReturnsOk_WhenRecordsFound()
        {
            // Arrange
            var runId = 1;
            var requestDto = new ProducerBillingInstructionsRequestDto { PageNumber = 1, PageSize = 10 };
            var responseDto = new ProducerBillingInstructionsResponseDto
            {
                Records = new List<ProducerBillingInstructionsDto>
                {
                    new ProducerBillingInstructionsDto { ProducerId = 1, ProducerName = "Test Producer" },
                },
                TotalRecords = 1,
                RunName = "Test Run",
                PageNumber = 1,
                PageSize = 10,
                CalculatorRunId = runId,
            };
            this.mockBillingFileService
                .Setup(x => x.GetProducerBillingInstructionsAsync(runId, requestDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await this.controller.ProducerBillingInstructions(runId, requestDto, CancellationToken.None);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            var returnedDto = okResult.Value as ProducerBillingInstructionsResponseDto;
            Assert.IsNotNull(returnedDto);
            Assert.AreEqual(1, returnedDto.Records.Count);
            Assert.AreEqual("Test Run", returnedDto.RunName);
        }

        [TestMethod]
        public async Task ProducerBillingInstructions_ReturnsOk_WhenNoRecordsButRunFound()
        {
            // Arrange
            var runId = 1;
            var requestDto = new ProducerBillingInstructionsRequestDto { PageNumber = 1, PageSize = 10 };
            var responseDto = new ProducerBillingInstructionsResponseDto
            {
                Records = new List<ProducerBillingInstructionsDto>(),
                TotalRecords = 0,
                RunName = "Test Run",
                PageNumber = 1,
                PageSize = 10,
                CalculatorRunId = runId,
            };

            this.mockBillingFileService
                .Setup(x => x.GetProducerBillingInstructionsAsync(runId, requestDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await this.controller.ProducerBillingInstructions(runId, requestDto, CancellationToken.None);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            var returnedDto = okResult.Value as ProducerBillingInstructionsResponseDto;
            Assert.IsNotNull(returnedDto);
            Assert.AreEqual(0, returnedDto.Records.Count);
            Assert.AreEqual("Test Run", returnedDto.RunName);
        }
    }
}