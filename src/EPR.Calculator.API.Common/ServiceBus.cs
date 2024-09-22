using Azure.Messaging.ServiceBus;

namespace EPR.Calculator.API.Common
{
    public static class ServiceBus
    {
        public static async Task SendMessage(string connectionString, string queueName)
        {
            ServiceBusClient serviceBusClient = new ServiceBusClient(connectionString);

            ServiceBusMessage serviceBusMessage = new ServiceBusMessage("Test message");

            ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(queueName);
            await serviceBusSender.SendMessageAsync(serviceBusMessage);
        }
    }
}
