using Azure.Messaging.ServiceBus;
using EPR.Calculator.API.Common.Models;
using Moq;
using Newtonsoft.Json;

namespace EPR.Calculator.API.Common.UnitTests
{
    [TestClass]
    public class ServiceBusTests
    {
        [TestMethod]
        public async Task SendMessage_ReturnErrorIfNoConnectionString()
        {
            try
            {
                var message = new CalculatorRunMessage { CalculatorRunId = 123450, FinancialYear = "2024-25", CreatedBy = "Test User" };
                await ServiceBus.ServiceBus.SendMessage(string.Empty, "TestQueue", message, 1, 1);
            }
            catch (ServiceBusException exception)
            {
                Assert.IsTrue(exception.Message.Contains("ServiceBusClient: Connection string not provided."));
                Assert.AreEqual(ServiceBusFailureReason.ServiceCommunicationProblem, exception.Reason);
            }
        }

        [TestMethod]
        public async Task SendMessage_Succeeds()
        {
            Mock<ServiceBusClient> clientMock = new Mock<ServiceBusClient>();
            Mock<ServiceBusSender> senderMock = new Mock<ServiceBusSender>();

            clientMock
                .Setup(client => client.CreateSender(It.IsAny<string>()))
                .Returns(senderMock.Object);

            senderMock
                .Setup(sender => sender.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var client = clientMock.Object;

            var sender = client.CreateSender("Test queue name");

            var message = new CalculatorRunMessage { CalculatorRunId = 123450, FinancialYear = "2024-25", CreatedBy = "Test User" };
            var messageString = JsonConvert.SerializeObject(message);

            var serviceBusMessage = new ServiceBusMessage(messageString);
            await sender.SendMessageAsync(serviceBusMessage);

            senderMock
                .Verify(sender => sender.SendMessageAsync(
                    It.Is<ServiceBusMessage>(m => (m.MessageId == serviceBusMessage.MessageId)),
                    It.IsAny<CancellationToken>()));
        }
    }
}