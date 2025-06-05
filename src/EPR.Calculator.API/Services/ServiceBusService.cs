using Azure.Messaging.ServiceBus;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Models;
using Microsoft.Extensions.Azure;
using Newtonsoft.Json;

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
            var messageString = JsonConvert.SerializeObject(calculatorRunMessage);
            await SendMessageAsync(serviceBusQueueName, messageString);
        }

        public async Task SendMessage(string serviceBusQueueName, BillingFileGenerationMessage billingFileGenerationMessage)
        {
            var messageString = JsonConvert.SerializeObject(billingFileGenerationMessage);
            await SendMessageAsync(serviceBusQueueName, messageString);
        }

        private async Task SendMessageAsync(string queueName, string message)
        {
            var client = this.serviceBusClientFactory.CreateClient(CommonConstants.ServiceBusClientName);
            var serviceBusSender = client.CreateSender(queueName);
            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(message);
            await serviceBusSender.SendMessageAsync(serviceBusMessage);
        }
    }
}
