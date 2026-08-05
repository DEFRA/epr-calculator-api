using EPR.Calculator.Service.Function.Converter;

namespace EPR.Calculator.Service.Function.UnitTests.Converter
{
    [TestClass]
    public class DateTimeConversionTests
    {
        [TestMethod]
        public void ConvertToIso8601Utc_NullOrEmpty_ReturnsEmptyString()
        {
            // Arrange & Act & Assert            
            Assert.AreEqual(string.Empty, DateTimeConversion.ConvertToIso8601Utc(string.Empty));
            Assert.AreEqual(string.Empty, DateTimeConversion.ConvertToIso8601Utc("   "));
        }

        [TestMethod]
        public void ConvertToIso8601Utc_InvalidDateTimeString_ThrowsFormatException()
        {
            // Arrange  
            string input = "invalid-date";

            // Act & Assert
            Should.Throw<FormatException>(() => DateTimeConversion.ConvertToIso8601Utc(input));
        }
    }
}
