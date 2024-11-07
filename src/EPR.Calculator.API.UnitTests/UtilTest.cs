using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class UtilTest
    {
        private string parameterType = "Paramter Type 1";
        private string parameterUniqueReferenceId = Guid.NewGuid().ToString();
        private string parameterCategory = "Parameter Category 1";

        [TestMethod]
        public void CreateErrorDtoTest()
        {
            DefaultParameterTemplateMaster template = new DefaultParameterTemplateMaster
            {
                ParameterType = this.parameterType,
                ParameterUniqueReferenceId = this.parameterUniqueReferenceId,
                ParameterCategory = this.parameterCategory
            };
            string errorMessage = "Some error message";
            var errorDto = Util.CreateErrorDto(template, errorMessage);
            Assert.IsNotNull(errorDto);
            Assert.AreEqual(errorDto.ParameterType, parameterType);
            Assert.AreEqual(errorDto.ParameterCategory, parameterCategory);
            Assert.AreEqual(errorDto.ParameterUniqueRef, parameterUniqueReferenceId);
        }

        [TestMethod]
        public void GetParameterValueTest_For_Non_Percent()
        {
            DefaultParameterTemplateMaster template = new DefaultParameterTemplateMaster
            {
                ParameterType = this.parameterType,
                ParameterUniqueReferenceId = this.parameterUniqueReferenceId,
                ParameterCategory = this.parameterCategory
            };
            var parameterValue = Util.GetParameterValue(template, "£100");
            Assert.IsNotNull(parameterValue);
            Assert.AreEqual(parameterValue, "100");
        }

        [TestMethod]
        public void GetParameterValueTest_For_Percent()
        {
            var parameterType = "Paramter Type 1 percent";
            var parameterUniqueReferenceId = Guid.NewGuid().ToString();
            var parameterCategory = "Parameter Category 1";
            DefaultParameterTemplateMaster template = new DefaultParameterTemplateMaster
            {
                ParameterType = parameterType,
                ParameterUniqueReferenceId = parameterUniqueReferenceId,
                ParameterCategory = parameterCategory
            };
            var parameterValue = Util.GetParameterValue(template, "100%");
            Assert.IsNotNull(parameterValue);
            Assert.AreEqual(parameterValue, "100");
        }

        /// <summary>
        /// Tests that GetFinancialYearAsYYYY returns the first year from a valid financial year string.
        /// </summary>
        /// <param name="financialYear">The financial year string to parse.</param>
        /// <param name="expectedFinancialYear">The expected first year as a string.</param>
        [TestMethod]
        [DataRow("2024-25", "2024")]
        [DataRow("2023-24", "2023")]
        [DataRow("2022-23", "2022")]
        public void GetFinancialYearAsYYYY_ValidString_ShouldReturnFirstYear(string financialYear, string expectedFinancialYear)
        {
            var result = Util.GetFinancialYearAsYYYY(financialYear);
            Assert.AreEqual(expectedFinancialYear, result);
        }

        /// <summary>
        /// Tests that GetFinancialYearAsYYYY throws a FormatException for an invalid string.
        /// </summary>
        /// <param name="financialYear">The invalid financial year string to parse.</param>
        [TestMethod]
        [DataRow("2024")]
        [DataRow("24-25")]
        [DataRow("2024-2025")]
        [DataRow("abcd-efgh")]
        public void GetFinancialYearAsYYYY_InvalidString_ShouldThrowFormatException(string financialYear)
        {
            var exception = Assert.ThrowsException<FormatException>(() => Util.GetFinancialYearAsYYYY(financialYear));
            Assert.AreEqual("Financial year format is invalid. Expected format is 'YYYY-YY'.", exception.Message);
        }

        /// <summary>
        /// Tests that GetFinancialYearAsYYYY throws a ArgumentException for a null or empty string.
        /// </summary>
        /// <param name="financialYear">The null or empty financial year string to convert.</param>
        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void GetFinancialYearAsYYYY_NullOrEmptyString_ShouldThrowArgumentException(string financialYear)
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => Util.GetFinancialYearAsYYYY(financialYear));
            Assert.AreEqual("Financial year cannot be null or empty (Parameter 'value')", exception.Message);
        }

        /// <summary>
        /// Tests that GetCalendarYear returns the previous year as a string for a valid financial year string.
        /// </summary>
        /// <param name="financialYear">The financial year string to convert.</param>
        /// <param name="expectedCalendarYear">The expected previous calendar year as a string.</param>
        [TestMethod]
        [DataRow("2024-25", "2023")]
        [DataRow("2023-24", "2022")]
        [DataRow("2022-23", "2021")]
        public void GetCalendarYear_ValidString_ShouldReturnPreviousYearAsString(string financialYear, string expectedCalendarYear)
        {
            var result = Util.GetCalendarYear(financialYear);
            Assert.AreEqual(expectedCalendarYear, result);
        }

        /// <summary>
        /// Tests that GetCalendarYear throws an ArgumentException for a null or empty string.
        /// </summary>
        /// <param name="financialYear">The null or empty financial year string to convert.</param>
        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void GetCalendarYear_NullOrEmptyString_ShouldThrowArgumentException(string financialYear)
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => Util.GetCalendarYear(financialYear));
            Assert.AreEqual("Financial year cannot be null or empty (Parameter 'financialYear')", exception.Message);
        }
    }
}
