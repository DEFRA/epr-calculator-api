using System.Security.Claims;
using System.Security.Principal;
using EPR.Calculator.API.BackgroundService;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class CalculatorControllerTests : BaseControllerTest
    {
        [TestInitialize]
        public void Setup()
        {
            CalculatorController.ControllerContext = CreateAuthenticatedControllerContext();
        }

        [TestMethod]
        public async Task Create_Returns_Accepted_When_Data_Is_Valid()
        {
            // Arrange
            var relativeYear = new RelativeYear(2024);
            AddDefaultParameterSettings(relativeYear);
            AddLapcapData(relativeYear);

            var request = new CreateCalculatorRunDto
            {
                CalculatorRunName = "Test calculator run",
                RelativeYear = relativeYear,
            };

            // Act
            var result = await CalculatorController.Create(request) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status202Accepted);
        }

        [TestMethod]
        public async Task Create_Returns_FailedDependency_When_DefaultParameterSettings_And_LapcapData_Missing()
        {
            // Arrange - relative year 2024 has no default parameter settings or lapcap data seeded.
            var request = new CreateCalculatorRunDto
            {
                CalculatorRunName = "Test calculator run",
                RelativeYear = new RelativeYear(2024),
            };

            // Act
            var result = await CalculatorController.Create(request) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status424FailedDependency);
            result.Value.ShouldBe(string.Format(CommonResources.DataNotAvaialbleForRelativeYear, 2024));
        }

        [TestMethod]
        public async Task Create_Returns_FailedDependency_When_DefaultParameterSettings_Missing()
        {
            // Arrange
            var relativeYear = new RelativeYear(2024);
            AddLapcapData(relativeYear);

            var request = new CreateCalculatorRunDto
            {
                CalculatorRunName = "Test calculator run",
                RelativeYear = relativeYear,
            };

            // Act
            var result = await CalculatorController.Create(request) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status424FailedDependency);
            result.Value.ShouldBe(string.Format(CommonResources.DefaultParameterNotAvailable, 2024));
        }

        [TestMethod]
        public async Task Create_Returns_FailedDependency_When_LapcapData_Missing()
        {
            // Arrange
            var relativeYear = new RelativeYear(2024);
            AddDefaultParameterSettings(relativeYear);

            var request = new CreateCalculatorRunDto
            {
                CalculatorRunName = "Test calculator run",
                RelativeYear = relativeYear,
            };

            // Act
            var result = await CalculatorController.Create(request) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status424FailedDependency);
            result.Value.ShouldBe(string.Format(CommonResources.LapcapDataNotAvailable, 2024));
        }

        [TestMethod]
        public async Task Create_Returns_BadRequest_When_RelativeYear_Invalid()
        {
            // Arrange
            var request = new CreateCalculatorRunDto
            {
                CalculatorRunName = "Test calculator run",
                RelativeYear = new RelativeYear(-1),
            };

            // Act
            var result = await CalculatorController.Create(request) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        }

        [TestMethod]
        public async Task Create_Returns_UnprocessableEntity_When_AnotherRun_Is_Already_Running()
        {
            // Arrange
            var relativeYear = new RelativeYear(2024);
            AddDefaultParameterSettings(relativeYear);
            AddLapcapData(relativeYear);

            DbContext.CalculatorRuns.Add(new CalculatorRun
            {
                Name = "Run In Progress",
                RelativeYear = relativeYear,
                CreatedBy = "Test user",
                CreatedAt = DateTime.UtcNow,
                Classification = RunClassification.Running,
            });
            DbContext.SaveChanges();

            var request = new CreateCalculatorRunDto
            {
                CalculatorRunName = "Test calculator run",
                RelativeYear = relativeYear,
            };

            // Act
            var result = await CalculatorController.Create(request) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
            var message = result.Value?.GetType().GetProperty("Message")?.GetValue(result.Value) as string;
            message.ShouldBe(CommonResources.CalculationAlreadyRunning);
        }

        [TestMethod]
        public async Task GetCalculatorRuns_Returns_Ok_With_Matching_Runs_When_RelativeYear_Matches()
        {
            // Act
            var result = await CalculatorController.GetCalculatorRuns(new RelativeYear(2024), runName: null, CancellationToken.None) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status200OK);
            var runs = result.Value as List<CalculatorRunDto>;
            runs.ShouldNotBeNull();
            runs.ShouldNotBeEmpty();
        }

        [TestMethod]
        public async Task GetCalculatorRuns_Returns_Ok_With_Empty_List_When_RelativeYear_Does_Not_Match()
        {
            // Act
            var result = await CalculatorController.GetCalculatorRuns(new RelativeYear(2022), runName: null, CancellationToken.None) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status200OK);
            var runs = result.Value as List<CalculatorRunDto>;
            runs.ShouldNotBeNull();
            runs.ShouldBeEmpty();
        }

        [TestMethod]
        public async Task GetCalculatorRuns_Returns_Ok_With_Matching_Runs_When_Full_Name_Matches()
        {
            // Act
            var result = await CalculatorController.GetCalculatorRuns(relativeYear: null, runName: "Test Run 1", CancellationToken.None) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status200OK);
            var runs = result.Value as List<CalculatorRunDto>;
            runs.ShouldNotBeNull();
            runs.Count.ShouldBe(1);
        }

        [TestMethod]
        public async Task GetCalculatorRuns_Returns_Ok_With_Matching_Runs_When_Full_Name_Matches_Case_Insenstive()
        {
            // Act
            var result = await CalculatorController.GetCalculatorRuns(relativeYear: null, runName: "TEST RUN 1", CancellationToken.None) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status200OK);
            var runs = result.Value as List<CalculatorRunDto>;
            runs.ShouldNotBeNull();
            runs.Count.ShouldBe(1);
        }

        [TestMethod]
        public async Task GetCalculatorRuns_Returns_Ok_With_Matching_Runs_When_Partial_Name_Matches()
        {
            // Act
            var result = await CalculatorController.GetCalculatorRuns(relativeYear: null, runName: "%Run%", CancellationToken.None) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status200OK);
            var runs = result.Value as List<CalculatorRunDto>;
            runs.ShouldNotBeNull();
            runs.Count.ShouldBe(2);
        }

        [TestMethod]
        public async Task GetCalculatorRuns_Returns_Ok_With_Empty_List_When_Name_Does_Not_Match()
        {
            // Act
            var result = await CalculatorController.GetCalculatorRuns(relativeYear: null, runName: "some bad name", CancellationToken.None) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status200OK);
            var runs = result.Value as List<CalculatorRunDto>;
            runs.ShouldNotBeNull();
            runs.ShouldBeEmpty();
        }

        [TestMethod]
        public async Task GetCalculatorRun_Returns_Ok_When_Found_By_Id()
        {
            // Arrange
            var run = new CalculatorRun
            {
                Name = "Run Found By Id",
                Classification = RunClassification.Running,
                RelativeYear = new RelativeYear(2024),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user",
                BillingRunStatus = BillingRunStatus.Running,
            };
            DbContext.CalculatorRuns.Add(run);
            DbContext.SaveChanges();

            // Act
            var result = await CalculatorController.GetCalculatorRun(run.Id, CancellationToken.None) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            var runDto = result.Value as CalculatorRunDto;
            runDto.ShouldNotBeNull();
            runDto.RunId.ShouldBe(run.Id);
            runDto.RunClassification.ShouldBe(RunClassification.Running);
            runDto.BillingRunStatus.ShouldBe(BillingRunStatus.Running);
            runDto.UpdatedAt.ShouldBeNull();
            runDto.UpdatedBy.ShouldBeNull();
        }



        [TestMethod]
        public async Task GetCalculatorRun_Returns_Ok_With_BillingFile_Details_When_Present()
        {
            // Arrange
            var run = new CalculatorRun
            {
                Name = "Run With Billing File",
                Classification = RunClassification.InitialCompleted,
                RelativeYear = new RelativeYear(2024),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user",
                BillingRunStatus = BillingRunStatus.Completed,
            };
            DbContext.CalculatorRuns.Add(run);
            DbContext.SaveChanges();

            DbContext.CalculatorRunBillingFileMetadata.Add(new CalculatorRunBillingFileMetadata
            {
                BillingCsvFileName = "test.csv",
                BillingJsonFileName = "test.json",
                BillingFileCreatedBy = "Test user",
                BillingFileCreatedDate = DateTime.UtcNow,
                BillingFileAuthorisedDate = DateTime.UtcNow,
                BillingFileAuthorisedBy = "Test user",
                CalculatorRunId = run.Id,
            });
            DbContext.SaveChanges();

            // Act
            var result = await CalculatorController.GetCalculatorRun(run.Id, CancellationToken.None) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            var runDto = result.Value as CalculatorRunDto;
            runDto.ShouldNotBeNull();
            runDto.RunClassification.ShouldBe(RunClassification.InitialCompleted);
            runDto.BillingFile.ShouldNotBeNull();
            runDto.BillingFile.CsvFileName.ShouldBe("test.csv");
            runDto.BillingFile.JsonFileName.ShouldBe("test.json");
        }

        [TestMethod]
        public async Task GetCalculatorRun_Returns_NotFound_When_Not_Found_By_Id()
        {
            // Act
            var result = await CalculatorController.GetCalculatorRun(999999, CancellationToken.None) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
            result.Value.ShouldBe(string.Format(CommonResources.UnableToFindRun, 999999));
        }

        [TestMethod]
        public async Task RelativeYears_Returns_Ok_With_Available_Years()
        {
            // Act
            var result = await CalculatorController.RelativeYears() as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            var years = result.Value as IEnumerable<RelativeYear>;
            years.ShouldNotBeNull();
            years.ShouldContain(new RelativeYear(2024));
        }

        [TestMethod]
        public async Task ClassificationByRelativeYear_Returns_Options_For_Valid_RelativeYear()
        {
            // Arrange
            var relativeYear = new RelativeYear(2024);
            var request = new CalcRelativeYearRequestDto { RunId = Random.Shared.Next(), RelativeYearValue = relativeYear.Value };

            var expectedClassifications = new List<CalculatorRunClassificationDto>
            {
                new() { Id = RunClassification.Initial, Status = nameof(RunClassification.Initial) },
                new() { Id = RunClassification.Test, Status = nameof(RunClassification.Test) },
            };

            var mockValidator = new Mock<ICalcRelativeYearRequestDtoDataValidator>();
            mockValidator
                .Setup(v => v.Validate(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResultDto<ErrorDto> { IsInvalid = false });

            var mockAvailableClassificationsService = new Mock<IAvailableClassificationsService>();
            mockAvailableClassificationsService
                .Setup(s => s.GetAvailableClassificationsForRelativeYearAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CalculatorRunClassification>
                {
                    new() { Id = RunClassification.Initial, Status = nameof(RunClassification.Initial) },
                    new() { Id = RunClassification.Test, Status = nameof(RunClassification.Test) },
                });

            var controller = CreateCalculatorController(
                validator: mockValidator.Object,
                availableClassificationsService: mockAvailableClassificationsService.Object);

            // Act
            var result = await controller.ClassificationByRelativeYear(request) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status200OK);
            var response = result.Value as RelativeYearClassificationResponseDto;
            response.ShouldNotBeNull();
            response.RelativeYear.ShouldBe(relativeYear);
            response.Classifications.ShouldBeEquivalentTo(expectedClassifications);
        }

        [TestMethod]
        public async Task ClassificationByRelativeYear_Returns_BadRequest_When_Validation_Fails()
        {
            // Arrange
            var request = new CalcRelativeYearRequestDto { RunId = Random.Shared.Next(), RelativeYearValue = 2025 };

            var mockValidator = new Mock<ICalcRelativeYearRequestDtoDataValidator>();
            mockValidator
                .Setup(v => v.Validate(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResultDto<ErrorDto>
                {
                    IsInvalid = true,
                    Errors = new List<ErrorDto> { new() { Message = "Invalid relative year format." } },
                });

            var controller = CreateCalculatorController(validator: mockValidator.Object);

            // Act
            var result = await controller.ClassificationByRelativeYear(request) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
            var errors = result.Value as List<ErrorDto>;
            errors.ShouldNotBeNull();
            errors[0].Message.ShouldBe("Invalid relative year format.");
        }

        [TestMethod]
        public async Task ClassificationByRelativeYear_Returns_NotFound_When_No_Classifications()
        {
            // Arrange
            var request = new CalcRelativeYearRequestDto { RunId = Random.Shared.Next(), RelativeYearValue = 2024 };

            var mockValidator = new Mock<ICalcRelativeYearRequestDtoDataValidator>();
            mockValidator
                .Setup(v => v.Validate(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResultDto<ErrorDto> { IsInvalid = false });

            var mockAvailableClassificationsService = new Mock<IAvailableClassificationsService>();
            mockAvailableClassificationsService
                .Setup(s => s.GetAvailableClassificationsForRelativeYearAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CalculatorRunClassification>());

            var controller = CreateCalculatorController(
                validator: mockValidator.Object,
                availableClassificationsService: mockAvailableClassificationsService.Object);

            // Act
            var result = await controller.ClassificationByRelativeYear(request) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
            result.Value.ShouldBe(CommonResources.NoClassificationsFound);
        }

        [TestMethod]
        public async Task ClassificationByRelativeYear_Throws_When_Validator_Throws()
        {
            // Arrange
            var request = new CalcRelativeYearRequestDto { RunId = Random.Shared.Next(), RelativeYearValue = 2024 };

            var mockValidator = new Mock<ICalcRelativeYearRequestDtoDataValidator>();
            mockValidator
                .Setup(v => v.Validate(request, It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            var controller = CreateCalculatorController(validator: mockValidator.Object);

            // Act & Assert
            await Should.ThrowAsync<Exception>(async () => await controller.ClassificationByRelativeYear(request));
        }

        [TestMethod]
        public async Task DeleteCalculatorRun_Returns_NoContent_When_Run_Does_Not_Exist()
        {
            // Arrange
            var mockRunStatusValidator = new Mock<IRunClassificationValidator>();
            var controller = CreateCalculatorController(runStatusValidator: mockRunStatusValidator.Object);

            // Act
            var result = await controller.DeleteCalculatorRun(999, CancellationToken.None) as NoContentResult;

            // Assert
            result.ShouldNotBeNull();
            mockRunStatusValidator.Verify(
                v => v.ValidateAsync(It.IsAny<CalculatorRun>(), It.IsAny<RunClassification>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [TestMethod]
        public async Task DeleteCalculatorRun_Returns_NoContent_When_Run_Already_Deleted()
        {
            // Arrange
            var run = AddCalculatorRun(RunClassification.Deleted);

            var mockRunStatusValidator = new Mock<IRunClassificationValidator>();
            var controller = CreateCalculatorController(runStatusValidator: mockRunStatusValidator.Object);

            // Act
            var result = await controller.DeleteCalculatorRun(run.Id, CancellationToken.None) as NoContentResult;

            // Assert
            result.ShouldNotBeNull();
            mockRunStatusValidator.Verify(
                v => v.ValidateAsync(It.IsAny<CalculatorRun>(), It.IsAny<RunClassification>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [TestMethod]
        public async Task DeleteCalculatorRun_Returns_UnprocessableEntity_When_Run_Status_Validation_Fails()
        {
            // Arrange
            var run = AddCalculatorRun(RunClassification.Unclassified);

            var mockRunStatusValidator = new Mock<IRunClassificationValidator>();
            mockRunStatusValidator
                .Setup(v => v.ValidateAsync(It.IsAny<CalculatorRun>(), It.IsAny<RunClassification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GenericValidationResultDto
                {
                    Errors = new List<string> { "Run cannot be deleted." },
                });

            var controller = CreateCalculatorController(runStatusValidator: mockRunStatusValidator.Object);

            // Act
            var result = await controller.DeleteCalculatorRun(run.Id, CancellationToken.None) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
            var errors = result.Value as List<string>;
            errors.ShouldNotBeNull();
            errors[0].ShouldBe("Run cannot be deleted.");
            DbContext.CalculatorRuns.Single(r => r.Id == run.Id)
                .Classification.ShouldBe(RunClassification.Unclassified);
        }

        [TestMethod]
        public async Task DeleteCalculatorRun_Returns_UnprocessableEntity_When_Designated_Runs_Validation_Fails()
        {
            // Arrange
            var run = AddCalculatorRun(RunClassification.Unclassified);

            var mockRunStatusValidator = new Mock<IRunClassificationValidator>();
            mockRunStatusValidator
                .Setup(v => v.ValidateAsync(It.IsAny<CalculatorRun>(), It.IsAny<RunClassification>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GenericValidationResultDto{Errors = ["Test"]});

            var controller = CreateCalculatorController(runStatusValidator: mockRunStatusValidator.Object);

            // Act
            var result = await controller.DeleteCalculatorRun(run.Id, CancellationToken.None) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
            var errors = result.Value as List<string>;
            errors.ShouldNotBeNull();
            errors[0].ShouldBe("Test");
            DbContext.CalculatorRuns.Single(r => r.Id == run.Id)
                .Classification.ShouldBe(RunClassification.Unclassified);
        }

        [TestMethod]
        public async Task DeleteCalculatorRun_Marks_Run_As_Deleted_When_Validation_Passes()
        {
            // Arrange
            var run = AddCalculatorRun(RunClassification.Unclassified);
            var designatedRuns = new List<CalculatorRunDto>();

            var mockRunStatusValidator = new Mock<IRunClassificationValidator>();
            mockRunStatusValidator
                .Setup(v => v.ValidateAsync(
                    It.Is<CalculatorRun>(r => r.Id == run.Id),
                    It.Is<RunClassification>(x => x == RunClassification.Deleted),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GenericValidationResultDto());

            var mockCalculationRunService = new Mock<ICalculationRunService>();
            mockCalculationRunService
                .Setup(s => s.GetDesignatedRunsByFinancialYear(run.RelativeYear, It.IsAny<CancellationToken>()))
                .ReturnsAsync(designatedRuns);

            var controller = CreateCalculatorController(
                runStatusValidator: mockRunStatusValidator.Object,
                calculationRunService: mockCalculationRunService.Object);

            // Act
            var result = await controller.DeleteCalculatorRun(run.Id, CancellationToken.None) as NoContentResult;

            // Assert
            result.ShouldNotBeNull();
            var deletedRun = DbContext.CalculatorRuns.Single(r => r.Id == run.Id);
            deletedRun.Classification.ShouldBe(RunClassification.Deleted);
            deletedRun.UpdatedBy.ShouldBe("TestUser");
            deletedRun.UpdatedAt.ShouldNotBeNull();
        }

        private static ControllerContext CreateAuthenticatedControllerContext(string userName = "TestUser")
        {
            var identity = new GenericIdentity(userName);
            identity.AddClaim(new Claim("name", userName));
            var principal = new ClaimsPrincipal(identity);

            return new ControllerContext { HttpContext = new DefaultHttpContext { User = principal } };
        }

        private CalculatorController CreateCalculatorController(
            ICalcRelativeYearRequestDtoDataValidator? validator = null,
            IAvailableClassificationsService? availableClassificationsService = null,
            IRunClassificationValidator? runStatusValidator = null,
            ICalculationRunService? calculationRunService = null)
        {
            return new CalculatorController(
                DbContext,
                Mock.Of<IBlobStorageService>(),
                Mock.Of<IBackgroundTaskQueue>(),
                runStatusValidator ?? Mock.Of<IRunClassificationValidator>(),
                validator ?? Mock.Of<ICalcRelativeYearRequestDtoDataValidator>(),
                availableClassificationsService ?? Mock.Of<IAvailableClassificationsService>(),
                calculationRunService ?? Mock.Of<ICalculationRunService>())
            {
                ControllerContext = CreateAuthenticatedControllerContext(),
            };
        }

        private CalculatorRun AddCalculatorRun(RunClassification classification)
        {
            var run = new CalculatorRun
            {
                Name = "Test run to delete",
                RelativeYear = new RelativeYear(2024),
                CreatedBy = "Test user",
                CreatedAt = DateTime.UtcNow,
                Classification = classification,
            };
            DbContext.CalculatorRuns.Add(run);
            DbContext.SaveChanges();
            return run;
        }

        private void AddDefaultParameterSettings(RelativeYear relativeYear, DateTime? effectiveTo = null)
        {
            DbContext.DefaultParameterSettings.Add(new DefaultParameterSettingMaster
            {
                RelativeYear = relativeYear,
                CreatedBy = "Test user",
                CreatedAt = DateTime.UtcNow,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = effectiveTo,
            });
            DbContext.SaveChanges();
        }

        private void AddLapcapData(RelativeYear relativeYear, DateTime? effectiveTo = null)
        {
            DbContext.LapcapDataMaster.Add(new LapcapDataMaster
            {
                RelativeYear = relativeYear,
                CreatedBy = "Test user",
                CreatedAt = DateTime.UtcNow,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = effectiveTo,
            });
            DbContext.SaveChanges();
        }
    }
}
