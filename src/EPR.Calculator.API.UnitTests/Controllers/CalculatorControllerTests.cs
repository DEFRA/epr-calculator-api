using System.Security.Claims;
using System.Security.Principal;
using AutoFixture;
using EnumsNET;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Services.Abstractions;
using EPR.Calculator.API.UnitTests.Helpers;
using EPR.Calculator.API.Validators;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class CalculatorControllerTests : BaseControllerTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatorControllerTests"/> class.
        /// </summary>
        public CalculatorControllerTests()
        {
            Fixture = new Fixture();

            // Set up authorisation.
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);
            var context = new DefaultHttpContext { User = principal };
            CalculatorController.ControllerContext = new ControllerContext { HttpContext = context };
        }

        public Fixture Fixture { get; init; }

        private CalculatorRunFinancialYear FinancialYear23_24 { get; } = new CalculatorRunFinancialYear { Name = "2023-24" };

        [TestMethod]
        public async Task Create_Calculator_Run()
        {
            var createCalculatorRunDto = new CreateCalculatorRunDto
            {
                CalculatorRunName = "Test calculator run",
                FinancialYear = "2024-25",
            };

            DbContext.DefaultParameterSettings.Add(new DefaultParameterSettingMaster
            {
                Id = 1,
                ParameterYear = FinancialYear24_25,
                CreatedBy = "Testuser",
                CreatedAt = DateTime.UtcNow,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = null,
            });

            DbContext.LapcapDataMaster.Add(new LapcapDataMaster
            {
                Id = 1,
                ProjectionYear = FinancialYear24_25,
                CreatedBy = "Testuser",
                CreatedAt = DateTime.UtcNow,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = null,
            });

            DbContext.SaveChanges();

            var actionResult = await CalculatorController.Create(createCalculatorRunDto) as ObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(202, actionResult.StatusCode);
        }

        [TestMethod]
        public async Task Create_Calculator_Run_Return_404_If_No_Default_Parameter_Settings_And_Lapcap_Data()
        {
            var createCalculatorRunDto = new CreateCalculatorRunDto
            {
                CalculatorRunName = "Test calculator run",
                FinancialYear = "2024-25",
            };

            DbContext.DefaultParameterSettings.Add(new DefaultParameterSettingMaster
            {
                Id = 1,
                ParameterYear = FinancialYear23_24,
                CreatedBy = "Testuser",
                CreatedAt = DateTime.UtcNow,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = null,
            });
            DbContext.SaveChanges();

            DbContext.LapcapDataMaster.Add(new LapcapDataMaster
            {
                Id = 1,
                ProjectionYear = FinancialYear23_24,
                CreatedBy = "Testuser",
                CreatedAt = DateTime.UtcNow,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = null,
            });
            DbContext.SaveChanges();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var actionResult = await CalculatorController.Create(createCalculatorRunDto) as ObjectResult;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(424, actionResult.StatusCode);
            Assert.AreEqual("Default parameter settings and Lapcap data not available for the financial year 2024-25.", actionResult.Value);
        }

        [TestMethod]
        public async Task Create_Calculator_Run_Return_404_If_No_Default_Parameter_Settings()
        {
            var createCalculatorRunDto = new CreateCalculatorRunDto
            {
                CalculatorRunName = "Test calculator run",
                FinancialYear = "2024-25",
            };

            DbContext.DefaultParameterSettings.Add(new DefaultParameterSettingMaster
            {
                Id = 1,
                ParameterYear = FinancialYear23_24,
                CreatedBy = "Testuser",
                CreatedAt = DateTime.UtcNow,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = null,
            });

            DbContext.LapcapDataMaster.Add(new LapcapDataMaster
            {
                Id = 1,
                ProjectionYear = FinancialYear24_25,
                CreatedBy = "Testuser",
                CreatedAt = DateTime.UtcNow,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = null,
            });
            DbContext.SaveChanges();

            var actionResult = await CalculatorController.Create(createCalculatorRunDto) as ObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(424, actionResult.StatusCode);
            Assert.AreEqual("Default parameter settings not available for the financial year 2024-25.", actionResult.Value);
        }

        [TestMethod]
        public async Task Create_Calculator_Run_Return_404_If_No_Lapcap_Data()
        {
            var financialYear27_28 = new CalculatorRunFinancialYear { Name = "2027-28" };

            var createCalculatorRunDto = new CreateCalculatorRunDto
            {
                CalculatorRunName = "Test calculator run",
                FinancialYear = "2027-28",
            };

            DbContext.DefaultParameterSettings.Add(new DefaultParameterSettingMaster
            {
                Id = 1,
                ParameterYear = financialYear27_28,
                CreatedBy = "Testuser",
                CreatedAt = DateTime.UtcNow,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = null,
            });
            DbContext.SaveChanges();

            DbContext.LapcapDataMaster.Add(new LapcapDataMaster
            {
                Id = 1,
                ProjectionYear = FinancialYear23_24,
                CreatedBy = "Testuser",
                CreatedAt = DateTime.UtcNow,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = null,
            });
            DbContext.SaveChanges();

            var actionResult = await CalculatorController.Create(createCalculatorRunDto) as ObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(424, actionResult.StatusCode);
            Assert.AreEqual("Lapcap data not available for the financial year 2027-28.", actionResult.Value);
        }

        [TestMethod]
        public async Task Create_Calculator_Run_Return_500_If_ConnectionString_Configuration_Is_Empty()
        {
            var createCalculatorRunDto = new CreateCalculatorRunDto
            {
                CalculatorRunName = "Test calculator run",
                FinancialYear = "2024-25",
            };

            DbContext.DefaultParameterSettings.Add(new DefaultParameterSettingMaster
            {
                Id = 1,
                ParameterYear = FinancialYear24_25,
                CreatedBy = "Testuser",
                CreatedAt = DateTime.UtcNow,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = null,
            });
            DbContext.SaveChanges();

            DbContext.LapcapDataMaster.Add(new LapcapDataMaster
            {
                Id = 1,
                ProjectionYear = FinancialYear24_25,
                CreatedBy = "Testuser",
                CreatedAt = DateTime.UtcNow,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = null,
            });
            DbContext.SaveChanges();

            var configs = ConfigurationItems.GetConfigurationValues();
            configs.GetSection("ServiceBus").GetSection("ConnectionString").Value = string.Empty;

            var mockServiceBusService = new Mock<IServiceBusService>();
            var mockStorageService = new Mock<IStorageService>();
            var mockValidator = new Mock<ICalcFinancialYearRequestDtoDataValidator>();

            CalculatorController =
                new CalculatorController(
                    DbContext,
                    configs,
                    mockStorageService.Object,
                    mockServiceBusService.Object,
                    mockValidator.Object,
                    Mock.Of<IAvailableClassificationsService>(),
                    Mock.Of<ICalculationRunService>(),
                    Mock.Of<IBillingFileService>());

            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext()
            {
                User = principal,
            };

            CalculatorController.ControllerContext = new ControllerContext
            {
                HttpContext = context,
            };

            var actionResult = await CalculatorController.Create(createCalculatorRunDto) as ObjectResult;
            var actionResultValue = actionResult?.Value as System.Configuration.ConfigurationErrorsException;

            Assert.IsNotNull(actionResult);
            Assert.AreEqual(500, actionResult.StatusCode);
            Assert.AreEqual("Configuration item not found: ServiceBus__ConnectionString", actionResultValue?.Message);
        }

        [TestMethod]
        public async Task Create_Calculator_Run_Return_500_If_QueueName_Configuration_Is_Empty()
        {
            var createCalculatorRunDto = new CreateCalculatorRunDto
            {
                CalculatorRunName = "Test calculator run",
                FinancialYear = "2024-25",
            };

            DbContext.DefaultParameterSettings.Add(new DefaultParameterSettingMaster
            {
                Id = 1,
                ParameterYear = FinancialYear24_25,
                CreatedBy = "Testuser",
                CreatedAt = DateTime.UtcNow,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = null,
            });
            DbContext.SaveChanges();

            DbContext.LapcapDataMaster.Add(new LapcapDataMaster
            {
                Id = 1,
                ProjectionYear = FinancialYear24_25,
                CreatedBy = "Testuser",
                CreatedAt = DateTime.UtcNow,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = null,
            });
            DbContext.SaveChanges();

            var configs = ConfigurationItems.GetConfigurationValues();
            configs.GetSection("ServiceBus").GetSection("QueueName").Value = string.Empty;

            var mockServiceBusService = new Mock<IServiceBusService>();
            var mockStorageService = new Mock<IStorageService>();
            var mockValidator = new Mock<ICalcFinancialYearRequestDtoDataValidator>();
            CalculatorController =
                new CalculatorController(
                    DbContext,
                    configs,
                    mockStorageService.Object,
                    mockServiceBusService.Object,
                    mockValidator.Object,
                    Mock.Of<IAvailableClassificationsService>(),
                    Mock.Of<ICalculationRunService>(),
                    Mock.Of<IBillingFileService>());

            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext()
            {
                User = principal,
            };

            CalculatorController.ControllerContext = new ControllerContext
            {
                HttpContext = context,
            };

            var actionResult = await CalculatorController.Create(createCalculatorRunDto) as ObjectResult;
            var actionResultValue = actionResult?.Value as System.Configuration.ConfigurationErrorsException;

            Assert.IsNotNull(actionResult);
            Assert.AreEqual(500, actionResult.StatusCode);
            Assert.AreEqual("Configuration item not found: ServiceBus__QueueName", actionResultValue?.Message);
        }

        [TestMethod]
        public async Task Create_Calculator_Run_Return_400_If_FinancialYear_Invalid()
        {
            var createCalculatorRunDto = new CreateCalculatorRunDto
            {
                CalculatorRunName = "Test calculator run",
                FinancialYear = Fixture.Create<string>(),
            };
            var actionResult = await CalculatorController.Create(createCalculatorRunDto) as ObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, actionResult.StatusCode);
        }

        [TestMethod]
        public async Task Get_Calculator_Runs_Return_Results_Test()
        {
            var runParams = new CalculatorRunsParamsDto
            {
                FinancialYear = "2024-25",
            };
            var actionResult = await CalculatorController.GetCalculatorRuns(runParams) as ObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(200, actionResult.StatusCode);
        }

        [TestMethod]
        public async Task Get_Calculator_Runs_Return_Not_Found_Test()
        {
            var runParams = new CalculatorRunsParamsDto
            {
                FinancialYear = "2022-23",
            };
            var actionResult = await CalculatorController.GetCalculatorRuns(runParams) as ObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(404, actionResult.StatusCode);
        }

        [TestMethod]
        public async Task Get_Calculator_Runs_Return_Bad_Request_Test()
        {
            var runParams = new CalculatorRunsParamsDto
            {
                FinancialYear = string.Empty,
            };
            var actionResult = await CalculatorController.GetCalculatorRuns(runParams) as ObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(400, actionResult.StatusCode);
        }

        [TestMethod]
        public void Get_Calculator_Run_Return_400_Error_With_No_NameSupplied()
        {
            CalculatorRunValidator validator = new CalculatorRunValidator();
            string name = string.Empty;
            var result = validator.Validate(name);

            Assert.IsNotNull(result);
            Assert.AreEqual("Calculator Run Name is Required", result.Errors[0].ErrorMessage);
        }

        [TestMethod]
        public async Task Get_Calculator_Run_Return_Results_By_Name_Test()
        {
            string calculatorRunName = "Test Run";

            var actionResult = await CalculatorController.GetCalculatorRunByName(calculatorRunName) as ObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(200, actionResult.Value);
        }

        [TestMethod]
        public async Task Get_Calculator_Run_Return_Results_Not_found()
        {
            string calculatorRunName = "test 45610";

            var actionResult = await CalculatorController.GetCalculatorRunByName(calculatorRunName) as ObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(404, actionResult.StatusCode);
        }

        [TestMethod]
        public async Task Get_Calculator_Run_Return_Result_With_String_Comparison_CaseInsensitive()
        {
            string calculatorRunName = "TEST run";

            var actionResult = await CalculatorController.GetCalculatorRunByName(calculatorRunName) as ObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(200, actionResult.Value);
        }

        [TestMethod]
        public async Task Create_Calculator_Run_Return_422_If_One_Calculation_Already_In_Running()
        {
            var createCalculatorRunDto = new CreateCalculatorRunDto
            {
                CalculatorRunName = "Test calculator run",
                FinancialYear = "2024-25",
            };

            DbContext.DefaultParameterSettings.Add(new DefaultParameterSettingMaster
            {
                Id = 1,
                ParameterYear = FinancialYear23_24,
                CreatedBy = "Testuser",
                CreatedAt = DateTime.UtcNow,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = null,
            });
            DbContext.SaveChanges();

            DbContext.LapcapDataMaster.Add(new LapcapDataMaster
            {
                Id = 1,
                ProjectionYear = FinancialYear23_24,
                CreatedBy = "Testuser",
                CreatedAt = DateTime.UtcNow,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = null,
            });
            DbContext.SaveChanges();

            DbContext.CalculatorRuns.Add(new CalculatorRun
            {
                CreatedBy = "Testuser",
                CreatedAt = DateTime.UtcNow,
                CalculatorRunClassificationId = 2,
                Financial_Year = FinancialYear23_24,
                Name = "TestOneAtATime",
            });
            DbContext.SaveChanges();

            var actionResult = await CalculatorController.Create(createCalculatorRunDto) as ObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(422, actionResult.StatusCode);
            var expectedJson = "{\"Message\":\"The calculator is currently running. You will be able to run another calculation once the current one has finished.\"}";
            var actualJson = System.Text.Json.JsonSerializer.Serialize(actionResult?.Value);
            Assert.AreEqual(expectedJson, actualJson);
        }

        [TestMethod]
        public async Task CanCallFinancialYears()
        {
            // Act
            var result = await CalculatorController.FinancialYears() as ObjectResult;
            var resultList = result?.Value as IEnumerable<FinancialYearDto>;

            // Assert
            Assert.IsInstanceOfType<IEnumerable<FinancialYearDto>>(resultList);
            Assert.AreEqual("2024-25", resultList?.Single().Name);
        }

        [TestMethod]
        public async Task ClassificationByFinancialYear_Returns_Options_For_Valid_FinancialYear()
        {
            var initialRun = RunClassification.INITIAL_RUN.AsString(EnumFormat.Description);
            var testRun = RunClassification.TEST_RUN.AsString(EnumFormat.Description);

            if (initialRun == null || testRun == null)
            {
                Assert.Fail("Run classifications Enums not loaded");
            }

            // Arrange
            var financialYear = "2024-25";
            var request = new CalcFinancialYearRequestDto { FinancialYear = financialYear };

            var mockValidator = new Mock<ICalcFinancialYearRequestDtoDataValidator>();
            mockValidator
                .Setup(v => v.Validate(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResultDto<ErrorDto> { IsInvalid = false });

            var mockAvailableClassificationsService = new Mock<IAvailableClassificationsService>();
            mockAvailableClassificationsService
                .Setup(s => s.GetAvailableClassificationsForFinancialYearAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CalculatorRunClassification>
                {
                    new() { Id = (int)RunClassification.INITIAL_RUN, Status = RunClassification.INITIAL_RUN.AsString(EnumFormat.Description)! },
                    new() { Id = (int)RunClassification.TEST_RUN, Status = RunClassification.TEST_RUN.AsString(EnumFormat.Description)! },
                });

            var mockDbContext = MockDbContextForCalculatorRunClassifications();

            var individualCalcController = new CalculatorController(
                mockDbContext.Object,
                ConfigurationItems.GetConfigurationValues(),
                Mock.Of<IStorageService>(),
                Mock.Of<IServiceBusService>(),
                mockValidator.Object,
                mockAvailableClassificationsService.Object,
                Mock.Of<ICalculationRunService>(),
                Mock.Of<IBillingFileService>());

            var expectedClassifications = new List<CalculatorRunClassificationDto>
            {
                new CalculatorRunClassificationDto { Id = (int)RunClassification.INITIAL_RUN, Status = RunClassification.INITIAL_RUN.AsString(EnumFormat.Description)! },
                new() { Id = (int)RunClassification.TEST_RUN, Status = RunClassification.TEST_RUN.AsString(EnumFormat.Description)! },
            };

            // Act
            var actionResult = await individualCalcController.ClassificationByFinancialYear(request) as ObjectResult;

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(StatusCodes.Status200OK, actionResult.StatusCode);
            var result = actionResult.Value as FinancialYearClassificationResponseDto;
            Assert.IsNotNull(result);
            var typeToAssert = typeof(CalculatorRunClassificationDto);
            Assert.IsInstanceOfType(expectedClassifications[0], typeToAssert);
            Assert.IsInstanceOfType(result.Classifications[1], typeToAssert);
            result.Classifications.Should().BeEquivalentTo(expectedClassifications);
            Assert.AreEqual(result.FinancialYear, financialYear);
        }

        [TestMethod]
        public async Task ClassificationByFinancialYear_Returns_BadRequest_For_Invalid_FinancialYear()
        {
            // Arrange
            var financialYear = "2025-2026"; // Invalid format
            var request = new CalcFinancialYearRequestDto { FinancialYear = financialYear };

            var mockValidator = new Mock<ICalcFinancialYearRequestDtoDataValidator>();
            mockValidator
                .Setup(v => v.Validate(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResultDto<ErrorDto>
                {
                    IsInvalid = true,
                    Errors = new List<ErrorDto> { new ErrorDto { Message = "Invalid financial year format." } },
                });

            var controller = new CalculatorController(
                DbContext,
                ConfigurationItems.GetConfigurationValues(),
                Mock.Of<IStorageService>(),
                Mock.Of<IServiceBusService>(),
                mockValidator.Object,
                Mock.Of<IAvailableClassificationsService>(),
                Mock.Of<ICalculationRunService>(),
                Mock.Of<IBillingFileService>());

            // Act
            var actionResult = await controller.ClassificationByFinancialYear(request) as ObjectResult;

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, actionResult.StatusCode);
            var errors = actionResult.Value as List<ErrorDto>;
            Assert.IsNotNull(errors);
            Assert.AreEqual("Invalid financial year format.", errors[0].Message);
        }

        [TestMethod]
        public async Task ClassificationByFinancialYear_Returns_NotFound_When_No_Classifications()
        {
            // Arrange
            var financialYear = "2024-25";
            var request = new CalcFinancialYearRequestDto { FinancialYear = financialYear };

            var mockValidator = new Mock<ICalcFinancialYearRequestDtoDataValidator>();
            mockValidator
                .Setup(v => v.Validate(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResultDto<ErrorDto> { IsInvalid = false });

            var mockAvailableClassificationsService = new Mock<IAvailableClassificationsService>();
            mockAvailableClassificationsService
                .Setup(s => s.GetAvailableClassificationsForFinancialYearAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CalculatorRunClassification>());

            var controller = new CalculatorController(
                DbContext,
                ConfigurationItems.GetConfigurationValues(),
                Mock.Of<IStorageService>(),
                Mock.Of<IServiceBusService>(),
                mockValidator.Object,
                mockAvailableClassificationsService.Object,
                Mock.Of<ICalculationRunService>(),
                Mock.Of<IBillingFileService>());

            // Act
            var actionResult = await controller.ClassificationByFinancialYear(request) as ObjectResult;

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(StatusCodes.Status404NotFound, actionResult.StatusCode);
            Assert.AreEqual("No classifications found.", actionResult.Value);
        }

        [TestMethod]
        public async Task ClassificationByFinancialYear_Returns_InternalServerError_On_Exception()
        {
            // Arrange
            var financialYear = "2024-25";
            var request = new CalcFinancialYearRequestDto { FinancialYear = financialYear };

            var mockValidator = new Mock<ICalcFinancialYearRequestDtoDataValidator>();
            mockValidator
                .Setup(v => v.Validate(request, It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            var controller = new CalculatorController(
                DbContext,
                ConfigurationItems.GetConfigurationValues(),
                Mock.Of<IStorageService>(),
                Mock.Of<IServiceBusService>(),
                mockValidator.Object,
                Mock.Of<IAvailableClassificationsService>(),
                Mock.Of<ICalculationRunService>(),
                Mock.Of<IBillingFileService>());

            // Act
            var actionResult = await controller.ClassificationByFinancialYear(request) as ObjectResult;

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, actionResult.StatusCode);
            Assert.AreEqual("An unexpected error occurred.", actionResult.Value);
        }

        private static Mock<ApplicationDBContext> MockDbContextForCalculatorRunClassifications()
        {
            var mockClassifications = new List<CalculatorRunClassification>
            {
                new() { Id = 1, Status = "IN THE QUEUE", CreatedBy = "Test user" },
                new() { Id = 2, Status = "RUNNING", CreatedBy = "Test user" },
                new() { Id = 3, Status = "UNCLASSIFIED", CreatedBy = "Test user" },
                new() { Id = 4, Status = "TEST RUN", CreatedBy = "Test user" },
                new() { Id = 5, Status = "ERROR", CreatedBy = "Test user" },
                new() { Id = 6, Status = "DELETED", CreatedBy = "Test user" },
                new() { Id = 7, Status = "INITIAL RUN COMPLETED", CreatedBy = "Test user" },
                new() { Id = 8, Status = "INITIAL RUN", CreatedBy = "Test user" },
                new() { Id = 9, Status = "INTERIM RE-CALCULATION RUN", CreatedBy = "Test user" },
                new() { Id = 10, Status = "FINAL RUN", CreatedBy = "Test user" },
                new() { Id = 11, Status = "FINAL RE-CALCULATION RUN", CreatedBy = "Test user" },
            }.AsQueryable();

            var mockDbContext = new Mock<ApplicationDBContext>();
            var mockClassificationsDbSet = new Mock<DbSet<CalculatorRunClassification>>();
            mockClassificationsDbSet.As<IQueryable<CalculatorRunClassification>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<CalculatorRunClassification>(mockClassifications.Provider));
            mockClassificationsDbSet.As<IQueryable<CalculatorRunClassification>>().Setup(m => m.Expression).Returns(mockClassifications.Expression);
            mockClassificationsDbSet.As<IQueryable<CalculatorRunClassification>>().Setup(m => m.ElementType).Returns(mockClassifications.ElementType);
            mockClassificationsDbSet.As<IQueryable<CalculatorRunClassification>>().Setup(m => m.GetEnumerator()).Returns(mockClassifications.GetEnumerator());
            mockClassificationsDbSet.As<IAsyncEnumerable<CalculatorRunClassification>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<CalculatorRunClassification>(mockClassifications.GetEnumerator()));

            mockDbContext.Setup(c => c.CalculatorRunClassifications).Returns(mockClassificationsDbSet.Object);
            return mockDbContext;
        }
    }
}