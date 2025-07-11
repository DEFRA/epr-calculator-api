namespace EPR.Calculator.API.UnitTests.Services
{
    using System.Threading.Tasks;
    using Azure.Messaging.ServiceBus;
    using EPR.Calculator.API.Constants;
    using EPR.Calculator.API.Models;
    using EPR.Calculator.API.Services;
    using Microsoft.Extensions.Azure;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ServiceBusServiceTests
    {
        private readonly Mock<IAzureClientFactory<ServiceBusClient>> mockServiceBusClientFactory;
        private readonly ServiceBusService serviceBusService;

        public ServiceBusServiceTests()
        {
            this.mockServiceBusClientFactory = new Mock<IAzureClientFactory<ServiceBusClient>>();
            this.serviceBusService = new ServiceBusService(this.mockServiceBusClientFactory.Object);
        }

        [TestMethod]
        public async Task CanSendMessage()
        {
            // Arrange
            var serviceBusQueueName = "Some queue";
            var calculatorRunMessage = new CalculatorRunMessage() { CalculatorRunId = 1, CreatedBy = "Test user", FinancialYear = "2024-25", MessageType = CommonConstants.ResultMessageType };

            var mockServiceBusClient = new Mock<ServiceBusClient>();
            this.mockServiceBusClientFactory.Setup(clientFactory => clientFactory.CreateClient(It.IsAny<string>()))
                .Returns(mockServiceBusClient.Object);

            var mockServiceBusSender = new Mock<ServiceBusSender>();
            mockServiceBusClient.Setup(client => client.CreateSender(It.IsAny<string>()))
                .Returns(mockServiceBusSender.Object);

            mockServiceBusSender.Setup(sender => sender.SendMessageAsync(It.IsAny<ServiceBusMessage>(), default))
                .Returns(Task.CompletedTask);

            // Act
            await this.serviceBusService.SendMessage(serviceBusQueueName, calculatorRunMessage);

            // Assert
            this.mockServiceBusClientFactory.Verify(clientFactory => clientFactory.CreateClient(It.IsAny<string>()), Times.Once);
            mockServiceBusClient.Verify(client => client.CreateSender(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task CanSendBiilingInstructionMessage()
        {
            // Arrange
            var serviceBusQueueName = "Some queue";
            var billingFileGenerationMessage = new BillingFileGenerationMessage() { ApprovedBy = "Test user", MessageType = CommonConstants.BillingMessageType, CalculatorRunId = 1 };

            var mockServiceBusClient = new Mock<ServiceBusClient>();
            this.mockServiceBusClientFactory.Setup(clientFactory => clientFactory.CreateClient(It.IsAny<string>()))
                .Returns(mockServiceBusClient.Object);

            var mockServiceBusSender = new Mock<ServiceBusSender>();
            mockServiceBusClient.Setup(client => client.CreateSender(It.IsAny<string>()))
                .Returns(mockServiceBusSender.Object);

            mockServiceBusSender.Setup(sender => sender.SendMessageAsync(It.IsAny<ServiceBusMessage>(), default))
                .Returns(Task.CompletedTask);

            // Act
            await this.serviceBusService.SendMessage(serviceBusQueueName, billingFileGenerationMessage);

            // Assert
            this.mockServiceBusClientFactory.Verify(clientFactory => clientFactory.CreateClient(It.IsAny<string>()), Times.Once);
            mockServiceBusClient.Verify(client => client.CreateSender(It.IsAny<string>()), Times.Once);
        }
    }
}