using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Services.Abstractions;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Amqp.Transaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class BillingFileNewControllerTests
    {
        private readonly BillingFileNewController billingFileControllerUnderTest;

        private readonly Mock<IBillingFileService> billingFileServiceMock;

        public BillingFileNewControllerTests()
        {
            this.billingFileServiceMock = new Mock<IBillingFileService>();
            this.billingFileControllerUnderTest = new BillingFileNewController(this.billingFileServiceMock.Object);

            // Mock User Claims
            // Set up authorisation.
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);
            var context = new DefaultHttpContext { User = principal };
            this.billingFileControllerUnderTest.ControllerContext = new ControllerContext { HttpContext = context };
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
            this.billingFileServiceMock.Setup(s => s.UpdateProducerBillingInstructionsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ProduceBillingInstuctionRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ServiceProcessResponseDto { StatusCode = (HttpStatusCode)httpStatusCode });

            var requestDto = new ProduceBillingInstuctionRequestDto
            {
                Status = BillingStatus.Rejected.ToString(),
                ReasonForRejection = null,
                OrganisationIds = new List<int> { 1, 2, 3 },
            };

            // Act
            var result = await this.billingFileControllerUnderTest.ProducerBillingInstructions(1, requestDto, CancellationToken.None) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(httpStatusCode, result.StatusCode);
        }
    }
}
