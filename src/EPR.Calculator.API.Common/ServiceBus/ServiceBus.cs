using Azure.Messaging.ServiceBus;
using EPR.Calculator.API.Common.Models;
using Newtonsoft.Json;

namespace EPR.Calculator.API.Common.ServiceBus
{
    public static class ServiceBus
    {
        public static async Task SendMessage(string serviceBusConnectionString, string serviceBusQueueName, CalculatorRunMessage message, int messageRetryCount, int messageRetryPeriod)
        {
            ServiceBusClientFactory serviceBusClientFactory = new ServiceBusClientFactory();

            await using (ServiceBusClient serviceBusClient = serviceBusClientFactory.GetServiceBusClient(serviceBusConnectionString, messageRetryCount, messageRetryPeriod))
            {
                var messageString = JsonConvert.SerializeObject(message);
                ServiceBusMessage serviceBusMessage = new ServiceBusMessage(messageString);

                ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(serviceBusQueueName);

                await serviceBusSender.SendMessageAsync(serviceBusMessage);
            }
        }
    }
}
