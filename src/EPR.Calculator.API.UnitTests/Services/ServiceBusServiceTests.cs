namespace EPR.Calculator.API.UnitTests.Services
{
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using Azure;
    using Azure.Messaging.ServiceBus;
    using Azure.Storage.Blobs.Models;
    using Azure.Storage.Blobs.Specialized;
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
            mockServiceBusClientFactory = new Mock<IAzureClientFactory<ServiceBusClient>>();
            serviceBusService = new ServiceBusService(mockServiceBusClientFactory.Object);
        }

        [TestMethod]
        public async Task CanSendMessage()
        {
            // Arrange
            var serviceBusQueueName = "Some queue";
            var calculatorRunMessage = new CalculatorRunMessage() { CalculatorRunId = 1, CreatedBy = "Test user", FinancialYear = "2024-25" };

            var mockServiceBusClient = new Mock<ServiceBusClient>();
            mockServiceBusClientFactory.Setup(clientFactory => clientFactory.CreateClient(It.IsAny<string>()))
                .Returns(mockServiceBusClient.Object);

            var mockServiceBusSender = new Mock<ServiceBusSender>();
            mockServiceBusClient.Setup(client => client.CreateSender(It.IsAny<string>()))
                .Returns(mockServiceBusSender.Object);

            mockServiceBusSender.Setup(sender => sender.SendMessageAsync(It.IsAny<ServiceBusMessage>(), default))
                .Returns(Task.CompletedTask);

            // Act
            await serviceBusService.SendMessage(serviceBusQueueName, calculatorRunMessage);

            // Assert
            mockServiceBusClientFactory.Verify(clientFactory => clientFactory.CreateClient(It.IsAny<string>()), Times.Once);
            mockServiceBusClient.Verify(client => client.CreateSender(It.IsAny<string>()), Times.Once);
        }
    }
}