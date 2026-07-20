using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.UnitTests.TestHelpers.Fixtures;
using EPR.Calculator.API.Validators;

namespace EPR.Calculator.API.UnitTests.Validator
{
    [TestClass]
    public class CalculatorRunStatusDataValidatorTest
    {
        private readonly CalculatorRunStatusDataValidator calculatorRunStatusDataValidatorUnderTest = new();

        public CalculatorRunStatusDataValidatorTest()
        {
            Fixture = TestFixtures.New();
        }

        private IFixture Fixture { get; init; }

        [TestMethod]
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
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int)runClassification,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)RunClassification.INITIAL_RUN,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsTrue(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
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
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int)RunClassification.UNCLASSIFIED,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)runClassification,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsFalse(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        [DataRow(RunClassification.INITIAL_RUN, RunClassification.INITIAL_RUN_COMPLETED)]
        [DataRow(RunClassification.INTERIM_RECALCULATION_RUN, RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED)]
        [DataRow(RunClassification.FINAL_RECALCULATION_RUN, RunClassification.FINAL_RECALCULATION_RUN_COMPLETED)]
        [DataRow(RunClassification.FINAL_RUN, RunClassification.FINAL_RUN_COMPLETED)]
        public void ValidateMethod_ShouldReturnIsInvalidAsFalse_WhenItsValidDesignatedCompletedRunAndExistingRunIsCorrectUnCompletedRun(
          RunClassification runClassificationFrom,
          RunClassification runClassificationTo)
        {
            // Arrange
            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int)runClassificationFrom,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)runClassificationTo,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsFalse(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
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
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int)RunClassification.RUNNING,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)runClassification,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsTrue(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        [DataRow(RunClassification.INTHEQUEUE)]
        [DataRow(RunClassification.DELETED)]
        public void ValidateMethod_ShouldReturnIsInvalidAsTrue_WhenItsDeletedRunAndExistingRunStatusIsNotToMarkAsDeleted(
            RunClassification runClassification)
        {
            // Arrange
            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int)runClassification,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)RunClassification.DELETED,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsTrue(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        [DataRow(RunClassification.UNCLASSIFIED)]
        [DataRow(RunClassification.ERROR)]
        [DataRow(RunClassification.INITIAL_RUN)]
        [DataRow(RunClassification.INTERIM_RECALCULATION_RUN)]
        [DataRow(RunClassification.FINAL_RECALCULATION_RUN)]
        [DataRow(RunClassification.FINAL_RUN)]
        [DataRow(RunClassification.TEST_RUN)]
        [DataRow(RunClassification.RUNNING)]
        public void ValidateMethod_ShouldReturnIsInvalidAsFalse_WhenItsDeletedRunAndExistingRunStatusIsValidToMarkAsDeleted(
            RunClassification runClassification)
        {
            // Arrange
            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int)runClassification,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)RunClassification.DELETED,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsFalse(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        [DataRow(RunClassification.TEST_RUN)]
        [DataRow(RunClassification.INTHEQUEUE)]
        [DataRow(RunClassification.RUNNING)]
        [DataRow(RunClassification.ERROR)]
        [DataRow(RunClassification.DELETED)]
        public void ValidateMethod_ShouldReturnIsInvalidAsTrue_WhenItsTestRunAndExistingRunIsAlreadyMarkedAsTestOrOthers(
            RunClassification runClassification)
        {
            // Arrange
            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int)runClassification,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)RunClassification.TEST_RUN,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsTrue(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        [DataRow(RunClassification.UNCLASSIFIED)]
        [DataRow(RunClassification.INITIAL_RUN)]
        [DataRow(RunClassification.INTERIM_RECALCULATION_RUN)]
        [DataRow(RunClassification.FINAL_RECALCULATION_RUN)]
        [DataRow(RunClassification.FINAL_RUN)]
        public void ValidateMethod_ShouldReturnIsInvalidAsFalse_WhenItsTestRunExistingRunStatusIsValidToMarkAsTestRun(
            RunClassification runClassification)
        {
            // Arrange
            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int)runClassification,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)RunClassification.TEST_RUN,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsFalse(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        [DataRow(RunClassification.RUNNING)]
        [DataRow(RunClassification.INTHEQUEUE)]
        [DataRow(RunClassification.ERROR)]
        [DataRow(RunClassification.UNCLASSIFIED)]
        public void ValidateMethod_ShouldReturnIsInvalidAsTrue_WhenClassificationIdIsNotARecognisedTransitionTarget(
            RunClassification runClassification)
        {
            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int)RunClassification.UNCLASSIFIED,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)runClassification,
                RunId = 1,
            };

            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsTrue(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        public void ValidateMethod_ShouldReturnIsInvalidAsTrue_WhenItsUnsupportedStatus()
        {
            // Arrange
            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int)RunClassification.RUNNING,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)RunClassification.INTERIM_RECALCULATION_RUN,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsTrue(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        public void ValidateWithDesignatedRunsMethod_ShouldReturnIsInvalidAsFalse_WhenNoDesignatedRunPerformed()
        {
            // Arrange
            List<CalculatorRunDto> designatedRuns = [];
            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int)RunClassification.RUNNING,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)RunClassification.INITIAL_RUN,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(
                designatedRuns,
                calculatorRun,
                runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsFalse(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        [DataRow(RunClassification.INITIAL_RUN, RunClassification.INITIAL_RUN)]
        [DataRow(RunClassification.INTERIM_RECALCULATION_RUN, RunClassification.INTERIM_RECALCULATION_RUN)]
        [DataRow(RunClassification.FINAL_RECALCULATION_RUN, RunClassification.INTERIM_RECALCULATION_RUN)]
        [DataRow(RunClassification.FINAL_RUN, RunClassification.INTERIM_RECALCULATION_RUN)]
        [DataRow(RunClassification.FINAL_RECALCULATION_RUN, RunClassification.FINAL_RECALCULATION_RUN)]
        [DataRow(RunClassification.INTERIM_RECALCULATION_RUN, RunClassification.FINAL_RECALCULATION_RUN)]
        [DataRow(RunClassification.FINAL_RUN, RunClassification.FINAL_RECALCULATION_RUN)]
        [DataRow(RunClassification.FINAL_RUN, RunClassification.FINAL_RUN)]
        [DataRow(RunClassification.INTERIM_RECALCULATION_RUN, RunClassification.FINAL_RUN)]
        [DataRow(RunClassification.FINAL_RECALCULATION_RUN, RunClassification.FINAL_RUN)]
        public void ValidateWithDesignatedRunsMethod_ShouldReturnIsInvalidAsTrue_WhenAnotherRunIsMarkedAsDesignatedRunButNotCompleted(
            RunClassification existingRunClassification,
            RunClassification requestRunClassification)
        {
            // Arrange
            List<CalculatorRunDto> designatedRuns = [];
            designatedRuns.Add(CreateDto(101, existingRunClassification));

            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int)RunClassification.UNCLASSIFIED,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)requestRunClassification,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(
                designatedRuns,
                calculatorRun,
                runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsTrue(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        [DataRow(RunClassification.INITIAL_RUN)]
        [DataRow(RunClassification.INITIAL_RUN_COMPLETED)]
        public void ValidateWithDesignatedRunsMethod_ShouldReturnIsInvalidAsTrue_WhenAnotherRunIsMarkedAsInitialRunCompleted(
            RunClassification requestRunClassification)
        {
            // Arrange
            List<CalculatorRunDto> designatedRuns = [];
            designatedRuns.Add(CreateDto(101, RunClassification.INITIAL_RUN_COMPLETED));

            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int)RunClassification.UNCLASSIFIED,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)requestRunClassification,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(
                designatedRuns,
                calculatorRun,
                runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsTrue(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        [DataRow(RunClassification.FINAL_RECALCULATION_RUN)]
        [DataRow(RunClassification.FINAL_RECALCULATION_RUN_COMPLETED)]
        public void ValidateWithDesignatedRunsMethod_ShouldReturnIsInvalidAsTrue_WhenAnotherRunIsMarkedAsFinalRecalculationRunCompleted(
           RunClassification requestRunClassification)
        {
            // Arrange
            List<CalculatorRunDto> designatedRuns = [];
            designatedRuns.Add(CreateDto(101, RunClassification.FINAL_RECALCULATION_RUN_COMPLETED));
            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int)RunClassification.UNCLASSIFIED,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)requestRunClassification,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(
                designatedRuns,
                calculatorRun,
                runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsTrue(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        [DataRow(RunClassification.FINAL_RECALCULATION_RUN)]
        [DataRow(RunClassification.FINAL_RECALCULATION_RUN_COMPLETED)]
        [DataRow(RunClassification.FINAL_RUN)]
        [DataRow(RunClassification.FINAL_RUN_COMPLETED)]
        public void ValidateWithDesignatedRunsMethod_ShouldReturnIsInvalidAsTrue_WhenAnotherRunIsMarkedAsFinalRunCompleted(
          RunClassification requestRunClassification)
        {
            // Arrange
            List<CalculatorRunDto> designatedRuns = [];
            designatedRuns.Add(CreateDto(101, RunClassification.FINAL_RUN_COMPLETED));

            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int)RunClassification.UNCLASSIFIED,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)requestRunClassification,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(
                designatedRuns,
                calculatorRun,
                runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsTrue(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        [DataRow(RunClassification.INTERIM_RECALCULATION_RUN)]
        [DataRow(RunClassification.FINAL_RECALCULATION_RUN)]
        public void ValidateWithDesignatedRunsMethod_ShouldReturnIsInvalidAsTrue_WhenSystemDontHaveInitialRunCompleted(
          RunClassification requestRunClassification)
        {
            // Arrange
            List<CalculatorRunDto> designatedRuns = [];
            designatedRuns.Add(CreateDto(101, RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED));

            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int)RunClassification.UNCLASSIFIED,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)requestRunClassification,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(
                designatedRuns,
                calculatorRun,
                runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsTrue(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        public void ValidateWithDesignatedRunsMethod_ShouldReturnIsInvalidAsTrue_WhenSystemDontHaveValidRunsToPerformFinalRun()
        {
            // Arrange
            List<CalculatorRunDto> designatedRuns = [];
            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int)RunClassification.UNCLASSIFIED,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)RunClassification.FINAL_RUN,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(
                designatedRuns,
                calculatorRun,
                runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsTrue(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        [DataRow(false, false, false)]
        [DataRow(true, false, false)]
        [DataRow(true, true, false)]
        [DataRow(true, true, true)]
        [DataRow(true, false, true)]
        public void ValidateWithDesignatedRunsMethod_ShouldReturnIsInvalidAsFalse_WhenSystemHaveCorrectDesignatedRunsToPerformInterimRecalculationRun(
          bool haveInterimRecalculationRunCompleted,
          bool haveFinalRecalculationRunCompleted,
          bool haveFinalRunCompleted)
        {
            // Arrange
            List<CalculatorRunDto> designatedRuns = [];
            designatedRuns.Add(CreateDto(101, RunClassification.INITIAL_RUN_COMPLETED));

            if (haveInterimRecalculationRunCompleted)
                designatedRuns.Add(CreateDto(102, RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED));

            if (haveFinalRecalculationRunCompleted)
                designatedRuns.Add(CreateDto(103, RunClassification.FINAL_RECALCULATION_RUN_COMPLETED));

            if (haveFinalRunCompleted)
                designatedRuns.Add(CreateDto(104, RunClassification.FINAL_RUN_COMPLETED));

            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int)RunClassification.UNCLASSIFIED,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)RunClassification.INTERIM_RECALCULATION_RUN,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(
                designatedRuns,
                calculatorRun,
                runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsFalse(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void ValidateWithDesignatedRunsMethod_ShouldReturnIsInvalidAsFalse_WhenSystemHaveCorrectDesignatedRunsToPerformFinalRecalculationRun(
            bool haveInterimRecalculationRunCompleted)
        {
            // Arrange
            List<CalculatorRunDto> designatedRuns = [];
            designatedRuns.Add(CreateDto(101, RunClassification.INITIAL_RUN_COMPLETED));

            if (haveInterimRecalculationRunCompleted)
                designatedRuns.Add(CreateDto(102, RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED));

            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int)RunClassification.UNCLASSIFIED,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)RunClassification.FINAL_RECALCULATION_RUN,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(
                designatedRuns,
                calculatorRun,
                runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsFalse(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        [DataRow(true, false)]
        [DataRow(true, true)]
        [DataRow(false, true)]
        public void ValidateWithDesignatedRunsMethod_ShouldReturnIsInvalidAsFalse_WhenSystemHaveCorrectDesignatedRunsToPerformInterimFinalRun(
            bool haveInterimRecalculationRunCompleted,
            bool haveFinalRecalculationRunCompleted)
        {
            // Arrange
            List<CalculatorRunDto> designatedRuns = [];
            designatedRuns.Add(CreateDto(101, RunClassification.INITIAL_RUN_COMPLETED));

            if (haveInterimRecalculationRunCompleted)
                designatedRuns.Add(CreateDto(102, RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED));

            if (haveFinalRecalculationRunCompleted)
                designatedRuns.Add(CreateDto(103, RunClassification.FINAL_RECALCULATION_RUN_COMPLETED));

            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int)RunClassification.UNCLASSIFIED,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)RunClassification.FINAL_RUN,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(
                designatedRuns,
                calculatorRun,
                runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsFalse(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        [DataRow(false, false, false)]
        [DataRow(true, false, false)]
        [DataRow(true, true, false)]
        [DataRow(true, true, true)]
        [DataRow(true, false, true)]
        public void ValidateWithDesignatedRunsMethod_ShouldReturnIsInvalidAsTrue_WhenCurrentRunIsOlderRunAndCantBeClassified(
            bool haveInterimRecalculationRunCompleted,
            bool haveFinalRecalculationRunCompleted,
            bool haveFinalRunCompleted)
        {
            // Arrange
            List<CalculatorRunDto> designatedRuns = [];
            designatedRuns.Add(CreateDto(101, RunClassification.INITIAL_RUN_COMPLETED));

            if (haveInterimRecalculationRunCompleted)
                designatedRuns.Add(CreateDto(102, RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED));

            if (haveFinalRecalculationRunCompleted)
                designatedRuns.Add(CreateDto(103, RunClassification.FINAL_RECALCULATION_RUN_COMPLETED));

            if (haveFinalRunCompleted)
                designatedRuns.Add(CreateDto(104, RunClassification.FINAL_RUN_COMPLETED));

            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CreatedAt = DateTime.UtcNow.AddDays(-2), // Older run
                CalculatorRunClassificationId = (int)RunClassification.UNCLASSIFIED,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                ClassificationId = (int)RunClassification.INTERIM_RECALCULATION_RUN,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(
                designatedRuns,
                calculatorRun,
                runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsTrue(genericValidationResultDto.IsInvalid);
        }

        private static CalculatorRunDto CreateDto(int runId, RunClassification classification)
        {
            var hasBeenSentToFss = classification
                is RunClassification.INITIAL_RUN_COMPLETED
                or RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED
                or RunClassification.FINAL_RECALCULATION_RUN_COMPLETED;

            return new CalculatorRunDto
            {
                RunId = runId,
                RunName = $"My - {classification}",
                RunClassification = classification,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                RelativeYear = new RelativeYear(2024),
                CreatedBy = "ignored",
                UpdatedBy = "ignored",
                BillingRunStatus = hasBeenSentToFss ? BillingRunStatus.Completed : BillingRunStatus.None,
                BillingRunStartedAt = hasBeenSentToFss ? DateTime.UtcNow.AddHours(-1) : null,
                BillingFile = !hasBeenSentToFss
                    ? null
                    : new CalculatorRunDto.BillingFileDto
                    {
                        Id = runId,
                        IsLatest = false,
                        HasBeenSentToFss = hasBeenSentToFss,
                        CsvFileName = "test.csv",
                        JsonFileName = "test.json",
                        CreatedAt = DateTime.UtcNow.AddDays(-1),
                        CreatedBy = "ignored",
                        SentAt = DateTime.UtcNow.AddDays(-1),
                        SentBy = "ignored"
                    }
            };
        }
    }
}
