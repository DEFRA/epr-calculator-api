using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;

namespace EPR.Calculator.Service.Function.UnitTests.Misc
{
    [TestClass]
    public class CsvSanitiserTests
    {
        [TestMethod]
        public void ShouldSanitiseData()
        {
            // Arrange
            var data = "Some data\n\t";

            // Act
            var result = CsvSanitiser.SanitiseData(data);

            // Assert
            Assert.AreEqual("\"Some data\",", result);
        }

        [TestMethod]
        public void ShouldReturnCommaIfNoData()
        {
            // Arrange
            var data = string.Empty;

            // Act
            var result = CsvSanitiser.SanitiseData(data);

            // Assert
            Assert.AreEqual("\"\",", result);
        }

        [TestMethod]
        public void ShouldSanitiseCurrencyData()
        {
            // Arrange
            var data = 100.5987m;
            var decimalPlaces = DecimalPlaces.Two;
            var isCurrency = true;

            // Act
            var result = CsvSanitiser.SanitiseData(data, decimalPlaces, null, isCurrency);

            // Assert
            Assert.AreEqual("\"£100.60\",", result);
        }

        [TestMethod]
        public void ShouldSanitisePercentageData()
        {
            // Arrange
            var data = 79.798m;
            var decimalPlaces = DecimalPlaces.Two;
            var isCurrency = false;
            var isPercentage = true;

            // Act
            var result = CsvSanitiser.SanitiseData(data, decimalPlaces, null, isCurrency, isPercentage);

            // Assert
            Assert.AreEqual("\"79.80%\",", result);
        }

        [TestMethod]
        public void ShouldShowHyphen_WhenFlagCanBeEmptyIsTrueAndValueIsNull()
        {
            // Arrange
            decimal? v = null;

            // Act
            var result = CsvSanitiser.SanitiseData(v, DecimalPlaces.Three, null, canBeEmpty: true);

            // Assert
            Assert.AreEqual("\"-\",", result);
        }

        [TestMethod]
        public void ShouldShowZero_WhenFlagCanBeEmptyIsFalseAndValueIsNull()
        {
            // Arrange
            decimal? v = null;

            // Act
            var result = CsvSanitiser.SanitiseData(v, DecimalPlaces.Three, null);

            // Assert
            Assert.AreEqual("\"0\",", result);
        }

        [TestMethod]
        public void ShouldPrefixLrm_WhenFlagTrue()
        {
            // Arrange
            var data = "+ve";

            // Act
            var result = CsvSanitiser.SanitiseData(
                data,
                appendLrmCharacterToPreventRenderedAsFormula: true);

            // Assert
            Assert.AreEqual("\"\u200E+ve\",", result);
        }

        [TestMethod]
        public void ShouldNotPrefixLrm_WhenFlagDefaultValueIsFalse()
        {
            // Arrange
            var data = "+ve";

            // Act
            var result = CsvSanitiser.SanitiseData(data);

            // Assert
            Assert.AreEqual("\"+ve\",", result);
        }

        [TestMethod]
        public void ShouldNotPrefixLrm_WhenValueIsNull()
        {
            string? data = null;

            var result = CsvSanitiser.SanitiseData(
                data,
                appendLrmCharacterToPreventRenderedAsFormula: true);

            Assert.AreEqual(",", result);
        }

        [TestMethod]
        public void ShouldNotPrefixLrm_WhenValueIsHyphen()
        {
            // Arrange
            var data = "-";

            // Act
            var result = CsvSanitiser.SanitiseData(
                data,
                appendLrmCharacterToPreventRenderedAsFormula: true);

            // Assert
            Assert.AreEqual("\"-\",", result);
        }
    }
}