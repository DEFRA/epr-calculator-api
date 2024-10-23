using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Tests.Controllers;
using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Wrapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
            Assert.AreEqual(400, objResult.StatusCode);
            Assert.AreEqual("Calculator Run 999 is missing", objResult.Value);
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
            Assert.AreEqual(422, objResult.StatusCode);
            Assert.AreEqual("Calculator Run 3 already has OrganisationDataMasterId associated with it", objResult.Value);
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
            Assert.AreEqual(422, objResult.StatusCode);
            Assert.AreEqual("Calculator Run 1 already has PomDataMasterId associated with it", objResult.Value);
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
            Assert.AreEqual(422, objResult.StatusCode);
            Assert.AreEqual("Calculator Run 1 classification should be RUNNING or IN THE QUEUE", objResult.Value);
        }

        [TestMethod]
        public void UpdateRpdStatus_With_RunId_When_Not_Successful()
        {
            var request = new Dtos.UpdateRpdStatus { isSuccessful = false, RunId = 1, UpdatedBy = "User1" };
            var result = this.calculatorInternalController?.UpdateRpdStatus(request);
            var objResult = result as ObjectResult;
            Assert.AreEqual(objResult.StatusCode, 201);
            var updatedRun = this.dbContext.CalculatorRuns.Single(x => x.Id == 1);
            Assert.IsNotNull(updatedRun);
            Assert.AreEqual(5, updatedRun.CalculatorRunClassificationId);
        }

        [TestMethod]
        public void UpdateRpdStatus_With_RunId_Having_Pom_Data_Missing()
        {
            var request = new Dtos.UpdateRpdStatus { isSuccessful = true, RunId = 1, UpdatedBy = "User1" };
            var result = this.calculatorInternalController?.UpdateRpdStatus(request);
            var objResult = result as ObjectResult;
            Assert.AreEqual(422, objResult.StatusCode);
            Assert.AreEqual("PomData or Organisation Data is missing", objResult.Value);
        }

        [TestMethod]
        public void UpdateRpdStatus_With_RunId_When_Successful()
        {
            var organisation = new OrganisationData
            {
                LoadTimestamp = DateTime.UtcNow,
                OrganisationName = "OrgName",
                OrganisationId = "OrgId1",
                SubsidaryId = "Sub1d1"
            };
            var pomData = new PomData
            {
                LoadTimeStamp = DateTime.Now,
                OrganisationId = "OrgId1",
                SubsidaryId = "Sub1d1",
                PackagingType = "packtype",
                PackagingClass = "class1",
                PackagingMaterial = "Aluminium",
                PackagingMaterialWeight = "200",
                SubmissionPeriod = "2024-25",
            };
            var pomDataList = new List<PomData> { pomData };
            var organisationDataList = new List<OrganisationData> { organisation };
            var mock = new Mock<IOrgAndPomWrapper>();
            mock.Setup(x => x.AnyPomData()).Returns(true);
            mock.Setup(x => x.AnyOrganisationData()).Returns(true);
            mock.Setup(x => x.GetOrganisationData()).Returns(organisationDataList);
            mock.Setup(x => x.GetPomData()).Returns(pomDataList);

            var controller = new CalculatorInternalController(dbContext, new RpdStatusDataValidator(mock.Object), mock.Object);

            var request = new Dtos.UpdateRpdStatus { isSuccessful = true, RunId = 1, UpdatedBy = "User1" };
            var result = controller?.UpdateRpdStatus(request);

            var objResult = result as ObjectResult;
            Assert.AreEqual(objResult.StatusCode, 201);
            var calcRun = dbContext.CalculatorRuns.Single(x => x.Id == 1);
            Assert.IsNotNull(calcRun);
            Assert.AreEqual(2, calcRun.CalculatorRunClassificationId);
            Assert.IsNotNull(calcRun.CalculatorRunOrganisationDataMasterId);
            Assert.IsNotNull(calcRun.CalculatorRunPomDataMasterId);
        }
    }
}
