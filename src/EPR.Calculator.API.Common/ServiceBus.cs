using Azure.Messaging.ServiceBus;
using EPR.Calculator.API.Common.Models;
using Newtonsoft.Json;

namespace EPR.Calculator.API.Common
{
    public static class ServiceBus
    {
        private static readonly string ConnectionString = ConfigurationHelper.GetSetting("ServiceBus:ConnectionString");
        private static readonly string QueueName = ConfigurationHelper.GetSetting("ServiceBus:QueueName");

        public static async Task SendMessage(CalculatorRunMessage message)
        {
            ServiceBusClient serviceBusClient = new ServiceBusClient(ConnectionString);

            var messageString = JsonConvert.SerializeObject(message);
            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(messageString);

            ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(QueueName);
            await serviceBusSender.SendMessageAsync(serviceBusMessage);
        }
    }
}
