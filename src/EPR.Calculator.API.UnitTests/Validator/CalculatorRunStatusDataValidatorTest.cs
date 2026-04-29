using AutoFixture;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Validators;

namespace EPR.Calculator.API.UnitTests.Validator
{
    [TestClass]
    public class CalculatorRunStatusDataValidatorTest
    {
        private readonly CalculatorRunStatusDataValidator calculatorRunStatusDataValidatorUnderTest = new();

        public CalculatorRunStatusDataValidatorTest()
        {
            Fixture = new Fixture();
        }

        private Fixture Fixture { get; init; }

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
                CalculatorRunClassificationId = (int) runClassification,
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
                CalculatorRunClassificationId = (int) RunClassification.UNCLASSIFIED,
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
                CalculatorRunClassificationId = (int) runClassificationFrom,
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
                CalculatorRunClassificationId = (int) RunClassification.RUNNING,
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
                CalculatorRunClassificationId = (int) runClassification,
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
                CalculatorRunClassificationId = (int) runClassification,
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
                CalculatorRunClassificationId = (int) runClassification,
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
                CalculatorRunClassificationId = (int) runClassification,
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
                CalculatorRunClassificationId = (int) RunClassification.RUNNING,
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
                CalculatorRunClassificationId = (int) RunClassification.RUNNING,
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
            designatedRuns.Add(new CalculatorRunDto
            {
                RunId = 101,
                RunName = $"My - {existingRunClassification}",
                RunClassification = existingRunClassification,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                RelativeYear = new RelativeYear(2024),
                CreatedBy = "Test User",
                UpdatedBy = "Test User",
                BillingRunStatus = BillingRunStatus.Unknown,
                BillingRunStartedAt = null,
                CompletedBillingRun = null
            });
            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int) RunClassification.UNCLASSIFIED,
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
            designatedRuns.Add(new CalculatorRunDto
            {
                RunId = 101,
                RunName = $"My - {RunClassification.INITIAL_RUN_COMPLETED}",
                RunClassification = RunClassification.INITIAL_RUN_COMPLETED,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                RelativeYear = new RelativeYear(2024),
                CreatedBy = "Test User",
                UpdatedBy = "Test User",
                BillingRunStatus = BillingRunStatus.Unknown,
                BillingRunStartedAt = null,
                CompletedBillingRun = null
            });
            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int) RunClassification.UNCLASSIFIED,
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
            designatedRuns.Add(new CalculatorRunDto
            {
                RunId = 101,
                RunName = $"My - {RunClassification.FINAL_RECALCULATION_RUN_COMPLETED}",
                RunClassification = RunClassification.FINAL_RECALCULATION_RUN_COMPLETED,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                RelativeYear = new RelativeYear(2024),
                CreatedBy = "Test User",
                UpdatedBy = "Test User",
                BillingRunStatus = BillingRunStatus.Unknown,
                BillingRunStartedAt = null,
                CompletedBillingRun = null
            });
            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int) RunClassification.UNCLASSIFIED,
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
            designatedRuns.Add(new CalculatorRunDto
            {
                RunId = 101,
                RunName = $"My - {RunClassification.FINAL_RUN_COMPLETED}",
                RunClassification = RunClassification.FINAL_RUN_COMPLETED,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                RelativeYear = new RelativeYear(2024),
                CreatedBy = "Test User",
                UpdatedBy = "Test User",
                BillingRunStatus = BillingRunStatus.Unknown,
                BillingRunStartedAt = null,
                CompletedBillingRun = null
            });
            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int) RunClassification.UNCLASSIFIED,
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
            designatedRuns.Add(new CalculatorRunDto
            {
                RunId = 101,
                RunName = $"My - {RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED}",
                RunClassification = RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                RelativeYear = new RelativeYear(2024),
                CreatedBy = "Test User",
                UpdatedBy = "Test User",
                BillingRunStatus = BillingRunStatus.Unknown,
                BillingRunStartedAt = null,
                CompletedBillingRun = null
            });
            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int) RunClassification.UNCLASSIFIED,
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
                CalculatorRunClassificationId = (int) RunClassification.UNCLASSIFIED,
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
            var completedBillingRun = new CalculatorRunDto.BillingMetadataDto
            {
                CsvFileName = "test.csv",
                JsonFileName = "test.json",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                CreatedBy = "Test User",
                AuthorisedAt = DateTime.UtcNow.AddDays(-1),
                AuthorisedBy = "Test User"
            };

            List<CalculatorRunDto> designatedRuns = [];
            designatedRuns.Add(new CalculatorRunDto
            {
                RunId = 101,
                RunName = $"My - {RunClassification.INITIAL_RUN_COMPLETED}",
                RunClassification = RunClassification.INITIAL_RUN_COMPLETED,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                RelativeYear = new RelativeYear(2024),
                CreatedBy = "Test User",
                UpdatedBy = "Test User",
                BillingRunStatus = BillingRunStatus.Unknown,
                BillingRunStartedAt = DateTime.UtcNow.AddDays(-1),
                CompletedBillingRun = completedBillingRun
            });

            if (haveInterimRecalculationRunCompleted)
            {
                designatedRuns.Add(new CalculatorRunDto
                {
                    RunId = 102,
                    RunName = $"My - {RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED}",
                    RunClassification = RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    RelativeYear = new RelativeYear(2024),
                    CreatedBy = "Test User",
                    UpdatedBy = "Test User",
                    BillingRunStatus = BillingRunStatus.Unknown,
                    BillingRunStartedAt = DateTime.UtcNow.AddDays(-1),
                    CompletedBillingRun = completedBillingRun
                });
            }

            if (haveFinalRecalculationRunCompleted)
            {
                designatedRuns.Add(new CalculatorRunDto
                {
                    RunId = 102,
                    RunName = $"My - {RunClassification.FINAL_RECALCULATION_RUN_COMPLETED}",
                    RunClassification = RunClassification.FINAL_RECALCULATION_RUN_COMPLETED,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    RelativeYear = new RelativeYear(2024),
                    CreatedBy = "Test User",
                    UpdatedBy = "Test User",
                    BillingRunStatus = BillingRunStatus.Unknown,
                    BillingRunStartedAt = DateTime.UtcNow.AddDays(-1),
                    CompletedBillingRun = completedBillingRun
                });
            }

            if (haveFinalRunCompleted)
            {
                designatedRuns.Add(new CalculatorRunDto
                {
                    RunId = 102,
                    RunName = $"My - {RunClassification.FINAL_RUN_COMPLETED}",
                    RunClassification = RunClassification.FINAL_RUN_COMPLETED,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    RelativeYear = new RelativeYear(2024),
                    CreatedBy = "Test User",
                    UpdatedBy = "Test User",
                    BillingRunStatus = BillingRunStatus.Unknown,
                    BillingRunStartedAt = DateTime.UtcNow.AddDays(-1),
                    CompletedBillingRun = completedBillingRun
                });
            }

            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int) RunClassification.UNCLASSIFIED,
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
            var completedBillingRun = new CalculatorRunDto.BillingMetadataDto
            {
                CsvFileName = "test.csv",
                JsonFileName = "test.json",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                CreatedBy = "Test User",
                AuthorisedAt = DateTime.UtcNow.AddDays(-1),
                AuthorisedBy = "Test User"
            };

            List<CalculatorRunDto> designatedRuns = [];
            designatedRuns.Add(new CalculatorRunDto
            {
                RunId = 101,
                RunName = $"My - {RunClassification.INITIAL_RUN_COMPLETED}",
                RunClassification = RunClassification.INITIAL_RUN_COMPLETED,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                RelativeYear = new RelativeYear(2024),
                CreatedBy = "Test User",
                UpdatedBy = "Test User",
                BillingRunStatus = BillingRunStatus.Unknown,
                BillingRunStartedAt = null,
                CompletedBillingRun = completedBillingRun
            });

            if (haveInterimRecalculationRunCompleted)
            {
                designatedRuns.Add(new CalculatorRunDto
                {
                    RunId = 102,
                    RunName = $"My - {RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED}",
                    RunClassification = RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    RelativeYear = new RelativeYear(2024),
                    CreatedBy = "Test User",
                    UpdatedBy = "Test User",
                    BillingRunStatus = BillingRunStatus.Unknown,
                    BillingRunStartedAt = null,
                    CompletedBillingRun = completedBillingRun
                });
            }

            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int) RunClassification.UNCLASSIFIED,
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
            var completedBillingRun = new CalculatorRunDto.BillingMetadataDto
            {
                CsvFileName = "test.csv",
                JsonFileName = "test.json",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                CreatedBy = "Test User",
                AuthorisedAt = DateTime.UtcNow.AddDays(-1),
                AuthorisedBy = "Test User"
            };

            List<CalculatorRunDto> designatedRuns = [];
            designatedRuns.Add(new CalculatorRunDto
            {
                RunId = 101,
                RunName = $"My - {RunClassification.INITIAL_RUN_COMPLETED}",
                RunClassification = RunClassification.INITIAL_RUN_COMPLETED,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                RelativeYear = new RelativeYear(2024),
                CreatedBy = "Test User",
                UpdatedBy = "Test User",
                BillingRunStatus = BillingRunStatus.Unknown,
                BillingRunStartedAt = null,
                CompletedBillingRun = completedBillingRun
            });

            if (haveInterimRecalculationRunCompleted)
            {
                designatedRuns.Add(new CalculatorRunDto
                {
                    RunId = 102,
                    RunName = $"My - {RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED}",
                    RunClassification = RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    RelativeYear = new RelativeYear(2024),
                    CreatedBy = "Test User",
                    UpdatedBy = "Test User",
                    BillingRunStatus = BillingRunStatus.Unknown,
                    BillingRunStartedAt = null,
                    CompletedBillingRun = completedBillingRun
                });
            }

            if (haveFinalRecalculationRunCompleted)
            {
                designatedRuns.Add(new CalculatorRunDto
                {
                    RunId = 103,
                    RunName = $"My - {RunClassification.FINAL_RECALCULATION_RUN_COMPLETED}",
                    RunClassification = RunClassification.FINAL_RECALCULATION_RUN_COMPLETED,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    RelativeYear = new RelativeYear(2024),
                    CreatedBy = "Test User",
                    UpdatedBy = "Test User",
                    BillingRunStatus = BillingRunStatus.Unknown,
                    BillingRunStartedAt = null,
                    CompletedBillingRun = completedBillingRun
                });
            }

            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                CalculatorRunClassificationId = (int) RunClassification.UNCLASSIFIED,
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
            designatedRuns.Add(new CalculatorRunDto
            {
                RunId = 101,
                RunName = $"My - {RunClassification.INITIAL_RUN_COMPLETED}",
                RunClassification = RunClassification.INITIAL_RUN_COMPLETED,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                RelativeYear = new RelativeYear(2024),
                CreatedBy = "Test User",
                UpdatedBy = "Test User",
                BillingRunStatus = BillingRunStatus.Unknown,
                BillingRunStartedAt = null,
                CompletedBillingRun = null
            });

            if (haveInterimRecalculationRunCompleted)
            {
                designatedRuns.Add(new CalculatorRunDto
                {
                    RunId = 102,
                    RunName = $"My - {RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED}",
                    RunClassification = RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    RelativeYear = new RelativeYear(2024),
                    CreatedBy = "Test User",
                    UpdatedBy = "Test User",
                    BillingRunStatus = BillingRunStatus.Unknown,
                    BillingRunStartedAt = null,
                    CompletedBillingRun = null
                });
            }

            if (haveFinalRecalculationRunCompleted)
            {
                designatedRuns.Add(new CalculatorRunDto
                {
                    RunId = 102,
                    RunName = $"My - {RunClassification.FINAL_RECALCULATION_RUN_COMPLETED}",
                    RunClassification = RunClassification.FINAL_RECALCULATION_RUN_COMPLETED,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    RelativeYear = new RelativeYear(2024),
                    CreatedBy = "Test User",
                    UpdatedBy = "Test User",
                    BillingRunStatus = BillingRunStatus.Unknown,
                    BillingRunStartedAt = null,
                    CompletedBillingRun = null
                });
            }

            if (haveFinalRunCompleted)
            {
                designatedRuns.Add(new CalculatorRunDto
                {
                    RunId = 102,
                    RunName = $"My - {RunClassification.FINAL_RUN_COMPLETED}",
                    RunClassification = RunClassification.FINAL_RUN_COMPLETED,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    RelativeYear = new RelativeYear(2024),
                    CreatedBy = "Test User",
                    UpdatedBy = "Test User",
                    BillingRunStatus = BillingRunStatus.Unknown,
                    BillingRunStartedAt = null,
                    CompletedBillingRun = null
                });
            }

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
    }
}
