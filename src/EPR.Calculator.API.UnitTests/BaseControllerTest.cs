using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPR.Calculator.API.UnitTests.Helpers;
using Moq;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Wrapper;
using EPR.Calculator.API.Builder;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Exporter;
using EPR.Calculator.API.Services;

namespace EPR.Calculator.API.Tests.Controllers
{
    [TestClass]
    public class BaseControllerTest
    {
        protected ApplicationDBContext? dbContext;
        protected DefaultParameterSettingController? defaultParameterSettingController;
        protected LapcapDataController? lapcapDataController;
        protected CalculatorController? calculatorController;
        protected CalculatorInternalController? calculatorInternalController;
        protected IOrgAndPomWrapper? wrapper;

        [TestInitialize]
        public void SetUp()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: "PayCal")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

            dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();
            dbContext.DefaultParameterTemplateMasterList.RemoveRange(dbContext.DefaultParameterTemplateMasterList);
            dbContext.SaveChanges();
            dbContext.DefaultParameterTemplateMasterList.AddRange(GetDefaultParameterTemplateMasterData().ToList());
            dbContext.SaveChanges();

            var validator = new CreateDefaultParameterDataValidator(dbContext);
            defaultParameterSettingController = new DefaultParameterSettingController(dbContext, validator);
            ILapcapDataValidator lapcapDataValidator = new LapcapDataValidator(dbContext);
            lapcapDataController = new LapcapDataController(dbContext, lapcapDataValidator);

             
            wrapper = new Mock<IOrgAndPomWrapper>().Object;
            calculatorInternalController = new CalculatorInternalController(
                dbContext,
                new RpdStatusDataValidator(wrapper),
                wrapper, 
                new Mock<ICalcResultBuilder>().Object,
                new Mock<ICalcResultsExporter<CalcResult>>().Object,
                new Mock<ITransposePomAndOrgDataService>().Object
            );

            var mockFactory = new Mock<IAzureClientFactory<ServiceBusClient>>();
            var mockClient = new Mock<ServiceBusClient>();
            var mockServiceBusSender = new Mock<ServiceBusSender>();
            mockServiceBusSender.Setup(msbs => msbs.SendMessageAsync(It.IsAny<ServiceBusMessage>(), default(CancellationToken))).Returns(Task.CompletedTask);
            mockClient.Setup(mc => mc.CreateSender(It.IsAny<string>())).Returns(mockServiceBusSender.Object);

            mockFactory.Setup(m => m.CreateClient(It.IsAny<string>())).Returns(mockClient.Object);

            dbContext.CalculatorRuns.AddRange(GetCalculatorRuns());
            dbContext.SaveChanges();
            calculatorController = new CalculatorController(dbContext, ConfigurationItems.GetConfigurationValues(), mockFactory.Object);

