using Azure.Messaging.ServiceBus;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Tests.Controllers;
using EPR.Calculator.API.UnitTests.Helpers;
using EPR.Calculator.API.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class CalculatorControllerTests : BaseControllerTest
    {
        [TestMethod]
        public async Task Create_Calculator_Run()
        {
            var createCalculatorRunDto = new CreateCalculatorRunDto
            {
                CalculatorRunName = "Test calculator run",
                CreatedBy = "Test user",
                FinancialYear = "2024-25"
            };

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            dbContext.DefaultParameterSettings.Add(new DefaultParameterSettingMaster
            {
                Id = 1,
                ParameterYear = "2024-25",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,
                EffectiveFrom = DateTime.Now,
                EffectiveTo = null
            });
            dbContext.SaveChanges();

            dbContext.LapcapDataMaster.Add(new LapcapDataMaster
            {
                Id = 1,
                ProjectionYear = "2024-25",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,
                EffectiveFrom = DateTime.Now,
                EffectiveTo = null
            });
            dbContext.SaveChanges();

            var actionResult = await calculatorController?.Create(createCalculatorRunDto) as ObjectResult;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(202, actionResult.StatusCode);
        }

        [TestMethod]
        public async Task Create_Calculator_Run_Return_404_If_No_Default_Parameter_Settings_And_Lapcap_Data()
        {
            var createCalculatorRunDto = new CreateCalculatorRunDto
            {
                CalculatorRunName = "Test calculator run",
                CreatedBy = "Test user",
                FinancialYear = "2024-25"
            };

            dbContext?.DefaultParameterSettings.Add(new DefaultParameterSettingMaster
            {
                Id = 1,
                ParameterYear = "2023-24",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,
                EffectiveFrom = DateTime.Now,
                EffectiveTo = null
            });
            dbContext?.SaveChanges();

            dbContext?.LapcapDataMaster.Add(new LapcapDataMaster
            {
                Id = 1,
                ProjectionYear = "2023-24",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,
                EffectiveFrom = DateTime.Now,
                EffectiveTo = null
            });
            dbContext?.SaveChanges();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var actionResult = await calculatorController?.Create(createCalculatorRunDto) as ObjectResult;
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
                CreatedBy = "Test user",
                FinancialYear = "2024-25"
            };

            dbContext?.DefaultParameterSettings.Add(new DefaultParameterSettingMaster
            {
                Id = 1,
                ParameterYear = "2023-24",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,
                EffectiveFrom = DateTime.Now,
                EffectiveTo = null
            });
            dbContext?.SaveChanges();

            dbContext?.LapcapDataMaster.Add(new LapcapDataMaster
            {
                Id = 1,
                ProjectionYear = "2024-25",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,
                EffectiveFrom = DateTime.Now,
                EffectiveTo = null
            });
            dbContext?.SaveChanges();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var actionResult = await calculatorController?.Create(createCalculatorRunDto) as ObjectResult;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(424, actionResult.StatusCode);
            Assert.AreEqual("Default parameter settings not available for the financial year 2024-25.", actionResult.Value);
        }

        [TestMethod]
        public async Task Create_Calculator_Run_Return_404_If_No_Lapcap_Data()
        {
            var createCalculatorRunDto = new CreateCalculatorRunDto
            {
                CalculatorRunName = "Test calculator run",
                CreatedBy = "Test user",
                FinancialYear = "2027-28"
            };

            dbContext?.DefaultParameterSettings.Add(new DefaultParameterSettingMaster
            {
                Id = 1,
                ParameterYear = "2027-28",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,
                EffectiveFrom = DateTime.Now,
                EffectiveTo = null
            });
            dbContext?.SaveChanges();

            dbContext?.LapcapDataMaster.Add(new LapcapDataMaster
            {
                Id = 1,
                ProjectionYear = "2023-24",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,
                EffectiveFrom = DateTime.Now,
                EffectiveTo = null
            });
            dbContext?.SaveChanges();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var actionResult = await calculatorController?.Create(createCalculatorRunDto) as ObjectResult;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
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
                CreatedBy = "Test user",
                FinancialYear = "2024-25"
            };

            dbContext?.DefaultParameterSettings.Add(new DefaultParameterSettingMaster
            {
                Id = 1,
                ParameterYear = "2024-25",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,
                EffectiveFrom = DateTime.Now,
                EffectiveTo = null
            });
            dbContext?.SaveChanges();

            dbContext?.LapcapDataMaster.Add(new LapcapDataMaster
            {
                Id = 1,
                ProjectionYear = "2024-25",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,
                EffectiveFrom = DateTime.Now,
                EffectiveTo = null
            });
            dbContext?.SaveChanges();

            var configs = ConfigurationItems.GetConfigurationValues();
            configs.GetSection("ServiceBus").GetSection("ConnectionString").Value = string.Empty;

            var mockFactory = new Mock<IAzureClientFactory<ServiceBusClient>>();

