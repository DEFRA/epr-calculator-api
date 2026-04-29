using AutoFixture;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.API.Dtos;
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
        [DataRow(RunClassification.InitialRun)]
        [DataRow(RunClassification.InterimRecalculationRun)]
        [DataRow(RunClassification.FinalRecalculationRun)]
        [DataRow(RunClassification.FinalRun)]
        public void ValidateMethod_ShouldReturnIsInvalidAsTrue_WhenRunIsAlreadyCompleted(
          RunClassification runClassification)
        {
            // Arrange
            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                Classification = runClassification,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                Classification = RunClassification.InitialRun,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsTrue(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        [DataRow(RunClassification.InitialRun)]
        [DataRow(RunClassification.InterimRecalculationRun)]
        [DataRow(RunClassification.FinalRecalculationRun)]
        [DataRow(RunClassification.FinalRun)]
        [DataRow(RunClassification.TestRun)]
        public void ValidateMethod_ShouldReturnIsInvalidAsFalse_WhenItsValidDesignatedRunAndExistingRunIsUnClassified(
            RunClassification runClassification)
        {
            // Arrange
            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                Classification = RunClassification.Unclassified,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                Classification = runClassification,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsFalse(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        [DataRow(RunClassification.InitialRun, RunClassification.InitialRunCompleted)]
        [DataRow(RunClassification.InterimRecalculationRun, RunClassification.InterimRecalculationRunCompleted)]
        [DataRow(RunClassification.FinalRecalculationRun, RunClassification.FinalRecalculationRunCompleted)]
        [DataRow(RunClassification.FinalRun, RunClassification.FinalRunCompleted)]
        public void ValidateMethod_ShouldReturnIsInvalidAsFalse_WhenItsValidDesignatedCompletedRunAndExistingRunIsCorrectUnCompletedRun(
          RunClassification runClassificationFrom,
          RunClassification runClassificationTo)
        {
            // Arrange
            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                Classification = runClassificationFrom,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                Classification = runClassificationTo,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsFalse(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        [DataRow(RunClassification.InitialRun)]
        [DataRow(RunClassification.InterimRecalculationRun)]
        [DataRow(RunClassification.FinalRecalculationRun)]
        [DataRow(RunClassification.FinalRun)]
        public void ValidateMethod_ShouldReturnIsInvalidAsTrue_WhenItsInValidDesignatedRunAndAndExistingRunIsRunning(
            RunClassification runClassification)
        {
            // Arrange
            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                Classification =  RunClassification.Running,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                Classification = runClassification,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsTrue(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        [DataRow(RunClassification.None)]
        [DataRow(RunClassification.Deleted)]
        public void ValidateMethod_ShouldReturnIsInvalidAsTrue_WhenItsDeletedRunAndExistingRunStatusIsNotToMarkAsDeleted(
            RunClassification runClassification)
        {
            // Arrange
            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                Classification =  runClassification,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                Classification = RunClassification.Deleted,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsTrue(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        [DataRow(RunClassification.Unclassified)]
        [DataRow(RunClassification.Errored)]
        [DataRow(RunClassification.InitialRun)]
        [DataRow(RunClassification.InterimRecalculationRun)]
        [DataRow(RunClassification.FinalRecalculationRun)]
        [DataRow(RunClassification.FinalRun)]
        [DataRow(RunClassification.TestRun)]
        [DataRow(RunClassification.Running)]
        public void ValidateMethod_ShouldReturnIsInvalidAsFalse_WhenItsDeletedRunAndExistingRunStatusIsValidToMarkAsDeleted(
            RunClassification runClassification)
        {
            // Arrange
            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                Classification =  runClassification,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                Classification = RunClassification.Deleted,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsFalse(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        [DataRow(RunClassification.TestRun)]
        [DataRow(RunClassification.None)]
        [DataRow(RunClassification.Running)]
        [DataRow(RunClassification.Errored)]
        [DataRow(RunClassification.Deleted)]
        public void ValidateMethod_ShouldReturnIsInvalidAsTrue_WhenItsTestRunAndExistingRunIsAlreadyMarkedAsTestOrOthers(
            RunClassification runClassification)
        {
            // Arrange
            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                Classification =  runClassification,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                Classification = RunClassification.TestRun,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsTrue(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        [DataRow(RunClassification.Unclassified)]
        [DataRow(RunClassification.InitialRun)]
        [DataRow(RunClassification.InterimRecalculationRun)]
        [DataRow(RunClassification.FinalRecalculationRun)]
        [DataRow(RunClassification.FinalRun)]
        public void ValidateMethod_ShouldReturnIsInvalidAsFalse_WhenItsTestRunExistingRunStatusIsValidToMarkAsTestRun(
            RunClassification runClassification)
        {
            // Arrange
            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                Classification =  runClassification,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                Classification = RunClassification.TestRun,
                RunId = 1,
            };

            // Act
            GenericValidationResultDto genericValidationResultDto = calculatorRunStatusDataValidatorUnderTest.Validate(calculatorRun, runStatusUpdateDto);

            // Assert
            Assert.IsNotNull(genericValidationResultDto);
            Assert.IsFalse(genericValidationResultDto.IsInvalid);
        }

        [TestMethod]
        [DataRow(RunClassification.None)]
        [DataRow(RunClassification.Running)]
        [DataRow(RunClassification.Unclassified)]
        [DataRow(RunClassification.Errored)]
        public void ValidateMethod_ShouldReturnIsInvalidAsTrue_WhenClassificationIdIsNotARecognisedTransitionTarget(
            RunClassification runClassification)
        {
            var calculatorRun = new CalculatorRun
            {
                RelativeYear = new RelativeYear(2024),
                Name = "Name",
                Classification = RunClassification.Unclassified,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                Classification = runClassification,
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
                Classification =  RunClassification.Running,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                Classification = RunClassification.InterimRecalculationRun,
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
                Classification =  RunClassification.Running,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                Classification = RunClassification.InitialRun,
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
        [DataRow(RunClassification.InitialRun, RunClassification.InitialRun)]
        [DataRow(RunClassification.InterimRecalculationRun, RunClassification.InterimRecalculationRun)]
        [DataRow(RunClassification.FinalRecalculationRun, RunClassification.InterimRecalculationRun)]
        [DataRow(RunClassification.FinalRun, RunClassification.InterimRecalculationRun)]
        [DataRow(RunClassification.FinalRecalculationRun, RunClassification.FinalRecalculationRun)]
        [DataRow(RunClassification.InterimRecalculationRun, RunClassification.FinalRecalculationRun)]
        [DataRow(RunClassification.FinalRun, RunClassification.FinalRecalculationRun)]
        [DataRow(RunClassification.FinalRun, RunClassification.FinalRun)]
        [DataRow(RunClassification.InterimRecalculationRun, RunClassification.FinalRun)]
        [DataRow(RunClassification.FinalRecalculationRun, RunClassification.FinalRun)]
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
                Classification =  RunClassification.Unclassified,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                Classification = requestRunClassification,
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
        [DataRow(RunClassification.InitialRun)]
        [DataRow(RunClassification.InitialRunCompleted)]
        public void ValidateWithDesignatedRunsMethod_ShouldReturnIsInvalidAsTrue_WhenAnotherRunIsMarkedAsInitialRunCompleted(
            RunClassification requestRunClassification)
        {
            // Arrange
            List<CalculatorRunDto> designatedRuns = [];
            designatedRuns.Add(new CalculatorRunDto
            {
                RunId = 101,
                RunName = $"My - {RunClassification.InitialRunCompleted}",
                RunClassification = RunClassification.InitialRunCompleted,
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
                Classification =  RunClassification.Unclassified,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                Classification = requestRunClassification,
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
        [DataRow(RunClassification.FinalRecalculationRun)]
        [DataRow(RunClassification.FinalRecalculationRunCompleted)]
        public void ValidateWithDesignatedRunsMethod_ShouldReturnIsInvalidAsTrue_WhenAnotherRunIsMarkedAsFinalRecalculationRunCompleted(
           RunClassification requestRunClassification)
        {
            // Arrange
            List<CalculatorRunDto> designatedRuns = [];
            designatedRuns.Add(new CalculatorRunDto
            {
                RunId = 101,
                RunName = $"My - {RunClassification.FinalRecalculationRunCompleted}",
                RunClassification = RunClassification.FinalRecalculationRunCompleted,
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
                Classification =  RunClassification.Unclassified,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                Classification = requestRunClassification,
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
        [DataRow(RunClassification.FinalRecalculationRun)]
        [DataRow(RunClassification.FinalRecalculationRunCompleted)]
        [DataRow(RunClassification.FinalRun)]
        [DataRow(RunClassification.FinalRunCompleted)]
        public void ValidateWithDesignatedRunsMethod_ShouldReturnIsInvalidAsTrue_WhenAnotherRunIsMarkedAsFinalRunCompleted(
          RunClassification requestRunClassification)
        {
            // Arrange
            List<CalculatorRunDto> designatedRuns = [];
            designatedRuns.Add(new CalculatorRunDto
            {
                RunId = 101,
                RunName = $"My - {RunClassification.FinalRunCompleted}",
                RunClassification = RunClassification.FinalRunCompleted,
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
                Classification =  RunClassification.Unclassified,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                Classification = requestRunClassification,
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
        [DataRow(RunClassification.InterimRecalculationRun)]
        [DataRow(RunClassification.FinalRecalculationRun)]
        public void ValidateWithDesignatedRunsMethod_ShouldReturnIsInvalidAsTrue_WhenSystemDontHaveInitialRunCompleted(
          RunClassification requestRunClassification)
        {
            // Arrange
            List<CalculatorRunDto> designatedRuns = [];
            designatedRuns.Add(new CalculatorRunDto
            {
                RunId = 101,
                RunName = $"My - {RunClassification.InterimRecalculationRunCompleted}",
                RunClassification = RunClassification.InterimRecalculationRunCompleted,
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
                Classification =  RunClassification.Unclassified,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                Classification = requestRunClassification,
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
                Classification =  RunClassification.Unclassified,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                Classification = RunClassification.FinalRun,
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
                RunName = $"My - {RunClassification.InitialRunCompleted}",
                RunClassification = RunClassification.InitialRunCompleted,
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
                    RunName = $"My - {RunClassification.InterimRecalculationRunCompleted}",
                    RunClassification = RunClassification.InterimRecalculationRunCompleted,
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
                    RunName = $"My - {RunClassification.FinalRecalculationRunCompleted}",
                    RunClassification = RunClassification.FinalRecalculationRunCompleted,
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
                    RunName = $"My - {RunClassification.FinalRunCompleted}",
                    RunClassification = RunClassification.FinalRunCompleted,
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
                Classification =  RunClassification.Unclassified,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                Classification = RunClassification.InterimRecalculationRun,
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
                RunName = $"My - {RunClassification.InitialRunCompleted}",
                RunClassification = RunClassification.InitialRunCompleted,
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
                    RunName = $"My - {RunClassification.InterimRecalculationRunCompleted}",
                    RunClassification = RunClassification.InterimRecalculationRunCompleted,
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
                Classification =  RunClassification.Unclassified,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                Classification = RunClassification.FinalRecalculationRun,
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
                RunName = $"My - {RunClassification.InitialRunCompleted}",
                RunClassification = RunClassification.InitialRunCompleted,
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
                    RunName = $"My - {RunClassification.InterimRecalculationRunCompleted}",
                    RunClassification = RunClassification.InterimRecalculationRunCompleted,
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
                    RunName = $"My - {RunClassification.FinalRecalculationRunCompleted}",
                    RunClassification = RunClassification.FinalRecalculationRunCompleted,
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
                Classification =  RunClassification.Unclassified,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                Classification = RunClassification.FinalRun,
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
                RunName = $"My - {RunClassification.InitialRunCompleted}",
                RunClassification = RunClassification.InitialRunCompleted,
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
                    RunName = $"My - {RunClassification.InterimRecalculationRunCompleted}",
                    RunClassification = RunClassification.InterimRecalculationRunCompleted,
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
                    RunName = $"My - {RunClassification.FinalRecalculationRunCompleted}",
                    RunClassification = RunClassification.FinalRecalculationRunCompleted,
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
                    RunName = $"My - {RunClassification.FinalRunCompleted}",
                    RunClassification = RunClassification.FinalRunCompleted,
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
                Classification = RunClassification.Unclassified,
            };
            var runStatusUpdateDto = new CalculatorRunStatusUpdateDto
            {
                Classification = RunClassification.InterimRecalculationRun,
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
