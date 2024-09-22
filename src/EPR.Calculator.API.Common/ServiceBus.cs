using Azure.Messaging.ServiceBus;
using EPR.Calculator.API.Common.Models;

namespace EPR.Calculator.API.Common
{
    public static class ServiceBus
    {
        private static readonly string ConnectionString = ConfigurationHelper.GetSetting("ServiceBus:ConnectionString");
        private static readonly string QueueName = ConfigurationHelper.GetSetting("ServiceBus:QueueName");

        public static async Task SendMessage(CalculatorRunMessage message)
        {
            try
            {
                ServiceBusClient serviceBusClient = new ServiceBusClient(ConnectionString);

                ServiceBusMessage serviceBusMessage = new ServiceBusMessage(message.CalculatorRunId);

                ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(QueueName);
                await serviceBusSender.SendMessageAsync(serviceBusMessage);
            }
            catch (Exception)
            {
                // TO DO: throw exception
            }
        }
    }
}
