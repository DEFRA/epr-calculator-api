using EPR.Calculator.API.Tests.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class CalculatorInternalControllerTest : BaseControllerTest
    {
        [TestMethod]
        public void UpdateRpdStatus_With_Missing_RunId()
        {
            var request = new Dtos.UpdateRpdStatus { isSuccessful = false, RunId = 999, UpdatedBy = "User1" };
            var result = this.calculatorInternalController?.UpdateRpdStatus(request);
            var objResult = result as ObjectResult;
            Assert.IsNotNull(objResult);
            Assert.AreEqual(objResult.StatusCode, 400);
            Assert.AreEqual(objResult.Value, "Calculator Run 999 is missing");
        }

        [TestMethod]
        public void UpdateRpdStatus_With_RunId_Having_OrganisationDataMasterId()
        {
            this.dbContext?.SaveChanges();

            var organisationMaster = this.dbContext?.CalculatorRunOrganisationDataMaster.ToList();
            this.dbContext?.CalculatorRuns.Add(new CalculatorRun
            {
                CalculatorRunClassificationId = 1,
                CreatedAt = DateTime.Now,
                CreatedBy = "Some User",
                Financial_Year = "2024-25",
                Name = "CalculationRun1-Test",
                DefaultParameterSettingMasterId = 1,
                LapcapDataMasterId = 1,
                CalculatorRunOrganisationDataMasterId = 1,
            });
            
            this.dbContext?.SaveChanges();


            var request = new Dtos.UpdateRpdStatus { isSuccessful = false, RunId = 3, UpdatedBy = "User1" };
            var result = this.calculatorInternalController?.UpdateRpdStatus(request);
            var objResult = result as ObjectResult;
            Assert.IsNotNull(objResult);
            Assert.AreEqual(objResult.StatusCode, 422);
            Assert.AreEqual(objResult.Value, "Calculator Run 3 already has OrganisationDataMasterId associated with it");
        }

        [TestMethod]
        public void UpdateRpdStatus_With_RunId_Having_PomDataMasterId()
        {
            var pomMaster = new CalculatorRunPomDataMaster
            {
                CalendarYear = "2023",
                CreatedAt = DateTime.Now,
                CreatedBy = "Some User",
                EffectiveFrom = DateTime.Now,
                EffectiveTo = null,
            };

            var pomDataDetail = new CalculatorRunPomDataDetail
            {
                OrganisationId = "1234",
                SubsidaryId = "4455",
                LoadTimeStamp = DateTime.Now,
                CalculatorRunPomDataMaster = pomMaster,
                Id = 0,
                SubmissionPeriod = "some-period",
                CalculatorRunPomDataMasterId = pomMaster.Id,
            };

            this.dbContext?.CalculatorRunPomDataDetails.Add(pomDataDetail);

            this.dbContext?.SaveChanges();

            var organisationMaster = this.dbContext?.CalculatorRunOrganisationDataMaster.ToList();
            var calcRun = this.dbContext?.CalculatorRuns.Single(run => run.Id == 1);
            calcRun.CalculatorRunPomDataMaster = pomMaster;
            calcRun.CalculatorRunPomDataMasterId = pomMaster.Id;

            this.dbContext?.SaveChanges();


            var request = new Dtos.UpdateRpdStatus { isSuccessful = false, RunId = 1, UpdatedBy = "User1" };
            var result = this.calculatorInternalController?.UpdateRpdStatus(request);
            var objResult = result as ObjectResult;
            Assert.IsNotNull(objResult);
            Assert.AreEqual(objResult.StatusCode, 422);
            Assert.AreEqual(objResult.Value, "Calculator Run 1 already has PomDataMasterId associated with it");
        }

        [TestMethod]
        public void UpdateRpdStatus_With_RunId_With_Incorrect_Classification()
        {
            var calcRun = this.dbContext.CalculatorRuns.Single(x => x.Id == 1);
            calcRun.CalculatorRunClassificationId = 3;
            this.dbContext.CalculatorRuns.Update(calcRun);

            var request = new Dtos.UpdateRpdStatus { isSuccessful = false, RunId = 1, UpdatedBy = "User1" };
            var result = this.calculatorInternalController?.UpdateRpdStatus(request);
            var objResult = result as ObjectResult;
            Assert.IsNotNull(objResult);
            Assert.AreEqual(objResult.StatusCode, 422);
            Assert.AreEqual(objResult.Value, "Calculator Run 1 classification should be RUNNING or IN THE QUEUE");
        }

        public void UpdateRpdStatus_With_RunId_When_Not_Successful()
        {
        }

        public void UpdateRpdStatus_With_RunId_Having_Pom_Data_Missing()
        {
        }

        public void UpdateRpdStatus_With_RunId_Having_Organisation_Data_Missing()
        {
        }
    }
}
