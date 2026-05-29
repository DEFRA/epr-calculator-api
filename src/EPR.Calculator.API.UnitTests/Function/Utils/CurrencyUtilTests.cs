using System.Globalization;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.UnitTests.Utils
{
    [TestClass]
    public class CurrencyUtilTests
    {
        [TestMethod]
        public void CanCallConvertToCurrency()
        {
            // Arrange
            var detail = 100.00m;

            // Act
            var result = CurrencyConverterUtils.ConvertToCurrency(detail);
            bool iscurrency = decimal.TryParse(result, NumberStyles.Currency, new CultureInfo("en-GB"), out _);

            // Assert
            Assert.IsTrue(iscurrency);
        }
    }
}