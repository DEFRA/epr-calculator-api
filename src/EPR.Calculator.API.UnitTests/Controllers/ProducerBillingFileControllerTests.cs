using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class ProducerBillingFileControllerTests
    {
        private readonly ProducerBillingFileController producerFileControllerTest;

        private readonly Mock<IBillingFileService> billingFileServiceMock;

        private readonly Mock<IBackgroundTaskQueue> backgroundTaskQueueMock;


        public ProducerBillingFileControllerTests()
        {
            this.billingFileServiceMock = new Mock<IBillingFileService>();
            this.backgroundTaskQueueMock = new Mock<IBackgroundTaskQueue>();
            this.producerFileControllerTest = new ProducerBillingFileController(this.billingFileServiceMock.Object, this.backgroundTaskQueueMock.Object);

            // Mock User Claims
            // Set up authorisation.
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);
            var context = new DefaultHttpContext { User = principal };
            this.producerFileControllerTest.ControllerContext = new ControllerContext { HttpContext = context };
        }

        [TestMethod]
        [DataRow(StatusCodes.Status200OK, "OK")]
        [DataRow(StatusCodes.Status400BadRequest, "Bad Request.")]
        [DataRow(StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity")]
        public async Task ProducerBillingInstructions_ShouldReturn202Accepted_WhenUpdateSuccessful(
            int httpStatusCode,
            string message)
        {
            // Arrange
            this.billingFileServiceMock.Setup(s => s.StartGeneratingBillingFileAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ServiceProcessResponseDto { StatusCode = (HttpStatusCode)httpStatusCode });

            this.backgroundTaskQueueMock.Setup(s => s.QueueAsync(It.IsAny<BackgroundServiceMessage>(), It.IsAny<CancellationToken>()));

            // Act
            var result = await this.producerFileControllerTest.ProducerBillingInstructions(1, CancellationToken.None) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(httpStatusCode, result.StatusCode);
        }
    }
}
