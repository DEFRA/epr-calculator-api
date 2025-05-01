using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Validators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests.Validator
{
    [TestClass]
    public class CalculatorRunStatusDataValidatorTest
    {
        private readonly CalculatorRunStatusDataValidator validator = new();

        [TestMethod]
        public void Validate_InitialRun_Test()
        {
            var calculatorRun = new CalculatorRun
            {
                Financial_Year = new CalculatorRunFinancialYear
                {
                    Name = "Name",
                },
                Name = "Name",
                CalculatorRunClassificationId = (int)RunClassification.UNCLASSIFIED,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)RunClassification.INITIAL_RUN,
                RunId = 1,
            };
            var vr = validator.Validate(calculatorRun, runStatusUpdateDto);
            Assert.IsNotNull(vr);
            Assert.IsFalse(vr.IsInvalid);
        }

        [TestMethod]
        public void Validate_InitialRun_Invalid_Test()
        {
            var calculatorRun = new CalculatorRun
            {
                Financial_Year = new CalculatorRunFinancialYear
                {
                    Name = "Name",
                },
                Name = "Name",
                CalculatorRunClassificationId = (int)RunClassification.RUNNING,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)RunClassification.INITIAL_RUN,
                RunId = 1,
            };
            var vr = validator.Validate(calculatorRun, runStatusUpdateDto);
            Assert.IsNotNull(vr);
            Assert.IsTrue(vr.IsInvalid);
        }

        [TestMethod]
        public void Validate_InitialRunComplete_Test()
        {
            var calculatorRun = new CalculatorRun
            {
                HasBillingFileGenerated = true,
                Financial_Year = new CalculatorRunFinancialYear
                {
                    Name = "Name",
                },
                Name = "Name",
                CalculatorRunClassificationId = (int)RunClassification.INITIAL_RUN,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)RunClassification.INITIAL_RUN_COMPLETED,
                RunId = 1,
            };
            var vr = validator.Validate(calculatorRun, runStatusUpdateDto);
            Assert.IsNotNull(vr);
            Assert.IsFalse(vr.IsInvalid);
        }

        [TestMethod]
        public void Validate_InitialRunComplete_Invalid_Test()
        {
            var calculatorRun = new CalculatorRun
            {
                HasBillingFileGenerated = false,
                Financial_Year = new CalculatorRunFinancialYear
                {
                    Name = "Name",
                },
                Name = "Name",
                CalculatorRunClassificationId = (int)RunClassification.INITIAL_RUN,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)RunClassification.INITIAL_RUN_COMPLETED,
                RunId = 1,
            };
            var vr = validator.Validate(calculatorRun, runStatusUpdateDto);
            Assert.IsNotNull(vr);
            Assert.IsTrue(vr.IsInvalid);
        }

        [TestMethod]
        public void Validate_Delete_Test()
        {
            var calculatorRun = new CalculatorRun
            {
                HasBillingFileGenerated = false,
                Financial_Year = new CalculatorRunFinancialYear
                {
                    Name = "Name",
                },
                Name = "Name",
                CalculatorRunClassificationId = (int)RunClassification.INITIAL_RUN,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)RunClassification.DELETED,
                RunId = 1,
            };
            var vr = validator.Validate(calculatorRun, runStatusUpdateDto);
            Assert.IsNotNull(vr);
            Assert.IsFalse(vr.IsInvalid);
        }

        [TestMethod]
        public void Validate_Delete_Invalid_Test()
        {
            var calculatorRun = new CalculatorRun
            {
                HasBillingFileGenerated = false,
                Financial_Year = new CalculatorRunFinancialYear
                {
                    Name = "Name",
                },
                Name = "Name",
                CalculatorRunClassificationId = (int)RunClassification.DELETED,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)RunClassification.DELETED,
                RunId = 1,
            };
            var vr = validator.Validate(calculatorRun, runStatusUpdateDto);
            Assert.IsNotNull(vr);
            Assert.IsTrue(vr.IsInvalid);
        }

        [TestMethod]
        public void Validate_Unsupported_Status_Test()
        {
            var calculatorRun = new CalculatorRun
            {
                HasBillingFileGenerated = false,
                Financial_Year = new CalculatorRunFinancialYear
                {
                    Name = "Name",
                },
                Name = "Name",
                CalculatorRunClassificationId = (int)RunClassification.RUNNING,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)RunClassification.INTERIM_RECALCULATION_RUN,
                RunId = 1,
            };
            var vr = validator.Validate(calculatorRun, runStatusUpdateDto);
            Assert.IsNotNull(vr);
            Assert.IsTrue(vr.IsInvalid);
        }
    }
}
