using Azure.Messaging.ServiceBus;

namespace EPR.Calculator.API.Common.ServiceBus
{
    public class ServiceBusClientFactory : IServiceBusClientFactory
    {
        public ServiceBusClient GetServiceBusClient(string connectionString, int retryCount, int retryPeriod)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new ArgumentNullException(connectionString, "ServiceBusClient: Connection string not provided.");
                }

                var options = new ServiceBusClientOptions();
                options.RetryOptions = new ServiceBusRetryOptions
                {
                    Delay = TimeSpan.FromSeconds(retryPeriod),
                    MaxDelay = TimeSpan.FromSeconds(retryPeriod),
                    Mode = ServiceBusRetryMode.Exponential,
                    MaxRetries = retryCount,
                };

                return new ServiceBusClient(connectionString, options);
            }
            catch (Exception exception)
            {
                throw new ServiceBusException(exception.Message, ServiceBusFailureReason.ServiceCommunicationProblem);
            }
        }
    }
}
