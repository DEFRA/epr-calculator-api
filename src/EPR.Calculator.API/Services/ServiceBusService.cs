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
            var client = serviceBusClientFactory.CreateClient(CommonConstants.ServiceBusClientName);
            var serviceBusSender = client.CreateSender(serviceBusQueueName);
            var messageString = JsonConvert.SerializeObject(calculatorRunMessage);
            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(messageString);
            await serviceBusSender.SendMessageAsync(serviceBusMessage);
        }
    }
}
