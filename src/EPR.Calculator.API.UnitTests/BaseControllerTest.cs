using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.Tests.Controllers
{
    [TestClass]
    public class BaseControllerTest
    {
        protected ApplicationDBContext? dbContext;
        protected DefaultParameterSettingController? defaultParameterSettingController;
        protected LapcapDataController? lapcapDataController;

        [TestInitialize]
        public void SetUp()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: "PayCal")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

            dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();
            var percentDecreses = dbContext.DefaultParameterTemplateMasterList.Where(x => x.ValidRangeTo < 0).ToList();
            foreach (var percent in percentDecreses) 
            {
                percent.ValidRangeFrom = percent.ValidRangeTo;
                percent.ValidRangeTo = 0M;
            }
            var tontDI = dbContext.DefaultParameterTemplateMasterList.SingleOrDefault(x => x.ParameterUniqueReferenceId == "TONT-DI");

            var tontAd = new DefaultParameterTemplateMaster
            {
                ParameterCategory = "Tonnage change threshold",
                ParameterUniqueReferenceId = "TONT-AD",
                ParameterType = "Amount Decrease",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99M
            };
            tontAd.ParameterUniqueReferenceId = "TONT-AD";
#pragma warning disable CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.
            dbContext.Entry(tontDI).State = EntityState.Deleted;
#pragma warning restore CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.
            dbContext.DefaultParameterTemplateMasterList.Add(tontAd);
            dbContext.SaveChanges();

            var validator = new CreateDefaultParameterDataValidator(dbContext);
            defaultParameterSettingController = new DefaultParameterSettingController(dbContext, validator);
            ILapcapDataValidator lapcapDataValidator = new LapcapDataValidator(dbContext);
            lapcapDataController = new LapcapDataController(dbContext, lapcapDataValidator);
        }

        [TestMethod]
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
        
        protected static IEnumerable<LapcapDataTemplateMaster> GetTemplateMasterData()
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
    }
}
