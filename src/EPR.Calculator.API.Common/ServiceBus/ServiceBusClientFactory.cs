using Azure.Messaging.ServiceBus;

namespace EPR.Calculator.API.Common.ServiceBus
{
    public class ServiceBusClientFactory : IServiceBusClientFactory
    {
        private readonly string _connectionString;

        public ServiceBusClientFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public ServiceBusClient GetServiceBusClient()
        {
            try
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    throw new ArgumentNullException(_connectionString, "ServiceBusClient: Connection string not provided.");
                }
                return new ServiceBusClient(_connectionString);
            }
            catch (Exception exception)
            {
                throw new ServiceBusException(exception.Message, ServiceBusFailureReason.ServiceCommunicationProblem);
            }
        }
    }
}
