using System.Globalization;
using EPR.Calculator.API.BackgroundService.Utils;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Utils
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
            var result = FormatUtils.FormatCurrency(detail);
            bool iscurrency = decimal.TryParse(result, NumberStyles.Currency, new CultureInfo("en-GB"), out _);

            // Assert
            Assert.IsTrue(iscurrency);
        }
    }
}
