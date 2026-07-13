using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using EPR.Calculator.API.Options;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.API.Services;

public interface IServiceBusService
{
    public Task SendMessage<T>(T message);
}

[ExcludeFromCodeCoverage(Justification = "This is a thin wrapper around the Azure SDK; meaningful correctness is not verifiable in unit tests.")]
public class ServiceBusService (
    ServiceBusClient serviceBusClient,
    IOptions<ServiceBusOptions> options
) : IServiceBusService
{
    public async Task SendMessage<T>(T message)
    {
        var messageString = JsonSerializer.Serialize(message);
        var serviceBusMessage = new ServiceBusMessage(messageString);
        await using var sender = serviceBusClient.CreateSender(options.Value.QueueName);
        await sender.SendMessageAsync(serviceBusMessage);
    }
}
