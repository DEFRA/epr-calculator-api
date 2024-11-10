using EPR.Calculator.API.Builder;
using EPR.Calculator.API.Builder.LateReportingTonnages;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests.Builder
{
    [TestClass]
    public class CalcResultLateReportingBuilderTest
    {
        public required CalcResultLateReportingBuilder builder;
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
            dbContext.DefaultParameterSettingDetail.RemoveRange(dbContext.DefaultParameterSettingDetail);
            dbContext.CalculatorRuns.RemoveRange(dbContext.CalculatorRuns);
            dbContext.SaveChanges();

            dbContext.DefaultParameterTemplateMasterList.AddRange(new List<DefaultParameterTemplateMaster>
            {
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "LRET-AL",
                    ParameterCategory = "Aluminium",
                    ParameterType = "Late Reporting Tonnage",
                    ValidRangeFrom = 0.000M,
                    ValidRangeTo = 999999999.999M
                },
                new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = "LRET-FC",
                    ParameterCategory = "Fibre composite",
                    ParameterType = "Late Reporting Tonnage",
                    ValidRangeFrom = 0.000M,
                    ValidRangeTo = 999999999.999M
                }
            });

            dbContext.DefaultParameterSettingDetail.AddRange(new List<DefaultParameterSettingDetail>
            {
                new DefaultParameterSettingDetail
                {
                    DefaultParameterSettingMasterId = 1,
                    ParameterUniqueReferenceId = "LRET-AL",
                    ParameterValue = 100.000M
                },
                new DefaultParameterSettingDetail
                {
                    DefaultParameterSettingMasterId = 1,
                    ParameterUniqueReferenceId = "LRET-FC",
                    ParameterValue = 200.000M
                }
            });
            
            dbContext.CalculatorRuns.Add(new CalculatorRun
            {
                Id = 1,
                CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                Name = "Test Run",
                Financial_Year = "2024-25",
                CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User",
                LapcapDataMasterId = 2,
                DefaultParameterSettingMasterId = 1
            });

            dbContext.SaveChanges();

            builder = new CalcResultLateReportingBuilder(dbContext);
        }

        public ApplicationDBContext? GetDbContext()
        {
            return dbContext;
        }

        [TestMethod]
        public void Construct_ShouldReturnCorrectResults()
        {
            var requestDto = new CalcResultsRequestDto { RunId = 1 };

            var result = builder.Construct(requestDto);

            Assert.IsNotNull(result);
            Assert.AreEqual(CalcResultLateReportingBuilder.LateReportingHeader, result.Name);

            var material1 = result.CalcResultLateReportingTonnageDetails.SingleOrDefault(x => x.Name == "Aluminium");
            Assert.IsNotNull(material1);
            Assert.AreEqual(100.000M, material1.TotalLateReportingTonnage);

            var material2 = result.CalcResultLateReportingTonnageDetails.SingleOrDefault(x => x.Name == "Fibre composite");
            Assert.IsNotNull(material2);
            Assert.AreEqual(200.000M, material2.TotalLateReportingTonnage);

            var total = result.CalcResultLateReportingTonnageDetails.SingleOrDefault(x => x.Name == CalcResultLateReportingBuilder.Total);
            Assert.IsNotNull(total);
            Assert.AreEqual(300.000M, total.TotalLateReportingTonnage);
        }
    }
}