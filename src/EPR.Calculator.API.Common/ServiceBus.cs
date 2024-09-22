using Azure.Messaging.ServiceBus;
using EPR.Calculator.API.Common.Models;
using Newtonsoft.Json;

namespace EPR.Calculator.API.Common
{
    public static class ServiceBus
    {
        public static async Task SendMessage(string serviceBusConnectionString, string serviceBusQueueName, CalculatorRunMessage message)
        {
            ServiceBusClient serviceBusClient = new ServiceBusClient(serviceBusConnectionString);

            var messageString = JsonConvert.SerializeObject(message);
            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(messageString);

            ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(serviceBusQueueName);
            await serviceBusSender.SendMessageAsync(serviceBusMessage);
        }
    }
}
