using Azure.Messaging.ServiceBus;
using EPR.Calculator.API.Common.Models;

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
                var message = new CalculatorRunMessage { CalculatorRunId = 123450, FinancialYear = "2024-25" };
                await ServiceBus.ServiceBus.SendMessage(string.Empty, "TestQueue", message, 1, 1);
            }
            catch (ServiceBusException exception)
            {
                Assert.IsTrue(exception.Message.Contains("ServiceBusClient: Connection string not provided."));
                Assert.AreEqual(ServiceBusFailureReason.ServiceCommunicationProblem, exception.Reason);
            }
        }

        // TO DO: Write more coverage during actual implementation
    }
}