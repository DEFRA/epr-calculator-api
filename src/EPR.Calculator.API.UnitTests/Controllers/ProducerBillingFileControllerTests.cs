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

        private readonly Mock<IServiceBusService> serviceBusServiceMock;

        private readonly Mock<IConfiguration> configMock;

        public ProducerBillingFileControllerTests()
        {
            this.billingFileServiceMock = new Mock<IBillingFileService>();
            this.serviceBusServiceMock = new Mock<IServiceBusService>();
            this.configMock = new Mock<IConfiguration>();
            this.producerFileControllerTest = new ProducerBillingFileController(this.billingFileServiceMock.Object, this.serviceBusServiceMock.Object, this.configMock.Object);

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
            this.billingFileServiceMock.Setup(s => s.UpdateProducerBillingInstructionsAcceptAllAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ServiceProcessResponseDto { StatusCode = (HttpStatusCode)httpStatusCode });

            this.serviceBusServiceMock.Setup(s => s.SendMessage(It.IsAny<string>(), It.IsAny<BillingFileGenerationMessage>()));
            this.configMock.Setup(s => s.GetSection(It.IsAny<string>()).GetSection(It.IsAny<string>()).Value).Returns("test");

            // Act
            var result = await this.producerFileControllerTest.ProducerBillingInstructions(1, CancellationToken.None) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(httpStatusCode, result.StatusCode);
        }
    }
}
