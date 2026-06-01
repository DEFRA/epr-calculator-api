using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;

namespace EPR.Calculator.Service.Function.UnitTests.Misc
{
    [TestClass]
    public class CsvSanitiserTests
    {
        [TestMethod]
        public void ShouldSanitiseDataWithDelimiter()
        {
            // Arrange
            var data = "Some data\n\t";

            // Act
            var result = CsvSanitiser.SanitiseData(data);

            // Assert
            Assert.AreEqual("\"Some data\",", result);
        }

        [TestMethod]
        public void ShouldSanitiseDataWithoutDemiliter()
        {
            // Arrange
            var data = "Some,\t data";

            // Act
            var result = CsvSanitiser.SanitiseData(data, false);

            // Assert
            Assert.AreEqual("\"Some data\"", result);
        }

        [TestMethod]
        public void ShouldReturnCommaIfNoDataWithDelimitedEnabled()
        {
            // Arrange
            var data = string.Empty;

            // Act
            var result = CsvSanitiser.SanitiseData(data);

            // Assert
            Assert.AreEqual("\"\",", result);
        }

        [TestMethod]
        public void ShouldReturnEmptyStringIfNoDataWithoutDelimiter()
        {
            // Arrange
            var data = string.Empty;

            // Act
            var result = CsvSanitiser.SanitiseData(data, false);

            // Assert
            Assert.AreEqual("\"\"", result);
        }

        [TestMethod]
        public void ShouldSanitiseCurrencyDataWithDelimiter()
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
        public void ShouldSanitiseCurrencyDataWithoutDelimiter()
        {
            // Arrange
            var data = 290.5987432m;
            var decimalPlaces = DecimalPlaces.Three;
            var isCurrency = true;
            var isPercentage = false;

            // Act
            var result = CsvSanitiser.SanitiseData(data, decimalPlaces, null, isCurrency, isPercentage, false);

            // Assert
            Assert.AreEqual("\"£290.599\"", result);
        }

        [TestMethod]
        public void ShouldSanitiseCurrencyDataAsStringWithDelimiter()
        {
            // Arrange
            var data = "100.5987";
            var decimalPlaces = DecimalPlaces.Two;
            var isCurrency = true;

            // Act
            var result = CsvSanitiser.SanitiseData(data, decimalPlaces, null, isCurrency);

            // Assert
            Assert.AreEqual("\"£100.60\",", result);
        }

        [TestMethod]
        public void ShouldSanitisePercentageDataWithDelimiter()
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
        public void ShouldSanitisePercentageDataWithoutDelimiter()
        {
            // Arrange
            var data = 83.456m;
            var decimalPlaces = DecimalPlaces.Two;
            var isCurrency = false;
            var isPercentage = true;

            // Act
            var result = CsvSanitiser.SanitiseData(data, decimalPlaces, null, isCurrency, isPercentage, false);

            // Assert
            Assert.AreEqual("\"83.46%\"", result);
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
        public void ShouldPrefixLrm_WhenFlagTrue_WithDelimiter()
        {
            // Arrange
            var data = "+ve";

            // Act
            var result = CsvSanitiser.SanitiseData(
                data,
                csvDelimiterRequired: true,
                appendLrmCharacterToPreventRenderedAsFormula: true);

            // Assert
            Assert.AreEqual("\"\u200E+ve\",", result);
        }

        [TestMethod]
        public void ShouldPrefixLrm_WhenFlagTrue_WithoutDelimiter()
        {
            // Arrange
            var data = "-ve";

            // Act
            var result = CsvSanitiser.SanitiseData(
                data,
                csvDelimiterRequired: false,
                appendLrmCharacterToPreventRenderedAsFormula: true);

            // Assert
            Assert.AreEqual("\"\u200E-ve\"", result);
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
        public void ShouldNotPrefixLrm_WhenValueIsNull_ReturnsDelimiterCharacterOnly()
        {
            string? data = null;

            var result = CsvSanitiser.SanitiseData(
                data,
                csvDelimiterRequired: true,
                appendLrmCharacterToPreventRenderedAsFormula: true);

            Assert.AreEqual(",", result);
        }

        [TestMethod]
        public void ShouldNotPrefixLrm_WhenValueIsHyphen_WithDelimiter()
        {
            // Arrange
            var data = "-";

            // Act
            var result = CsvSanitiser.SanitiseData(
                data,
                csvDelimiterRequired: true,
                appendLrmCharacterToPreventRenderedAsFormula: true);

            // Assert
            Assert.AreEqual("\"-\",", result);
        }

        [TestMethod]
        public void ShouldNotPrefixLrm_WhenValueIsHyphen_WithoutDelimiter()
        {
            // Arrange
            var data = "-";

            // Act
            var result = CsvSanitiser.SanitiseData(
                data,
                csvDelimiterRequired: false,
                appendLrmCharacterToPreventRenderedAsFormula: true);

            // Assert
            Assert.AreEqual("\"-\"", result);
        }
    }
}