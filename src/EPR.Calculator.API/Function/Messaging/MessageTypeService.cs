using System.Text.Json;

namespace EPR.Calculator.Service.Function.Messaging
{
    /// <summary>
    /// Defines a service for deserializing JSON strings into specific message types.
    /// </summary>
    public interface IMessageTypeService
    {
        /// <summary>
        /// Deserializes the specified JSON string into a concrete <see cref="MessageBase"/> derived type
        /// based on the contained message type information.
        /// </summary>
        MessageBase DeserializeMessage(string json);
    }

    /// <summary>
    /// Service for handling Mmssage type service operations.
    /// </summary>
    public class MessageTypeService : IMessageTypeService
    {
        private readonly Dictionary<string, Type> typeMappings = new()
        {
            { "Billing", typeof(CreateBillingFileMessage) },
            { "Result", typeof(CreateResultFileMessage) }
        };

        public MessageBase DeserializeMessage(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new JsonException("Input JSON is null or empty.");

            var jObject = JsonDocument.Parse(json);
            var messageType = jObject.RootElement.GetProperty("MessageType").ToString();

            if (string.IsNullOrWhiteSpace(messageType))
                throw new ArgumentException("MessageType not found in the message.");

            if (!typeMappings.TryGetValue(messageType, out var targetType))
                throw new NotSupportedException($"Unsupported MessageType: {messageType}");

            return (MessageBase) JsonSerializer.Deserialize(json, targetType)!;
        }
    }
}
