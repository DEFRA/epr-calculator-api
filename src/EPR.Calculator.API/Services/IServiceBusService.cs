using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Services
{
    public interface IServiceBusService
    {
        public Task SendMessage(string serviceBusQueueName, CalculatorRunMessage calculatorRunMessage);

        public Task SendMessage(string? serviceBusQueueName, BillingFileGenerationMessage billingFileGenerationMessage);
    }
}
