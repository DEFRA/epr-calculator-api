using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using EPR.Calculator.API.BackgroundService;
using EPR.Calculator.API.Controllers;
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
    private Mock<IBackgroundTaskQueue> backgroundTaskQueueMock = null!;
    private ProducerBillingFileController controller = null!;

    [TestInitialize]
    public void Setup()
    {
        billingFileServiceMock = new Mock<IBillingFileService>();
        backgroundTaskQueueMock = new Mock<IBackgroundTaskQueue>();
        controller = new ProducerBillingFileController(
            billingFileServiceMock.Object,
            backgroundTaskQueueMock.Object)
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
        backgroundTaskQueueMock.Verify(
            s => s.QueueAsync(It.Is<BackgroundServiceMessage>(
                m => m.CalculatorRunId == RunId && m.Username == UserName),
                It.IsAny<CancellationToken>()),
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
        backgroundTaskQueueMock.Verify(
            s => s.QueueAsync(It.IsAny<BackgroundServiceMessage>(), It.IsAny<CancellationToken>()),
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
