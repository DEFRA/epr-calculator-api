using System.Configuration;
using System.Security.Claims;
using System.Security.Principal;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.UnitTests.Helpers;
using EPR.Calculator.API.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

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
        public async Task Create_Throws_ConfigurationErrorsException_When_ServiceBusConnectionString_Missing()
        {
            // Arrange
            var relativeYear = new RelativeYear(2024);
            AddDefaultParameterSettings(relativeYear);
            AddLapcapData(relativeYear);

            var configuration = ConfigurationItems.GetConfigurationValues();
            configuration.GetSection("ServiceBus").GetSection("ConnectionString").Value = string.Empty;

            var controller = CreateCalculatorController(configuration);

            var request = new CreateCalculatorRunDto
            {
                CalculatorRunName = "Test calculator run",
                RelativeYear = relativeYear,
            };

            // Act & Assert
            await Should.ThrowAsync<ConfigurationErrorsException>(async () => await controller.Create(request));
        }

        [TestMethod]
        public async Task Create_Throws_ConfigurationErrorsException_When_ServiceBusQueueName_Missing()
        {
            // Arrange
            var relativeYear = new RelativeYear(2024);
            AddDefaultParameterSettings(relativeYear);
            AddLapcapData(relativeYear);

            var configuration = ConfigurationItems.GetConfigurationValues();
            configuration.GetSection("ServiceBus").GetSection("QueueName").Value = string.Empty;

            var controller = CreateCalculatorController(configuration);

            var request = new CreateCalculatorRunDto
            {
                CalculatorRunName = "Test calculator run",
                RelativeYear = relativeYear,
            };

            // Act & Assert
            await Should.ThrowAsync<ConfigurationErrorsException>(async () => await controller.Create(request));
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
                CalculatorRunClassificationId = (int)RunClassification.RUNNING,
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
        public async Task GetCalculatorRuns_Returns_Ok_With_Matching_Runs()
        {
            // Arrange
            var request = new CalculatorRunsParamsDto { RelativeYear = new RelativeYear(2024) };

            // Act
            var result = await CalculatorController.GetCalculatorRuns(request, CancellationToken.None) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status200OK);
            var runs = result.Value as List<CalculatorRunDto>;
            runs.ShouldNotBeNull();
            runs.ShouldNotBeEmpty();
        }

        [TestMethod]
        public async Task GetCalculatorRuns_Returns_Ok_With_Empty_List_When_No_Runs_Match()
        {
            // Arrange
            var request = new CalculatorRunsParamsDto { RelativeYear = new RelativeYear(2022) };

            // Act
            var result = await CalculatorController.GetCalculatorRuns(request, CancellationToken.None) as ObjectResult;

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
                CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                RelativeYear = new RelativeYear(2024),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user",
                BillingRunStatus = BillingRunStatus.Running,
            };
            DbContext.CalculatorRuns.Add(run);
            DbContext.SaveChanges();

            // Act
            var result = await CalculatorController.GetCalculatorRun(run.Id.ToString(), CancellationToken.None) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            var runDto = result.Value as CalculatorRunDto;
            runDto.ShouldNotBeNull();
            runDto.RunId.ShouldBe(run.Id);
            runDto.RunClassification.ShouldBe(RunClassification.RUNNING);
            runDto.BillingRunStatus.ShouldBe(BillingRunStatus.Running);
            runDto.UpdatedAt.ShouldBeNull();
            runDto.UpdatedBy.ShouldBeNull();
        }

        [TestMethod]
        public async Task GetCalculatorRun_Returns_Ok_When_Found_By_Name()
        {
            // Arrange
            var run = new CalculatorRun
            {
                Name = "Uniquely Named Run",
                CalculatorRunClassificationId = (int)RunClassification.INITIAL_RUN,
                RelativeYear = new RelativeYear(2024),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user",
            };
            DbContext.CalculatorRuns.Add(run);
            DbContext.SaveChanges();

            // Act
            var result = await CalculatorController.GetCalculatorRun(run.Name, CancellationToken.None) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            var runDto = result.Value as CalculatorRunDto;
            runDto.ShouldNotBeNull();
            runDto.RunId.ShouldBe(run.Id);
        }

        [TestMethod]
        public async Task GetCalculatorRun_Returns_Ok_When_Found_By_Name_CaseInsensitive()
        {
            // Arrange
            var run = new CalculatorRun
            {
                Name = "Case Sensitivity Check",
                CalculatorRunClassificationId = (int)RunClassification.INITIAL_RUN,
                RelativeYear = new RelativeYear(2024),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user",
            };
            DbContext.CalculatorRuns.Add(run);
            DbContext.SaveChanges();

            // Act
            var result = await CalculatorController.GetCalculatorRun(run.Name.ToUpperInvariant(), CancellationToken.None) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            var runDto = result.Value as CalculatorRunDto;
            runDto.ShouldNotBeNull();
            runDto.RunId.ShouldBe(run.Id);
        }

        [TestMethod]
        public async Task GetCalculatorRun_Returns_Ok_With_BillingFile_Details_When_Present()
        {
            // Arrange
            var run = new CalculatorRun
            {
                Name = "Run With Billing File",
                CalculatorRunClassificationId = (int)RunClassification.INITIAL_RUN_COMPLETED,
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
            var result = await CalculatorController.GetCalculatorRun(run.Id.ToString(), CancellationToken.None) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            var runDto = result.Value as CalculatorRunDto;
            runDto.ShouldNotBeNull();
            runDto.RunClassification.ShouldBe(RunClassification.INITIAL_RUN_COMPLETED);
            runDto.BillingFile.ShouldNotBeNull();
            runDto.BillingFile.CsvFileName.ShouldBe("test.csv");
            runDto.BillingFile.JsonFileName.ShouldBe("test.json");
        }

        [TestMethod]
        public async Task GetCalculatorRun_Returns_NotFound_When_Not_Found_By_Id()
        {
            // Act
            var result = await CalculatorController.GetCalculatorRun("999999", CancellationToken.None) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
            result.Value.ShouldBe(string.Format(CommonResources.UnableToFindRun, 999999));
        }

        [TestMethod]
        public async Task GetCalculatorRun_Returns_NotFound_When_Not_Found_By_Name()
        {
            // Act
            var result = await CalculatorController.GetCalculatorRun("a run name that does not exist", CancellationToken.None) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
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
                new() { Id = (int)RunClassification.INITIAL_RUN, Status = nameof(RunClassification.INITIAL_RUN) },
                new() { Id = (int)RunClassification.TEST_RUN, Status = nameof(RunClassification.TEST_RUN) },
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
                    new() { Id = (int)RunClassification.INITIAL_RUN, Status = nameof(RunClassification.INITIAL_RUN) },
                    new() { Id = (int)RunClassification.TEST_RUN, Status = nameof(RunClassification.TEST_RUN) },
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
        public void CalculatorRunValidator_Validate_Returns_Required_Error_When_Name_Is_Empty()
        {
            // Arrange
            var validator = new CalculatorRunValidator();

            // Act
            var result = validator.Validate(string.Empty);

            // Assert
            result.ShouldNotBeNull();
            result.Errors[0].ErrorMessage.ShouldBe(CommonResources.CalculatorRunNameRequired);
        }

        private static ControllerContext CreateAuthenticatedControllerContext(string userName = "TestUser")
        {
            var identity = new GenericIdentity(userName);
            identity.AddClaim(new Claim("name", userName));
            var principal = new ClaimsPrincipal(identity);

            return new ControllerContext { HttpContext = new DefaultHttpContext { User = principal } };
        }

        private CalculatorController CreateCalculatorController(
            IConfiguration? configuration = null,
            ICalcRelativeYearRequestDtoDataValidator? validator = null,
            IAvailableClassificationsService? availableClassificationsService = null)
        {
            return new CalculatorController(
                DbContext,
                configuration ?? ConfigurationItems.GetConfigurationValues(),
                Mock.Of<IStorageService>(),
                Mock.Of<IServiceBusService>(),
                validator ?? Mock.Of<ICalcRelativeYearRequestDtoDataValidator>(),
                availableClassificationsService ?? Mock.Of<IAvailableClassificationsService>(),
                Mock.Of<ICalculationRunService>())
            {
                ControllerContext = CreateAuthenticatedControllerContext(),
            };
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
