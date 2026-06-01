using System.Text.Json;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Messaging;

namespace EPR.Calculator.Service.Function.UnitTests.Messaging
{
    [TestClass]
    public class MessageTypeServiceTests
    {
        private MessageTypeService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _service = new MessageTypeService();
        }

        [TestMethod]
        public void DeserializeMessage_ValidBillingMessage_ReturnsCreateBillingFileMessage()
        {
            // Arrange
            var json = """
            {
                "MessageType": "Billing",
                "CalculatorRunId": 123,
                "ApprovedBy": "Test User"
            }
            """;

            // Act
            var result = _service.DeserializeMessage(json);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(CreateBillingFileMessage));

            var billingMessage = (CreateBillingFileMessage)result;
            Assert.AreEqual(123, billingMessage.CalculatorRunId);
            Assert.AreEqual("Billing", billingMessage.MessageType);
        }

        [TestMethod]
        public void DeserializeMessage_ResultMessageType_ReturnsCreateResultFileMessage()
        {
            // Arrange
            var json = """
            {
                "MessageType": "Result",
                "CalculatorRunId": 123,
                "CreatedBy": "Test User",
                "RelativeYear": 2024
            }
            """;

            // Act
            var result = _service.DeserializeMessage(json);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(CreateResultFileMessage));

            var billingMessage = (CreateResultFileMessage)result;
            Assert.AreEqual(123, billingMessage.CalculatorRunId);
            Assert.AreEqual("Test User", billingMessage.CreatedBy);
            Assert.AreEqual(new RelativeYear(2024), billingMessage.RelativeYear);
            Assert.AreEqual("Result", billingMessage.MessageType);
        }

        [TestMethod]
        public void DeserializeMessage_MissingMessageType_Throws()
        {
            // Arrange
            var json = """
                {
                    "CalculatorRunId": 123,
                    "CreatedBy": "Test User",
                    "RelativeYear": 2024
                }
                """;

            // Act & Assert
            Should.Throw<KeyNotFoundException>(() => _service.DeserializeMessage(json));
        }

        [TestMethod]
        public void DeserializeMessage_UnsupportedMessageType_ThrowsNotSupportedException()
        {
            // Arrange
            var json = """
                {
                    "MessageType": "Unknown"
                }
                """;

            // Act & Assert
            Should.Throw<NotSupportedException>(() => _service.DeserializeMessage(json));
        }

        [TestMethod]
        public void DeserializeMessage_NullOrEmpty_ThrowsJsonException()
        {
            // Act & Assert
            Should.Throw<JsonException>(() => _service.DeserializeMessage(null!));
        }

        [TestMethod]
        public void DeserializeMessage_InvalidJson_ThrowsJsonException()
        {
            // Arrange
            var json = "invalid json";

            // Act & Assert
            Should.Throw<JsonException>(() => _service.DeserializeMessage(json));
        }
    }
}
