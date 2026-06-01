using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.UnitTests.Utils
{
    [TestClass]
    public class CurrencyConverterTests
    {
        [TestMethod]
        public void CanCallFormatAsGbpCurrencyForZeroCurrency()
        {
            // Act
            var result = CurrencyConverterUtils.ConvertToCurrency(0);

            // Assert
            Assert.AreEqual("£0.00", result);
        }

        [TestMethod]
        public void CanCallFormatAsGbpCurrencyWithRounding()
        {
            // Act
            var result = CurrencyConverterUtils.ConvertToCurrency(10.578m);

            // Assert
            Assert.AreEqual("£10.58", result);
        }

        [TestMethod]
        public void CanCallFormatAsGbpCurrencyWithRoundingFourDecimalPlaces()
        {
            // Act
            var result = CurrencyConverterUtils.ConvertToCurrency(10.53324785678m, 4);

            // Assert
            Assert.AreEqual("£10.5332", result);
        }
    }
}
