using Azure.Messaging.ServiceBus;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.UnitTests.Helpers;
using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Wrapper;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Azure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class BaseControllerTest
    {
        public BaseControllerTest()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: "PayCal")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

            this.DbContext = new ApplicationDBContext(dbContextOptions);
            this.DbContext.Database.EnsureCreated();
            this.DbContext.DefaultParameterTemplateMasterList.RemoveRange(
                this.DbContext.DefaultParameterTemplateMasterList);
            this.DbContext.SaveChanges();

            this.DbContext.DefaultParameterTemplateMasterList.AddRange(
                GetDefaultParameterTemplateMasterData().ToList());
            this.DbContext.SaveChanges();

            var validator = new CreateDefaultParameterDataValidator(this.DbContext);
            this.DefaultParameterSettingController = new DefaultParameterSettingController(this.DbContext, validator, new TelemetryClient());
            ILapcapDataValidator lapcapDataValidator = new LapcapDataValidator(this.DbContext);
            this.LapcapDataController = new LapcapDataController(this.DbContext, lapcapDataValidator, new TelemetryClient());

            this.Wrapper = new Mock<IOrgAndPomWrapper>().Object;
            var mockStorageService = new Mock<IStorageService>();
            var mockServiceBusService = new Mock<IServiceBusService>();
            var mockFactory = new Mock<IAzureClientFactory<ServiceBusClient>>();
            var mockClient = new Mock<ServiceBusClient>();
            var mockServiceBusSender = new Mock<ServiceBusSender>();
            mockServiceBusSender.Setup(msbs => msbs.SendMessageAsync(It.IsAny<ServiceBusMessage>(), default)).Returns(Task.CompletedTask);
            mockClient.Setup(mc => mc.CreateSender(It.IsAny<string>())).Returns(mockServiceBusSender.Object);

            mockFactory.Setup(m => m.CreateClient(It.IsAny<string>())).Returns(mockClient.Object);

            this.FinancialYear24_25 = new CalculatorRunFinancialYear { Name = "2024-25" };
            this.DbContext.FinancialYears.Add(this.FinancialYear24_25);

            this.DbContext.CalculatorRuns.AddRange(this.GetCalculatorRuns());
            this.DbContext.SaveChanges();
            this.CalculatorController = new CalculatorController(
                this.DbContext,
                ConfigurationItems.GetConfigurationValues(),
                mockStorageService.Object,
                mockServiceBusService.Object);

            this.DbContext.Material.RemoveRange(this.DbContext.Material.ToList());
            this.DbContext.SaveChanges();
            this.DbContext.Material.AddRange(GetMaterials());
            this.DbContext.SaveChanges();
        }

        protected ApplicationDBContext DbContext { get; set; }

        protected CalculatorRunFinancialYear FinancialYear24_25 { get; init; }

        protected DefaultParameterSettingController DefaultParameterSettingController { get; set; }

        protected LapcapDataController LapcapDataController { get; set; }

        protected CalculatorController CalculatorController { get; set; }

        protected IOrgAndPomWrapper Wrapper { get; set; }

        public static IEnumerable<DefaultParameterTemplateMaster> GetDefaultParameterTemplateMasterData()
        {
            var list = new List<DefaultParameterTemplateMaster>
            {
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "COMC-ENG",
                    ParameterCategory = "England",
                    ParameterType = "Communication costs by country",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "COMC-NIR",
                    ParameterCategory = "Northern Ireland",
                    ParameterType = "Communication costs by country",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "COMC-SCT",
                    ParameterCategory = "Scotland",
                    ParameterType = "Communication costs by country",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "COMC-UK",
                    ParameterCategory = "United Kingdom",
                    ParameterType = "Communication costs by country",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "COMC-WLS",
                    ParameterCategory = "Wales",
                    ParameterType = "Communication costs by country",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "COMC-AL",
                    ParameterCategory = "Aluminium",
                    ParameterType = "Communication costs by material",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "COMC-FC",
                    ParameterCategory = "Fibre composite",
                    ParameterType = "Communication costs by material",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "COMC-GL",
                    ParameterCategory = "Glass",
                    ParameterType = "Communication costs by material",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "COMC-PC",
                    ParameterCategory = "Paper or card",
                    ParameterType = "Communication costs by material",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "COMC-PL",
                    ParameterCategory = "Plastic",
                    ParameterType = "Communication costs by material",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "COMC-ST",
                    ParameterCategory = "Steel",
                    ParameterType = "Communication costs by material",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "COMC-WD",
                    ParameterCategory = "Wood",
                    ParameterType = "Communication costs by material",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "LRET-FC",
                    ParameterCategory = "Fibre composite",
                    ParameterType = "Late reporting tonnage",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "LRET-GL",
                    ParameterCategory = "Glass",
                    ParameterType = "Late reporting tonnage",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "LRET-AL",
                    ParameterCategory = "Aluminium",
                    ParameterType = "Late reporting tonnage",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "LRET-WD",
                    ParameterCategory = "Wood",
                    ParameterType = "Late reporting tonnage",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "LRET-ST",
                    ParameterCategory = "Steel",
                    ParameterType = "Late reporting tonnage",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "LRET-PC",
                    ParameterCategory = "Paper or card",
                    ParameterType = "Late reporting tonnage",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "LAPC-ENG",
                    ParameterCategory = "England",
                    ParameterType = "Local authority data preparation costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "LAPC-NIR",
                    ParameterCategory = "Northern Ireland",
                    ParameterType = "Local authority data preparation costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "LAPC-SCT",
                    ParameterCategory = "Scotland",
                    ParameterType = "Local authority data preparation costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "LAPC-WLS",
                    ParameterCategory = "Wales",
                    ParameterType = "Local authority data preparation costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "MATT-AD",
                    ParameterCategory = "Amount Decrease",
                    ParameterType = "Materiality threshold",
                    ValidRangeFrom = -999999999.990m,
                    ValidRangeTo = 0.00m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "MATT-AI",
                    ParameterCategory = "Amount Increase",
                    ParameterType = "Materiality threshold",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "MATT-PD",
                    ParameterCategory = "Percent Decrease",
                    ParameterType = "Materiality threshold",
                    ValidRangeFrom = -999.990m,
                    ValidRangeTo = 0.00m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "MATT-PI",
                    ParameterCategory = "Percent Increase",
                    ParameterType = "Materiality threshold",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999.990m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "COMC-OT",
                    ParameterCategory = "Other",
                    ParameterType = "Other materials",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "LRET-OT",
                    ParameterCategory = "Other materials",
                    ParameterType = "Late reporting tonnage",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "BADEBT-P",
                    ParameterCategory = "Bad debt provision",
                    ParameterType = "Percentage",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 1000.000m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "LRET-PL",
                    ParameterCategory = "Plastic",
                    ParameterType = "Late reporting tonnage",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "SAOC-ENG",
                    ParameterCategory = "England",
                    ParameterType = "Scheme administrator operating costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "SAOC-NIR",
                    ParameterCategory = "Northern Ireland",
                    ParameterType = "Scheme administrator operating costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "SAOC-SCT",
                    ParameterCategory = "Scotland",
                    ParameterType = "Scheme administrator operating costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "SAOC-WLS",
                    ParameterCategory = "Wales",
                    ParameterType = "Scheme administrator operating costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "SCSC-ENG",
                    ParameterCategory = "England",
                    ParameterType = "Scheme setup costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "SCSC-NIR",
                    ParameterCategory = "Northern Ireland",
                    ParameterType = "Scheme setup costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "SCSC-SCT",
                    ParameterCategory = "Scotland",
                    ParameterType = "Scheme setup costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "SCSC-WLS",
                    ParameterCategory = "Wales",
                    ParameterType = "Scheme setup costs",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "TONT-AD",
                    ParameterCategory = "Amount Decrease",
                    ParameterType = "Tonnage change threshold",
                    ValidRangeFrom = -999999999.990m,
                    ValidRangeTo = 0.00m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "TONT-AI",
                    ParameterCategory = "Amount Increase",
                    ParameterType = "Tonnage change threshold",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999999999.99m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "TONT-PD",
                    ParameterCategory = "Percent Decrease",
                    ParameterType = "Tonnage change threshold",
                    ValidRangeFrom = -999.990m,
                    ValidRangeTo = 0.00m,
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "TONT-PI",
                    ParameterCategory = "Percent Increase",
                    ParameterType = "Tonnage change threshold",
                    ValidRangeFrom = 0m,
                    ValidRangeTo = 999.990m,
                },
            };
            return list;
        }

        public static IEnumerable<LapcapDataTemplateMaster> GetLapcapTemplateMasterData()
        {
            var list = new List<LapcapDataTemplateMaster>
            {
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "ENG-AL",
                    Country = "England",
                    Material = "Aluminium",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "ENG-FC",
                    Country = "England",
                    Material = "Fibre composite",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "ENG-GL",
                    Country = "England",
                    Material = "Glass",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "ENG-PC",
                    Country = "England",
                    Material = "Paper or card",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "ENG-PL",
                    Country = "England",
                    Material = "Plastic",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "ENG-ST",
                    Country = "England",
                    Material = "Steel",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "ENG-WD",
                    Country = "England",
                    Material = "Wood",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "ENG-OT",
                    Country = "England",
                    Material = "Other",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "NI-AL",
                    Country = "Northern Ireland",
                    Material = "Aluminium",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "NI-FC",
                    Country = "Northern Ireland",
                    Material = "Fibre composite",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "NI-GL",
                    Country = "Northern Ireland",
                    Material = "Glass",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "NI-PC",
                    Country = "Northern Ireland",
                    Material = "Paper or card",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "NI-PL",
                    Country = "Northern Ireland",
                    Material = "Plastic",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "NI-ST",
                    Country = "Northern Ireland",
                    Material = "Steel",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "NI-WD",
                    Country = "Northern Ireland",
                    Material = "Wood",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "NI-OT",
                    Country = "Northern Ireland",
                    Material = "Other",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "SCT-AL",
                    Country = "Scotland",
                    Material = "Aluminium",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "SCT-FC",
                    Country = "Scotland",
                    Material = "Fibre composite",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "SCT-GL",
                    Country = "Scotland",
                    Material = "Glass",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "SCT-PC",
                    Country = "Scotland",
                    Material = "Paper or card",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "SCT-PL",
                    Country = "Scotland",
                    Material = "Plastic",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "SCT-ST",
                    Country = "Scotland",
                    Material = "Steel",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "SCT-WD",
                    Country = "Scotland",
                    Material = "Wood",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "SCT-OT",
                    Country = "Scotland",
                    Material = "Other",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "WLS-AL",
                    Country = "Wales",
                    Material = "Aluminium",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "WLS-FC",
                    Country = "Wales",
                    Material = "Fibre composite",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "WLS-GL",
                    Country = "Wales",
                    Material = "Glass",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "WLS-PC",
                    Country = "Wales",
                    Material = "Paper or card",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "WLS-PL",
                    Country = "Wales",
                    Material = "Plastic",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "WLS-ST",
                    Country = "Wales",
                    Material = "Steel",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "WLS-WD",
                    Country = "Wales",
                    Material = "Wood",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
                new LapcapDataTemplateMaster
                {
                    UniqueReference = "WLS-OT",
                    Country = "Wales",
                    Material = "Other",
                    TotalCostFrom = 0M,
                    TotalCostTo = 999999999.99M,
                },
            };
            return list;
        }

        [TestMethod]
        public void CheckDbContext()
        {
            Assert.IsNotNull(this.DbContext);
            Assert.IsTrue(this.DbContext.Database.IsInMemory());
        }

        [TestCleanup]
        public void TearDown()
        {
            this.DbContext.Database.EnsureDeleted();
        }

        protected static IEnumerable<Material> GetMaterials()
        {
            var list = new List<Material>
            {
                new Material
                {
                    Id = 1,
                    Code = "AL",
                    Name = "Aluminium",
                    Description = "Aluminium",
                },
                new Material
                {
                    Id = 2,
                    Code = "FC",
                    Name = "Fibre composite",
                    Description = "Fibre composite",
                },
                new Material
                {
                    Id = 3,
                    Code = "GL",
                    Name = "Glass",
                    Description = "Glass",
                },
                new Material
                {
                    Id = 4,
                    Code = "PC",
                    Name = "Paper or card",
                    Description = "Paper or card",
                },
                new Material
                {
                    Id = 5,
                    Code = "PL",
                    Name = "Plastic",
                    Description = "Plastic",
                },
                new Material
                {
                    Id = 6,
                    Code = "ST",
                    Name = "Steel",
                    Description = "Steel",
                },
                new Material
                {
                    Id = 7,
                    Code = "WD",
                    Name = "Wood",
                    Description = "Wood",
                },
                new Material
                {
                    Id = 8,
                    Code = "OT",
                    Name = "Other materials",
                    Description = "Other materials",
                },
            };
            return list;
        }

        protected static IEnumerable<CalculatorRunPomDataMaster> GetCalculatorRunPomDataMaster()
        {
            var list = new List<CalculatorRunPomDataMaster>
            {
                new CalculatorRunPomDataMaster
                {
                    Id = 1,
                    CalendarYear = "2024-25",
                    EffectiveFrom = DateTime.Now,
                    CreatedBy = "Test user",
                    CreatedAt = DateTime.Now,
                },
            };
            return list;
        }

        protected static IEnumerable<CalculatorRunPomDataDetail> GetCalculatorRunPomDataDetails()
        {
            var list = new List<CalculatorRunPomDataDetail>
            {
                new CalculatorRunPomDataDetail
                {
                    Id = 1,
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
                    CalculatorRunPomDataMaster = GetCalculatorRunPomDataMaster().ToList()[0],
                },
            };
            return list;
        }

        protected static IEnumerable<CalculatorRunOrganisationDataMaster> GetCalculatorRunOrganisationDataMaster()
        {
            var list = new List<CalculatorRunOrganisationDataMaster>
            {
                new CalculatorRunOrganisationDataMaster
                {
                    Id = 1,
                    CalendarYear = "2024-25",
                    EffectiveFrom = DateTime.Now,
                    CreatedBy = "Test user",
                    CreatedAt = DateTime.Now,
                },
            };
            return list;
        }

        protected static IEnumerable<CalculatorRunOrganisationDataDetail> GetCalculatorRunOrganisationDataDetails()
        {
            var list = new List<CalculatorRunOrganisationDataDetail>();
            list.AddRange(new List<CalculatorRunOrganisationDataDetail>
            {
                new CalculatorRunOrganisationDataDetail
                {
                    Id = 1,
                    OrganisationId = 1,
                    OrganisationName = "UPU LIMITED",
                    LoadTimeStamp = DateTime.Now,
                    CalculatorRunOrganisationDataMasterId = 1,
                    SubmissionPeriodDesc = "July to December 2023",
                    CalculatorRunOrganisationDataMaster = new CalculatorRunOrganisationDataMaster
                    {
                        Id = 1,
                        CalendarYear = "2024-25",
                        EffectiveFrom = DateTime.Now,
                        CreatedBy = "Test user",
                        CreatedAt = DateTime.Now,
                    },
                },
                new CalculatorRunOrganisationDataDetail
                {
                    Id = 2,
                    OrganisationId = 1,
                    SubsidaryId = "SUBSID1",
                    OrganisationName = "UPU LIMITED",
                    LoadTimeStamp = DateTime.Now,
                    CalculatorRunOrganisationDataMasterId = 1,
                    SubmissionPeriodDesc = "July to December 2023",
                    CalculatorRunOrganisationDataMaster = new CalculatorRunOrganisationDataMaster
                    {
                        Id = 1,
                        CalendarYear = "2024-25",
                        EffectiveFrom = DateTime.Now,
                        CreatedBy = "Test user",
                        CreatedAt = DateTime.Now,
                    },
                },
            });
            return list;
        }

        protected IEnumerable<CalculatorRun> GetCalculatorRuns()
        {
            var list = new List<CalculatorRun>
            {
                new CalculatorRun
                {
                    CalculatorRunClassificationId = (int)RunClassification.INTHEQUEUE,
                    Name = "Test Run",
                    Financial_Year = this.FinancialYear24_25,
                    CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                    CreatedBy = "Test User",
                },
                new CalculatorRun
                {
                    CalculatorRunClassificationId = (int)RunClassification.INTHEQUEUE,
                    Name = "Test Calculated Result",
                    Financial_Year = this.FinancialYear24_25,
                    CreatedAt = new DateTime(2024, 8, 21, 14, 16, 27, DateTimeKind.Utc),
                    CreatedBy = "Test User",
                },
                new CalculatorRun
                {
                    CalculatorRunClassificationId = (int)RunClassification.INTHEQUEUE,
                    Name = "Test Run",
                    Financial_Year = this.FinancialYear24_25,
                    CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                    CreatedBy = "Test User",
                    CalculatorRunOrganisationDataMasterId = 1,
                    CalculatorRunPomDataMasterId = 1,
                },
                new CalculatorRun
                {
                    CalculatorRunClassificationId = (int)RunClassification.INTHEQUEUE,
                    Name = "Test 422 error",
                    Financial_Year = this.FinancialYear24_25,
                    CreatedAt = new DateTime(2024, 8, 21, 14, 16, 27, DateTimeKind.Utc),
                    CreatedBy = "Test User",
                    CalculatorRunOrganisationDataMasterId = 2,
                    CalculatorRunPomDataMasterId = 2,
                    LapcapDataMasterId = 2,
                    DefaultParameterSettingMasterId = 2,
                },
                new CalculatorRun
                {
                    CalculatorRunClassificationId = (int)RunClassification.INTHEQUEUE,
                    Name = "Test Calculated Result",
                    Financial_Year = this.FinancialYear24_25,
                    CreatedAt = new DateTime(2024, 8, 21, 14, 16, 27, DateTimeKind.Utc),
                    CreatedBy = "Test User",
                    CalculatorRunOrganisationDataMasterId = 2,
                    CalculatorRunPomDataMasterId = 2,
                },
            };
            return list;
        }
    }
}