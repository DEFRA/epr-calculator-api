using AutoFixture;
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
        private readonly CalculatorRunStatusDataValidator calculatorRunStatusDataValidatorUnderTest = new();

        public CalculatorRunStatusDataValidatorTest()
        {
            this.Fixture = new Fixture();
        }

        private Fixture Fixture { get; init; }

        [DataTestMethod]
        [DataRow(RunClassification.INITIAL_RUN)]
        [DataRow(RunClassification.INTERIM_RECALCULATION_RUN)]
        [DataRow(RunClassification.FINAL_RECALCULATION_RUN)]
        [DataRow(RunClassification.FINAL_RUN)]
        public void ValidateMethod_ShouldReturnIsInvalidAsTrue_WhenRunIsAlreadyCompleted(
          RunClassification runClassification)
        {
            // Arrange
            var calculatorRun = new CalculatorRun
            {
                Financial_Year = new CalculatorRunFinancialYear
                {
                    Name = "Name",
                },
                Name = "Name",
                CalculatorRunClassificationId = (int)runClassification,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)RunClassification.INITIAL_RUN,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = this.calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsTrue(genericValidationResultDto.IsInvalid);
        }

        [DataTestMethod]
        [DataRow(RunClassification.INITIAL_RUN)]
        [DataRow(RunClassification.INTERIM_RECALCULATION_RUN)]
        [DataRow(RunClassification.FINAL_RECALCULATION_RUN)]
        [DataRow(RunClassification.FINAL_RUN)]
        [DataRow(RunClassification.TEST_RUN)]
        public void ValidateMethod_ShouldReturnIsInvalidAsFalse_WhenItsValidDesignatedRunAndExistingRunIsUnClassified(
            RunClassification runClassification)
        {
            // Arrange
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
                ClassificationId = (int)runClassification,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = this.calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsFalse(genericValidationResultDto.IsInvalid);
        }

        [DataTestMethod]
        [DataRow(RunClassification.INITIAL_RUN, RunClassification.INITIAL_RUN_COMPLETED)]
        [DataRow(RunClassification.INTERIM_RECALCULATION_RUN, RunClassification.INITIAL_RUN_COMPLETED)]
        [DataRow(RunClassification.FINAL_RECALCULATION_RUN, RunClassification.FINAL_RECALCULATION_RUN_COMPLETED)]
        [DataRow(RunClassification.FINAL_RUN, RunClassification.FINAL_RUN_COMPLETED)]
        public void ValidateMethod_ShouldReturnIsInvalidAsFalse_WhenItsValidDesignatedCompletedRunAndExistingRunIsCorrectUnCompletedRun(
          RunClassification runClassificationFrom,
          RunClassification runClassificationTo)
        {
            // Arrange
            var calculatorRun = new CalculatorRun
            {
                Financial_Year = new CalculatorRunFinancialYear
                {
                    Name = "Name",
                },
                Name = "Name",
                CalculatorRunClassificationId = (int)runClassificationFrom,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)runClassificationTo,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = this.calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsFalse(genericValidationResultDto.IsInvalid);
        }

        [DataTestMethod]
        [DataRow(RunClassification.INITIAL_RUN)]
        [DataRow(RunClassification.INTERIM_RECALCULATION_RUN)]
        [DataRow(RunClassification.FINAL_RECALCULATION_RUN)]
        [DataRow(RunClassification.FINAL_RUN)]
        public void ValidateMethod_ShouldReturnIsInvalidAsTrue_WhenItsInValidDesignatedRunAndAndExistingRunIsRunning(
            RunClassification runClassification)
        {
            // Arrange
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
                ClassificationId = (int)runClassification,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = this.calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsTrue(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        public void ValidateMethod_ShouldReturnIsInvalidAsTrue_WhenItsDeletedRunAndExistingRunIsAlreadyMarkedAsDeleted()
        {
            // Arrange
            var calculatorRun = new CalculatorRun
            {
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

            // Act
            GenericValidationResultDto genericValidationResultDto = this.calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsTrue(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        public void ValidateMethod_ShouldReturnIsInvalidAsFalse_WhenItsDeletedRunAndExistingRunIsIsUnClassified()
        {
            // Arrange
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
                ClassificationId = (int)RunClassification.DELETED,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = this.calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsFalse(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        public void ValidateMethod_ShouldReturnIsInvalidAsTrue_WhenItsTestRunAndExistingRunIsAlreadyMarkedAsTest()
        {
            // Arrange
            var calculatorRun = new CalculatorRun
            {
                Financial_Year = new CalculatorRunFinancialYear
                {
                    Name = "Name",
                },
                Name = "Name",
                CalculatorRunClassificationId = (int)RunClassification.TEST_RUN,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)RunClassification.TEST_RUN,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = this.calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsTrue(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        public void ValidateMethod_ShouldReturnIsInvalidAsFalse_WhenItsTestRunAndExistingRunIsIsUnClassified()
        {
            // Arrange
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
                ClassificationId = (int)RunClassification.TEST_RUN,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = this.calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsFalse(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        public void ValidateMethod_ShouldReturnIsInvalidAsTrue_WhenItsUnsupportedStatus()
        {
            // Arrange
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
                ClassificationId = (int)RunClassification.INTERIM_RECALCULATION_RUN,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = this.calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsTrue(genericValidationResultDto.IsInvalid);
        }
    }
}