            dbContext.Material.AddRange(GetMaterials());
            dbContext.SaveChanges();
        }

        public void CheckDbContext()
        {
            Assert.IsNotNull(dbContext);
            Assert.IsTrue(dbContext.Database.IsInMemory());
        }

        [TestCleanup]
        public void TearDown()
        {
            dbContext?.Database.EnsureDeleted();
        }

        protected static IEnumerable<DefaultParameterTemplateMaster> GetDefaultParameterTemplateMasterData()
        {
            var list = new List<DefaultParameterTemplateMaster>();
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-ENG",
                ParameterCategory = "England",
                ParameterType = "Communication costs by country",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-NIR",
                ParameterCategory = "Northern Ireland",
                ParameterType = "Communication costs by country",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-SCT",
                ParameterCategory = "Scotland",
                ParameterType = "Communication costs by country",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-UK",
                ParameterCategory = "United Kingdom",
                ParameterType = "Communication costs by country",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-WLS",
                ParameterCategory = "Wales",
                ParameterType = "Communication costs by country",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-AL",
                ParameterCategory = "Aluminium",
                ParameterType = "Communication costs by material",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-FC",
                ParameterCategory = "Fibre composite",
                ParameterType = "Communication costs by material",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-GL",
                ParameterCategory = "Glass",
                ParameterType = "Communication costs by material",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-PC",
                ParameterCategory = "Paper or card",
                ParameterType = "Communication costs by material",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-PL",
                ParameterCategory = "Plastic",
                ParameterType = "Communication costs by material",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-ST",
                ParameterCategory = "Steel",
                ParameterType = "Communication costs by material",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-WD",
                ParameterCategory = "Wood",
                ParameterType = "Communication costs by material",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-FC",
                ParameterCategory = "Fibre composite",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-GL",
                ParameterCategory = "Glass",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-AL",
                ParameterCategory = "Aluminium",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-WD",
                ParameterCategory = "Wood",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-ST",
                ParameterCategory = "Steel",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-PC",
                ParameterCategory = "Paper or card",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LAPC-ENG",
                ParameterCategory = "England",
                ParameterType = "Local authority data preparation costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LAPC-NIR",
                ParameterCategory = "Northern Ireland",
                ParameterType = "Local authority data preparation costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LAPC-SCT",
                ParameterCategory = "Scotland",
                ParameterType = "Local authority data preparation costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LAPC-WLS",
                ParameterCategory = "Wales",
                ParameterType = "Local authority data preparation costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "MATT-AD",
                ParameterCategory = "Amount Decrease",
                ParameterType = "Materiality threshold",
                ValidRangeFrom = -999999999.990m,
                ValidRangeTo = 0.00m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "MATT-AI",
                ParameterCategory = "Amount Increase",
                ParameterType = "Materiality threshold",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "MATT-PD",
                ParameterCategory = "Percent Decrease",
                ParameterType = "Materiality threshold",
                ValidRangeFrom = -999.990m,
                ValidRangeTo = 0.00m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "MATT-PI",
                ParameterCategory = "Percent Increase",
                ParameterType = "Materiality threshold",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999.990m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-OT",
                ParameterCategory = "Other",
                ParameterType = "Other materials",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-OT",
                ParameterCategory = "Other materials",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "BADEBT-P",
                ParameterCategory = "Bad debt provision",
                ParameterType = "Percentage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 1000.000m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-PL",
                ParameterCategory = "Plastic",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SAOC-ENG",
                ParameterCategory = "England",
                ParameterType = "Scheme administrator operating costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SAOC-NIR",
                ParameterCategory = "Northern Ireland",
                ParameterType = "Scheme administrator operating costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SAOC-SCT",
                ParameterCategory = "Scotland",
                ParameterType = "Scheme administrator operating costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SAOC-WLS",
                ParameterCategory = "Wales",
                ParameterType = "Scheme administrator operating costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SCSC-ENG",
                ParameterCategory = "England",
                ParameterType = "Scheme setup costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SCSC-NIR",
                ParameterCategory = "Northern Ireland",
                ParameterType = "Scheme setup costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SCSC-SCT",
                ParameterCategory = "Scotland",
                ParameterType = "Scheme setup costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SCSC-WLS",
                ParameterCategory = "Wales",
                ParameterType = "Scheme setup costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "TONT-AD",
                ParameterCategory = "Amount Decrease",
                ParameterType = "Tonnage change threshold",
                ValidRangeFrom = -999999999.990m,
                ValidRangeTo = 0.00m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "TONT-AI",
                ParameterCategory = "Amount Increase",
                ParameterType = "Tonnage change threshold",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "TONT-PD",
                ParameterCategory = "Percent Decrease",
                ParameterType = "Tonnage change threshold",
                ValidRangeFrom = -999.990m,
                ValidRangeTo = 0.00m
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "TONT-PI",
                ParameterCategory = "Percent Increase",
                ParameterType = "Tonnage change threshold",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999.990m
            });
            return list;
        }
        
        protected static IEnumerable<LapcapDataTemplateMaster> GetLapcapTemplateMasterData()
        {
            var list = new List<LapcapDataTemplateMaster>();
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "ENG-AL",
                Country = "England",
                Material = "Aluminium",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "ENG-FC",
                Country = "England",
                Material = "Fibre composite",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "ENG-GL",
                Country = "England",
                Material = "Glass",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "ENG-PC",
                Country = "England",
                Material = "Paper or card",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "ENG-PL",
                Country = "England",
                Material = "Plastic",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "ENG-ST",
                Country = "England",
                Material = "Steel",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "ENG-WD",
                Country = "England",
                Material = "Wood",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "ENG-OT",
                Country = "England",
                Material = "Other",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "NI-AL",
                Country = "NI",
                Material = "Aluminium",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "NI-FC",
                Country = "NI",
                Material = "Fibre composite",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "NI-GL",
                Country = "NI",
                Material = "Glass",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "NI-PC",
                Country = "NI",
                Material = "Paper or card",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "NI-PL",
                Country = "NI",
                Material = "Plastic",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "NI-ST",
                Country = "NI",
                Material = "Steel",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "NI-WD",
                Country = "NI",
                Material = "Wood",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "NI-OT",
                Country = "NI",
                Material = "Other",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "SCT-AL",
                Country = "Scotland",
                Material = "Aluminium",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "SCT-FC",
                Country = "Scotland",
                Material = "Fibre composite",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "SCT-GL",
                Country = "Scotland",
                Material = "Glass",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "SCT-PC",
                Country = "Scotland",
                Material = "Paper or card",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "SCT-PL",
                Country = "Scotland",
                Material = "Plastic",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "SCT-ST",
                Country = "Scotland",
                Material = "Steel",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "SCT-WD",
                Country = "Scotland",
                Material = "Wood",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "SCT-OT",
                Country = "Scotland",
                Material = "Other",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "WLS-AL",
                Country = "Wales",
                Material = "Aluminium",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "WLS-FC",
                Country = "Wales",
                Material = "Fibre composite",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "WLS-GL",
                Country = "Wales",
                Material = "Glass",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "WLS-PC",
                Country = "Wales",
                Material = "Paper or card",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "WLS-PL",
                Country = "Wales",
                Material = "Plastic",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "WLS-ST",
                Country = "Wales",
                Material = "Steel",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "WLS-WD",
                Country = "Wales",
                Material = "Wood",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            list.Add(new LapcapDataTemplateMaster
            {
                UniqueReference = "WLS-OT",
                Country = "Wales",
                Material = "Other",
                TotalCostFrom = 0M,
                TotalCostTo = 999999999.99M,
            });
            return list;
        }

        protected static IEnumerable<CalculatorRun> GetCalculatorRuns()
        {
            var list = new List<CalculatorRun>();
            list.Add(new CalculatorRun
            {
                Id = 1,
                CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                Name = "Test Run",
                Financial_Year = "2024-25",
                CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User"
            });
            list.Add(new CalculatorRun
            {
                Id = 2,
                CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                Name = "Test Calculated Result",
                Financial_Year = "2024-25",
                CreatedAt = new DateTime(2024, 8, 21, 14, 16, 27, DateTimeKind.Utc),
                CreatedBy = "Test User"
            });
            list.Add(new CalculatorRun
            {
                Id = 3,
                CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                Name = "Test Run",
                Financial_Year = "2024-25",
                CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User",
                CalculatorRunOrganisationDataMasterId = 1,
                CalculatorRunPomDataMasterId = 1,
            });
            list.Add(new CalculatorRun
            {
                Id = 4,
                CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                Name = "Test Calculated Result",
                Financial_Year = "2024-25",
                CreatedAt = new DateTime(2024, 8, 21, 14, 16, 27, DateTimeKind.Utc),
                CreatedBy = "Test User",
                CalculatorRunOrganisationDataMasterId = 2,
                CalculatorRunPomDataMasterId = 2,
            });
            return list;
        }

        protected static IEnumerable<Material> GetMaterials()
        {
            var list = new List<Material>();
            list.Add(new Material
            {
                Id = 1,
                Code = "AL",
                Name = "Aluminium",
                Description = "Aluminium"
            });
            list.Add(new Material
            {
                Id = 2,
                Code = "FC",
                Name = "Fibre composite",
                Description = "Fibre composite"
            });
            list.Add(new Material
            {
                Id = 3,
                Code = "GL",
                Name = "Glass",
                Description = "Glass"
            });
            list.Add(new Material
            {
                Id = 4,
                Code = "PC",
                Name = "Paper or card",
                Description = "Paper or card"
            });
            list.Add(new Material
            {
                Id = 5,
                Code = "PL",
                Name = "Plastic",
                Description = "Plastic"
            });
            list.Add(new Material
            {
                Id = 6,
                Code = "ST",
                Name = "Steel",
                Description = "Steel"
            });
            list.Add(new Material
            {
                Id = 7,
                Code = "WD",
                Name = "Wood",
                Description = "Wood"
            });
            list.Add(new Material
            {
                Id = 8,
                Code = "OT",
                Name = "Other materials",
                Description = "Other materials"
            });
            return list;
        }

        protected static IEnumerable<CalculatorRunPomDataMaster> GetCalculatorRunPomDataMaster()
        {
            var list = new List<CalculatorRunPomDataMaster>();
            list.Add(new CalculatorRunPomDataMaster
            {
                Id = 1,
                CalendarYear = "2024-25",
                EffectiveFrom = DateTime.Now,
                CreatedBy = "Test user",
                CreatedAt  = DateTime.Now
            });
            return list;
        }

        protected static IEnumerable<CalculatorRunPomDataDetail> GetCalculatorRunPomDataDetails()
        {
            var list = new List<CalculatorRunPomDataDetail>();
            list.Add(new CalculatorRunPomDataDetail
            {
                Id= 1,
                OrganisationId = 1,
                SubsidaryId = "SUBSID1",
                SubmissionPeriod = "2023-P3",
                PackagingActivity = null,
                PackagingType = "CW",
                PackagingClass = "O1",
                PackagingMaterial = "PC",
                PackagingMaterialWeight = 1000,
                LoadTimeStamp = DateTime.Now,
                CalculatorRunPomDataMasterId = 1,
                SubmissionPeriodDesc = "July to December 2023",
                CalculatorRunPomDataMaster = BaseControllerTest.GetCalculatorRunPomDataMaster().ToList()[0]
            });
            return list;
        }

        protected static IEnumerable<CalculatorRunOrganisationDataMaster> GetCalculatorRunOrganisationDataMaster()
        {
            var list = new List<CalculatorRunOrganisationDataMaster>();
            list.Add(new CalculatorRunOrganisationDataMaster
            {
                CalendarYear = "2024-25",
                EffectiveFrom = DateTime.Now,
                CreatedBy = "Test user",
                CreatedAt = DateTime.Now
            });
            return list;
        }

        protected static IEnumerable<CalculatorRunOrganisationDataDetail> GetCalculatorRunOrganisationDataDetails()
        {
            var list = new List<CalculatorRunOrganisationDataDetail>();
            list.Add(new CalculatorRunOrganisationDataDetail
            {
                Id = 1,
                OrganisationId = 1,
                SubsidaryId = "SUBSID1",
                OrganisationName = "UPU LIMITED",
                LoadTimeStamp= DateTime.Now,
                CalculatorRunOrganisationDataMasterId = 1,
                SubmissionPeriodDesc = "January to June 2023",
                CalculatorRunOrganisationDataMaster = BaseControllerTest.GetCalculatorRunOrganisationDataMaster().ToList()[0]
            });
            return list;
        }
    }
}
