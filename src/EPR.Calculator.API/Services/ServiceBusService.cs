using Azure.Messaging.ServiceBus;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Models;
using Microsoft.Extensions.Azure;
using Newtonsoft.Json;

namespace EPR.Calculator.API.Services
{
    public class ServiceBusService : IServiceBusService
    {
        public async Task SendMessage(IAzureClientFactory<ServiceBusClient> serviceBusClientFactory, string serviceBusQueueName, CalculatorRunMessage calculatorRunMessage)
        {
            var client = serviceBusClientFactory.CreateClient(CommonConstants.ServiceBusClientName);
            ServiceBusSender serviceBusSender = client.CreateSender(serviceBusQueueName);
            var messageString = JsonConvert.SerializeObject(calculatorRunMessage);
            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(messageString);
            await serviceBusSender.SendMessageAsync(serviceBusMessage);
        }
    }
}
