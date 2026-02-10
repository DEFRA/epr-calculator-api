using System.Text.Json;
using Azure.Messaging.ServiceBus;
using EPR.Calculator.API.Models;
using Microsoft.Extensions.Azure;

namespace EPR.Calculator.API.Services
{
    public class ServiceBusService : IServiceBusService
    {
        private readonly IAzureClientFactory<ServiceBusClient> serviceBusClientFactory;

        public ServiceBusService(IAzureClientFactory<ServiceBusClient> serviceBusClientFactory)
        {
            this.serviceBusClientFactory = serviceBusClientFactory;
        }

        public async Task SendMessage(string serviceBusQueueName, CalculatorRunMessage calculatorRunMessage)
        {
            var messageString = JsonSerializer.Serialize(calculatorRunMessage);
            await SendMessageAsync(serviceBusQueueName, messageString);
        }

        public async Task SendMessage(string serviceBusQueueName, BillingFileGenerationMessage billingFileGenerationMessage)
        {
            var messageString = JsonSerializer.Serialize(billingFileGenerationMessage);
            await SendMessageAsync(serviceBusQueueName, messageString);
        }

        private async Task SendMessageAsync(string queueName, string message)
        {
            var client = this.serviceBusClientFactory.CreateClient(CommonResources.ServiceBusClientName);
            var serviceBusSender = client.CreateSender(queueName);
            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(message);
            await serviceBusSender.SendMessageAsync(serviceBusMessage);
        }
    }
}
