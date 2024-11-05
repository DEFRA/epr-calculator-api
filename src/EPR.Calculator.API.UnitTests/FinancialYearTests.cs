using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPR.Calculator.API.Utils;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class FinancialYearTests
    {
        [TestMethod]
        public void FinancialYear_Value_ShouldReturnCorrectDateTime()
        {
            var date = new DateTime(2024, 1, 1);
            var financialYear = new FinancialYear(date);

            var result = financialYear.Value;

            Assert.AreEqual(date, result);
        }

        [TestMethod]
        public void ToCalendarYear_ShouldReturnPreviousYear()
        {
            var date = new DateTime(2024, 1, 1);
            var financialYear = new FinancialYear(date);

            var result = financialYear.ToCalendarYear();

            Assert.AreEqual(new DateTime(2023, 1, 1), result);
        }

        [TestMethod]
        public void Parse_ValidString_ShouldReturnFinancialYear()
        {
            var input = "2024-25";

            var result = FinancialYear.Parse(input);

            Assert.AreEqual(new DateTime(2024, 1, 1), result.Value);
        }

        [TestMethod]
        [DataRow("2024-25", "2024")]
        [DataRow("2023-24", "2023")]
        [DataRow("2022-23", "2022")]
        public void FinancialYearAsString_ValidString_ShouldReturnFirstYear(string financialYear, string expectedFinancialYear)
        {
            var result = FinancialYear.FinancialYearAsString(financialYear);
            Assert.AreEqual(expectedFinancialYear, result);
        }

        [TestMethod]
        [DataRow("2024")]
        [DataRow("24-25")]
        [DataRow("2024-2025")]
        [DataRow("abcd-efgh")]
        public void FinancialYearAsString_InvalidString_ShouldThrowFormatException(string financialYear)
        {
            var exception = Assert.ThrowsException<FormatException>(() => FinancialYear.FinancialYearAsString(financialYear));
            Assert.AreEqual("Financial year format is invalid. Expected format is 'YYYY-YY'.", exception.Message);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void FinancialYearAsString_NullOrEmptyString_ShouldThrowArgumentException(string financialYear)
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => FinancialYear.FinancialYearAsString(financialYear));
            Assert.AreEqual("Financial year cannot be null or empty (Parameter 'value')", exception.Message);
        }

        [TestMethod]
        [DataRow("2024-25", "2023")]
        [DataRow("2023-24", "2022")]
        [DataRow("2022-23", "2021")]
        public void ToCalendarYearAsString_ValidString_ShouldReturnPreviousYearAsString(string financialYear, string expectedCalendarYear)
        {
            var result = FinancialYear.ToCalendarYearAsString(financialYear);
            Assert.AreEqual(expectedCalendarYear, result);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void ToCalendarYearAsString_NullOrEmptyString_ShouldThrowArgumentException(string financialYear)
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => FinancialYear.ToCalendarYearAsString(financialYear));
            Assert.AreEqual("Financial year cannot be null or empty (Parameter 'value')", exception.Message);
        }
    }
}