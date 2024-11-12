﻿using EPR.Calculator.API.Builder;
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
                            Name = "Total",
                            EnglandDisposalCost = "£13,280.45",
                            WalesDisposalCost = "£210.28",
                            ScotlandDisposalCost = "£161.07",
                            NorthernIrelandDisposalCost = "£91.00",
                            TotalDisposalCost = "£13,742.80",
                             EnglandCost = 13280.45m,
                             WalesCost=210.28m,
                             ScotlandCost=91.00m,
                             NorthernIrelandCost =91.00m,
                             TotalCost=13742.80m
                        } ,
                    }
                },

                CalcResultParameterOtherCost = new CalcResultParameterOtherCost
                {
                    Name = "4 LA Data Prep Charge",
                    Details = new[]
                    {
                        new CalcResultParameterOtherCostDetail
                        {
                            Name = "4 LA Data Prep Charge",
                            England= "£115.45",
                            Wales = "£114.00",
                            Scotland = "£117.00",
                            NorthernIreland = "£19.00",
                            Total = "£365.45",
                            EnglandValue= 115.45m,
                             WalesValue=114.00m,
                             ScotlandValue=117.00m,
                             NorthernIrelandValue =19.00m,
                             TotalValue=365.45m
                        } ,
                    }
                }
            };

            var resultCalc = builder.Construct(resultsDto, calcResult);
            // Assert
            Assert.IsNotNull(calcResult);
            Assert.AreEqual("1 + 4 Apportionment %s", resultCalc.Name);
            Assert.AreEqual(5, resultCalc.CalcResultOnePlusFourApportionmentDetails.Count());

            // Check header row
            var headerRow = resultCalc.CalcResultOnePlusFourApportionmentDetails.FirstOrDefault();
            Assert.AreEqual(OnePlus4ApportionmentRowHeaders.Name, headerRow.Name);
            Assert.AreEqual(OnePlus4ApportionmentRowHeaders.Total, headerRow.Total);

            // Check disposal cost row
            var disposalRow = resultCalc.CalcResultOnePlusFourApportionmentDetails.Single(x=>x.Name== "1 Fee for LA Disposal Costs");
            Assert.AreEqual("£13,742.80", disposalRow.Total);
            Assert.AreEqual("£13,280.45", disposalRow.EnglandDisposalTotal);

            // Check data preparation charge row
            var prepchargeRow = resultCalc.CalcResultOnePlusFourApportionmentDetails.Single(x => x.Name == "4 LA Data Prep Charge");
            Assert.AreEqual("£365.45", prepchargeRow.Total);
            Assert.AreEqual("£115.45", prepchargeRow.EnglandDisposalTotal);

            // Check total row
            var totalRow = resultCalc.CalcResultOnePlusFourApportionmentDetails.Single(x => x.OrderId==3); ;
            Assert.AreEqual("£14,108.25", totalRow.Total); // 13,742.80 + 365.45

            // Check apportionment row
            var apportionmentRow = resultCalc.CalcResultOnePlusFourApportionmentDetails.Single(x => x.OrderId == 4); ;
            Assert.AreEqual("100.00000000%", apportionmentRow.Total);
            Assert.AreEqual("94.95082664%", apportionmentRow.EnglandDisposalTotal);
        }
    }
}