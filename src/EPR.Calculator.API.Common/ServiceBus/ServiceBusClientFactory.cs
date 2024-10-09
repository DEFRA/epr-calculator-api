using Azure.Messaging.ServiceBus;

namespace EPR.Calculator.API.Common.ServiceBus
{
    public class ServiceBusClientFactory : IServiceBusClientFactory
    {
        private readonly string _connectionString;
        private readonly int _retryCount;
        private readonly int _retryPeriod;

        public ServiceBusClientFactory(string connectionString, int retryCount, int retryPeriod)
        {
            _connectionString = connectionString;
            _retryCount = retryCount;
            _retryPeriod = retryPeriod;
        }

        public ServiceBusClient GetServiceBusClient()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_connectionString))
                {
                    throw new ArgumentNullException(_connectionString, "ServiceBusClient: Connection string not provided.");
                }

                var options = new ServiceBusClientOptions();
                options.RetryOptions = new ServiceBusRetryOptions
                {
                    Delay = TimeSpan.FromSeconds(_retryPeriod),
                    MaxDelay = TimeSpan.FromSeconds(_retryPeriod),
                    Mode = ServiceBusRetryMode.Exponential,
                    MaxRetries = _retryCount,
                };

                return new ServiceBusClient(_connectionString, options);
            }
            catch (Exception exception)
            {
                throw new ServiceBusException(exception.Message, ServiceBusFailureReason.ServiceCommunicationProblem);
            }
        }
    }
}
