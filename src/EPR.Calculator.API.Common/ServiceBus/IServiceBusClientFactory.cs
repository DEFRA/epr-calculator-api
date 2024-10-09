using Azure.Messaging.ServiceBus;

namespace EPR.Calculator.API.Common.ServiceBus
{
    public interface IServiceBusClientFactory
    {
        public ServiceBusClient GetServiceBusClient();
    }
}
