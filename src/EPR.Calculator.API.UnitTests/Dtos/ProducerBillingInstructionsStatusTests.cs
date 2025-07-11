using EPR.Calculator.API.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests.Dtos
{
    [TestClass]
    public class ProducerBillingInstructionsStatusTests
    {
        [TestMethod]
        public void ProducerBillingInstructionsStatus_ShouldInitializeProperties()
        {
            // Arrange
            var status = "Active";
            var totalRecords = 100;

            // Act
            var producerBillingInstructionsStatus = new ProducerBillingInstructionsStatus
            {
                Status = status,
                TotalRecords = totalRecords,
            };

            // Assert
            Assert.AreEqual(status, producerBillingInstructionsStatus.Status);
            Assert.AreEqual(totalRecords, producerBillingInstructionsStatus.TotalRecords);
        }
    }
}
