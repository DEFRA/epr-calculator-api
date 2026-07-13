using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.API.UnitTests.Controllers;

[TestClass]
public class ProducerBillingFileControllerTests
{
    private const int RunId = 1;
    private const string UserName = "TestUser";

    private Mock<IBillingFileService> billingFileServiceMock = null!;
    private Mock<IServiceBusService> serviceBusMock = null!;
    private ProducerBillingFileController controller = null!;

    [TestInitialize]
    public void Setup()
    {
        billingFileServiceMock = new Mock<IBillingFileService>();
        serviceBusMock = new Mock<IServiceBusService>();
        controller = new ProducerBillingFileController(
            billingFileServiceMock.Object,
            serviceBusMock.Object)
        {
            ControllerContext = CreateAuthenticatedControllerContext(UserName),
        };
    }

    [TestMethod]
    [DataRow(HttpStatusCode.OK, "OK")]
    [DataRow(HttpStatusCode.BadRequest, "Bad Request.")]
    [DataRow(HttpStatusCode.UnprocessableEntity, "Unprocessable Entity")]
    public async Task ProducerBillingInstructions_ReturnsStatusCodeAndMessageFromService(
        HttpStatusCode statusCode,
        string message)
    {
        // Arrange
        SetupBillingFileService(statusCode, message);

        // Act
        var result = await controller.ProducerBillingInstructions(RunId, CancellationToken.None) as ObjectResult;

        // Assert
        result.ShouldNotBeNull();
        result.StatusCode.ShouldBe((int)statusCode);
        result.Value.ShouldBe(message);
    }

    [TestMethod]
    public async Task ProducerBillingInstructions_SendsBillingMessageWithRunAndApprover_WhenServiceReturnsOk()
    {
        // Arrange
        SetupBillingFileService(HttpStatusCode.OK);

        // Act
        await controller.ProducerBillingInstructions(RunId, CancellationToken.None);

        // Assert
        serviceBusMock.Verify(
            s => s.SendMessage(It.Is<BillingFileGenerationMessage>(
                m => m.CalculatorRunId == RunId && m.ApprovedBy == UserName)),
            Times.Once);
    }

    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.UnprocessableEntity)]
    [DataRow(HttpStatusCode.InternalServerError)]
    public async Task ProducerBillingInstructions_DoesNotSendBillingMessage_WhenServiceDoesNotReturnOk(
        HttpStatusCode statusCode)
    {
        // Arrange
        SetupBillingFileService(statusCode);

        // Act
        await controller.ProducerBillingInstructions(RunId, CancellationToken.None);

        // Assert
        serviceBusMock.Verify(
            s => s.SendMessage(It.IsAny<BillingFileGenerationMessage>()),
            Times.Never);
    }

    private void SetupBillingFileService(HttpStatusCode statusCode, string? message = null)
    {
        billingFileServiceMock
            .Setup(s => s.StartGeneratingBillingFileAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BillingFileService.Response { StatusCode = statusCode, Message = message });
    }

    private static ControllerContext CreateAuthenticatedControllerContext(string userName)
    {
        var identity = new GenericIdentity(userName);
        identity.AddClaim(new Claim("name", userName));
        var principal = new ClaimsPrincipal(identity);

        return new ControllerContext { HttpContext = new DefaultHttpContext { User = principal } };
    }
}
