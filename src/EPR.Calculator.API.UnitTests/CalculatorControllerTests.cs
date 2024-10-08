using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Tests.Controllers;
using EPR.Calculator.API.UnitTests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests
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

            var actionResult = await calculatorController?.Create(createCalculatorRunDto);
            Assert.IsNotNull(actionResult);
            // Assert.AreEqual(202, actionResult.StatusCode);
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
