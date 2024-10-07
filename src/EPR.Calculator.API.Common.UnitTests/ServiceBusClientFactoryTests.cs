using Azure.Messaging.ServiceBus;
using EPR.Calculator.API.Common.ServiceBus;

namespace EPR.Calculator.API.Common.UnitTests
{
    [TestClass]
    public class ServiceBusClientFactoryTests
    {
        [TestMethod]
        public void GetServiceBusClient_ReturnErrorIfNoConnectionString()
        {
            try
            {
                var serviceBusClientFactory = new ServiceBusClientFactory(string.Empty);
                var serviceBusClient = serviceBusClientFactory.GetServiceBusClient();
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