#pragma warning disable CS8604 // Possible null reference argument.
            calculatorController = new CalculatorController(dbContext, configs, mockFactory.Object);
#pragma warning restore CS8604 // Possible null reference argument.

            var actionResult = await calculatorController.Create(createCalculatorRunDto) as ObjectResult;

            Assert.IsNotNull(actionResult);
            Assert.AreEqual(500, actionResult.StatusCode);
            Assert.AreEqual("Configuration item not found: ServiceBus__ConnectionString", actionResult.Value);
        }

        [TestMethod]
        public async Task Create_Calculator_Run_Return_500_If_QueueName_Configuration_Is_Empty()
        {
            var createCalculatorRunDto = new CreateCalculatorRunDto
            {
                CalculatorRunName = "Test calculator run",
                CreatedBy = "Test user",
                FinancialYear = "2024-25"
            };

            dbContext?.DefaultParameterSettings.Add(new DefaultParameterSettingMaster
            {
                Id = 1,
                ParameterYear = "2024-25",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,
                EffectiveFrom = DateTime.Now,
                EffectiveTo = null
            });
            dbContext?.SaveChanges();

            dbContext?.LapcapDataMaster.Add(new LapcapDataMaster
            {
                Id = 1,
                ProjectionYear = "2024-25",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,
                EffectiveFrom = DateTime.Now,
                EffectiveTo = null
            });
            dbContext?.SaveChanges();

            var configs = ConfigurationItems.GetConfigurationValues();
            configs.GetSection("ServiceBus").GetSection("QueueName").Value = string.Empty;

            var mockFactory = new Mock<IAzureClientFactory<ServiceBusClient>>();

#pragma warning disable CS8604 // Possible null reference argument.
            calculatorController = new CalculatorController(dbContext, configs, mockFactory.Object);
#pragma warning restore CS8604 // Possible null reference argument.

            var actionResult = await calculatorController.Create(createCalculatorRunDto) as ObjectResult;

            Assert.IsNotNull(actionResult);
            Assert.AreEqual(500, actionResult.StatusCode);
            Assert.AreEqual("Configuration item not found: ServiceBus__QueueName", actionResult.Value);
        }

        [TestMethod]
        public async Task Create_Calculator_Run_Return_500_If_PostMessageRetryCount_Configuration_Is_Empty()
        {
            var createCalculatorRunDto = new CreateCalculatorRunDto
            {
                CalculatorRunName = "Test calculator run",
                CreatedBy = "Test user",
                FinancialYear = "2024-25"
            };

            dbContext?.DefaultParameterSettings.Add(new DefaultParameterSettingMaster
            {
                Id = 1,
                ParameterYear = "2024-25",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,
                EffectiveFrom = DateTime.Now,
                EffectiveTo = null
            });
            dbContext?.SaveChanges();

            dbContext?.LapcapDataMaster.Add(new LapcapDataMaster
            {
                Id = 1,
                ProjectionYear = "2024-25",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,
                EffectiveFrom = DateTime.Now,
                EffectiveTo = null
            });
            dbContext?.SaveChanges();

            var configs = ConfigurationItems.GetConfigurationValues();
            configs.GetSection("ServiceBus").GetSection("PostMessageRetryCount").Value = string.Empty;

            var mockFactory = new Mock<IAzureClientFactory<ServiceBusClient>>();

#pragma warning disable CS8604 // Possible null reference argument.
            calculatorController = new CalculatorController(dbContext, configs, mockFactory.Object);
#pragma warning restore CS8604 // Possible null reference argument.

            var actionResult = await calculatorController.Create(createCalculatorRunDto) as ObjectResult;

            Assert.IsNotNull(actionResult);
            Assert.AreEqual(500, actionResult.StatusCode);
            Assert.AreEqual("Configuration item not found: ServiceBus__PostMessageRetryCount", actionResult.Value);
        }

        [TestMethod]
        public async Task Create_Calculator_Run_Return_500_If_PostMessageRetryPeriod_Configuration_Is_Empty()
        {
            var createCalculatorRunDto = new CreateCalculatorRunDto
            {
                CalculatorRunName = "Test calculator run",
                CreatedBy = "Test user",
                FinancialYear = "2024-25"
            };

            dbContext?.DefaultParameterSettings.Add(new DefaultParameterSettingMaster
            {
                Id = 1,
                ParameterYear = "2024-25",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,
                EffectiveFrom = DateTime.Now,
                EffectiveTo = null
            });
            dbContext?.SaveChanges();

            dbContext?.LapcapDataMaster.Add(new LapcapDataMaster
            {
                Id = 1,
                ProjectionYear = "2024-25",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,
                EffectiveFrom = DateTime.Now,
                EffectiveTo = null
            });
            dbContext?.SaveChanges();

            var configs = ConfigurationItems.GetConfigurationValues();
            configs.GetSection("ServiceBus").GetSection("PostMessageRetryPeriod").Value = string.Empty;

            var mockFactory = new Mock<IAzureClientFactory<ServiceBusClient>>();

#pragma warning disable CS8604 // Possible null reference argument.
            calculatorController = new CalculatorController(dbContext, configs, mockFactory.Object);
#pragma warning restore CS8604 // Possible null reference argument.

            var actionResult = await calculatorController.Create(createCalculatorRunDto) as ObjectResult;

            Assert.IsNotNull(actionResult);
            Assert.AreEqual(500, actionResult.StatusCode);
            Assert.AreEqual("Configuration item not found: ServiceBus__PostMessageRetryPeriod", actionResult.Value);
        }

        [TestMethod]
        public void Get_Calculator_Runs_Return_Results_Test()
        {
            var runParams = new CalculatorRunsParamsDto
            {
                FinancialYear = "2024-25"
            };
            var actionResult = calculatorController?.GetCalculatorRuns(runParams) as ObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(200, actionResult.StatusCode);
        }

        [TestMethod]
        public void Get_Calculator_Runs_Return_Not_Found_Test()
        {
            var runParams = new CalculatorRunsParamsDto
            {
                FinancialYear = "2022-23"
            };
            var actionResult = calculatorController?.GetCalculatorRuns(runParams) as ObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(404, actionResult.StatusCode);
        }

        [TestMethod]
        public void Get_Calculator_Runs_Return_Bad_Request_Test()
        {
            var runParams = new CalculatorRunsParamsDto
            {
                FinancialYear = string.Empty
            };
            var actionResult = calculatorController?.GetCalculatorRuns(runParams) as ObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(400, actionResult.StatusCode);
        }

        [TestMethod]
        public void Get_Calculator_Run_Return_400_Error_With_No_NameSupplied()
        {
            CalculatorRunValidator _validator = new CalculatorRunValidator();
            string _name = string.Empty;
            var result = _validator.Validate(_name);

            Assert.IsNotNull(result);
            Assert.AreEqual("Calculator Run Name is Required", result.Errors.First().ErrorMessage);
        }

        [TestMethod]
        public void Get_Calculator_Run_Return_Results_By_Name_Test()
        {
            string calculatorRunName = "Test Run";

            var actionResult = calculatorController?.GetCalculatorRunByName(calculatorRunName) as ObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(200, actionResult.Value);
        }

        [TestMethod]
        public void Get_Calculator_Run_Return_Results_Not_found()
        {
            string calculatorRunName = "test 45610";

            var actionResult = calculatorController?.GetCalculatorRunByName(calculatorRunName) as ObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(404, actionResult.StatusCode);
        }

        [TestMethod]
        public void Get_Calculator_Run_Return_Result_With_String_Comparison_CaseInsensitive()
        {
            string calculatorRunName = "TEST run";

            var actionResult = calculatorController?.GetCalculatorRunByName(calculatorRunName) as ObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(200, actionResult.Value);
        }
    }
}