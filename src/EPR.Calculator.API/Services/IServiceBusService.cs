using Azure.Messaging.ServiceBus;
using EPR.Calculator.API.Models;
using Microsoft.Extensions.Azure;

namespace EPR.Calculator.API.Services
{
    public interface IServiceBusService
    {
        public Task SendMessage(IAzureClientFactory<ServiceBusClient> serviceBusClientFactory, string serviceBusQueueName, CalculatorRunMessage calculatorRunMessage);
    }
}
