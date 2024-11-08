using EPR.Calculator.API.Builder.Lapcap;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Tests.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests.Builder
{
    [TestClass]
    public class CalcResultLapcapDataBuilderTest
    {
        public CalcResultLapcapDataBuilder builder;
        protected ApplicationDBContext? dbContext;

        [TestInitialize]
        public void DataSetup()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                                    .UseInMemoryDatabase(databaseName: "PayCal")
                                    .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                                                .Options;

            dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();
            dbContext.DefaultParameterTemplateMasterList.RemoveRange(dbContext.DefaultParameterTemplateMasterList);
            dbContext.SaveChanges();
            dbContext.DefaultParameterTemplateMasterList.AddRange(BaseControllerTest.GetDefaultParameterTemplateMasterData().ToList());
            // dbContext.LapcapDataTemplateMaster.AddRange(BaseControllerTest.GetLapcapTemplateMasterData().ToList());
            dbContext.SaveChanges();

            builder = new CalcResultLapcapDataBuilder(dbContext);
        }

        [TestMethod]
        public void ConstructTest()
        {
            var run = new CalculatorRun
            {
                Id = 1,
                CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                Name = "Test Run",
                Financial_Year = "2024-25",
                CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User",
                LapcapDataMasterId = 2
            };

            var detail = new LapcapDataDetail
            {
                Id = 1,
                LapcapDataMasterId = 2,
                UniqueReference = "ENG-AL",
                TotalCost = 30.99m
            };
            var lapcapDataMaster = new LapcapDataMaster
            {
                Id = 2,
                ProjectionYear = "2024-25",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,
                EffectiveFrom = DateTime.Now,
            };
            detail.LapcapDataMaster = lapcapDataMaster;


            dbContext.LapcapDataMaster.Add(lapcapDataMaster);
            dbContext.LapcapDataDetail.Add(detail);
            dbContext.SaveChanges();

            dbContext.CalculatorRuns.Add(run);
            dbContext.SaveChanges();

            var resultsDto = new CalcResultsRequestDto { RunId = 1 };
            var lapcapResults = builder.Construct(resultsDto);
        }
    }
}
