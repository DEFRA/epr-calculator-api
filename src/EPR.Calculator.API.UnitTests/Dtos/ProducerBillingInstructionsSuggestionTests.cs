using EPR.Calculator.API.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests.Dtos
{
    [TestClass]
    public class ProducerBillingInstructionsSuggestionTests
    {
        [TestMethod]
        public void ProducerBillingInstructionsSuggestionTests_ShouldInitializeProperties()
        {
            // Arrange
            var status = "Active";
            var totalRecords = 100;

            // Act
            var producerBillingInstructionsStatus = new ProducerBillingInstructionSuggestion
            {
                Suggestion = status,
                TotalRecords = totalRecords,
            };

            // Assert
            Assert.AreEqual(status, producerBillingInstructionsStatus.Suggestion);
            Assert.AreEqual(totalRecords, producerBillingInstructionsStatus.TotalRecords);
        }
    }
}
