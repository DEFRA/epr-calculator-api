using EPR.Calculator.API.Builder;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Tests.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.API.UnitTests.Builder
{
    [TestClass]
    public class CalcResultOnePlusFourApportionmentBuilderTest
    {
        private CalcResultOnePlusFourApportionmentBuilder builder;
        private ApplicationDBContext dbContext;

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

            builder = new CalcResultOnePlusFourApportionmentBuilder(dbContext);
        }

        [TestMethod]
        public void Construct_ShouldReturnCorrectApportionment()
        {
            // Arrange
            var resultsDto = new CalcResultsRequestDto { RunId = 6 };
            var calcResult = new CalcResult
            {
                CalcResultLapcapData = new CalcResultLapcapData
                {
                    Name = "LAPCAP Data",
                    CalcResultLapcapDataDetails = new[]
                    {
                        new CalcResultLapcapDataDetails
                        {
                            Name = "1 Fee for LA Disposal Costs",
                            EnglandDisposalCost = "£100.00",
                            WalesDisposalCost = "£200.00",
                            ScotlandDisposalCost = "£300.00",
                            NorthernIrelandDisposalCost = "£400.00",
                            TotalDisposalCost = "£1000.00",
                            OrderId = 1
                        } ,
                    }
                },

                CalcResultParameterOtherCost = new CalcResultParameterOtherCost
                {
                    Name = "",
                    Details = new[]
                    {
                        new CalcResultParameterOtherCostDetail
                        {
                            Name = "4 LA Data Prep Charge",
                            England= "£100.00",
                            Wales = "£200.00",
                            Scotland = "£300.00",
                            NorthernIreland = "£400.00",
                            Total = "£1000.00",
                            OrderId = 2
                        } ,
                    }

                }
            };

            new CalcResultOnePlusFourApportionmentDetail 


            var lapcapDisposalCostResults = builder.Construct(resultsDto, calcResult);

            //Assert

        }

    }
}
