using EPR.Calculator.API.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class CommonUtilTests
    {
        [TestMethod]
        [DataRow("2024-25", "2023")]
        [DataRow("2023-24", "2022")]
        [DataRow("2022-23", "2021")]
        public void ConvertToCalendarYear_ValidFinancialYear_ReturnsExpectedCalendarYear(string financialYear, string expectedCalendarYear)
        {
            var result = CommonUtil.ConvertToCalendarYear(financialYear);

            Assert.AreEqual(expectedCalendarYear, result);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void ConvertToCalendarYear_NullOrEmptyFinancialYear_ThrowsArgumentException(string financialYear)
        {
            Assert.ThrowsException<ArgumentException>(() => CommonUtil.ConvertToCalendarYear(financialYear));
        }

        [TestMethod]
        [DataRow("2024")]
        [DataRow("24-25")]
        [DataRow("2024-2025")]
        [DataRow("abcd-efgh")]
        public void ConvertToCalendarYear_InvalidFormatFinancialYear_ThrowsFormatException(string financialYear)
        {
            Assert.ThrowsException<FormatException>(() => CommonUtil.ConvertToCalendarYear(financialYear));
        }
    }
}