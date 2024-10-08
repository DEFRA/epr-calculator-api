using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Mappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class CalculatorRunsMapperTests
    {
        [TestMethod]
        public void Map_ShouldReturnCorrectDto_WhenGivenValidCalculatorRun()
        {
            // Arrange
            var calculatorRun = new CalculatorRun
            {
                Id = 1,
                Name = "Test Run",
                Financial_Year = "2023",
                CreatedAt = DateTime.Now,
                CalculatorRunClassificationId = 2,
                CreatedBy = "User1"
            };

            // Act
            var result = CalculatorRunsMapper.Map(calculatorRun);

            // Assert
            Assert.AreEqual(calculatorRun.Id, result?.Id);
            Assert.AreEqual(calculatorRun.Name, result?.CalculatorRunName);
            Assert.AreEqual(calculatorRun.Financial_Year, result?.FinancialYear);
            Assert.AreEqual(calculatorRun.CreatedAt, result?.CreatedAt);
            Assert.AreEqual(calculatorRun.CalculatorRunClassificationId, result?.RunClassificationId);
            Assert.AreEqual(calculatorRun.CreatedBy, result?.CreatedBy);
        }

        [TestMethod]
        public void Map_ShouldReturnNull_WhenGivenNullCalculatorRun()
        {
            // Act
            var result = CalculatorRunsMapper.Map(null);

            // Assert
            Assert.IsNull(result);
        }
    }
}