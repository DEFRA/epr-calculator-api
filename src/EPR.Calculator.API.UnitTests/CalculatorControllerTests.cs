﻿using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Tests.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class CalculatorControllerTests : BaseControllerTest
    {
        //[TestMethod]
        //public async Task Create_Calculator_Run()
        //{
        //    var createCalculatorRunDto = new CreateCalculatorRunDto
        //    {
        //        CalculatorRunName = "Test calculator run",
        //        CreatedBy = "Test user",
        //        FinancialYear = "2024-25"
        //    };

        //    dbContext.DefaultParameterSettings.Add(new DefaultParameterSettingMaster
        //    {
        //        Id = 1,
        //        ParameterYear = "2024-25",
        //        CreatedBy = "Testuser",
        //        CreatedAt = DateTime.Now,
        //        EffectiveFrom = DateTime.Now,
        //        EffectiveTo = null
        //    });
        //    dbContext.SaveChanges();

        //    dbContext.LapcapDataMaster.Add(new LapcapDataMaster
        //    {
        //        Id = 1,
        //        ProjectionYear = "2024-25",
        //        CreatedBy = "Testuser",
        //        CreatedAt = DateTime.Now,
        //        EffectiveFrom = DateTime.Now,
        //        EffectiveTo = null
        //    });
        //    dbContext.SaveChanges();

        //    var actionResult = await calculatorController?.Create(createCalculatorRunDto) as ObjectResult;
        //    Assert.IsNotNull(actionResult);
        //    Assert.AreEqual(202, actionResult.StatusCode);
        //}

        [TestMethod]
        public async Task Create_Calculator_Run_Return_404_If_No_Default_Parameter_Settings_And_Lapcap_Data()
        {
            var createCalculatorRunDto = new CreateCalculatorRunDto
            {
                CalculatorRunName = "Test calculator run",
                CreatedBy = "Test user",
                FinancialYear = "2024-25"
            };

            dbContext.DefaultParameterSettings.Add(new DefaultParameterSettingMaster
            {
                Id = 1,
                ParameterYear = "2023-24",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,
                EffectiveFrom = DateTime.Now,
                EffectiveTo = null
            });
            dbContext.SaveChanges();

            dbContext.LapcapDataMaster.Add(new LapcapDataMaster
            {
                Id = 1,
                ProjectionYear = "2023-24",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,
                EffectiveFrom = DateTime.Now,
                EffectiveTo = null
            });
            dbContext.SaveChanges();

            var actionResult = await calculatorController?.Create(createCalculatorRunDto) as ObjectResult;
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

            dbContext.DefaultParameterSettings.Add(new DefaultParameterSettingMaster
            {
                Id = 1,
                ParameterYear = "2023-24",
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
                FinancialYear = "2024-25"
            };

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
                ProjectionYear = "2023-24",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,
                EffectiveFrom = DateTime.Now,
                EffectiveTo = null
            });
            dbContext.SaveChanges();

            var actionResult = await calculatorController?.Create(createCalculatorRunDto) as ObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(424, actionResult.StatusCode);
            Assert.AreEqual("Lapcap data not available for the financial year 2024-25.", actionResult.Value);
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
    }
}
