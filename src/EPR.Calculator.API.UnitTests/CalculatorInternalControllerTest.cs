using EPR.Calculator.API.Builder;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Exporter;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Services;
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
                OrganisationId = 1234,
                SubsidaryId = "4455",
                LoadTimeStamp = DateTime.Now,
                CalculatorRunPomDataMaster = pomMaster,
                Id = 0,
                SubmissionPeriod = "some-period",
                CalculatorRunPomDataMasterId = pomMaster.Id,
                SubmissionPeriodDesc= "some-period-desc",
            };

            this.dbContext?.CalculatorRunPomDataDetails.Add(pomDataDetail);

            this.dbContext?.SaveChanges();

            var organisationMaster = this.dbContext?.CalculatorRunOrganisationDataMaster.ToList();
            var calcRun = this.dbContext?.CalculatorRuns.Single(run => run.Id == 1);
            if (calcRun != null)
            {
                calcRun.CalculatorRunPomDataMaster = pomMaster;
                calcRun.CalculatorRunPomDataMasterId = pomMaster.Id;
            }

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
            var calcRun = this.dbContext?.CalculatorRuns.Single(x => x.Id == 1);
            if (calcRun!=null)
            {
                calcRun.CalculatorRunClassificationId = 3;
                this.dbContext?.CalculatorRuns.Update(calcRun);
            }
            
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
            Assert.AreEqual(201, objResult?.StatusCode);
            var updatedRun = this.dbContext?.CalculatorRuns.Single(x => x.Id == 1);
            Assert.IsNotNull(updatedRun);
            Assert.AreEqual(5, updatedRun.CalculatorRunClassificationId);
        }

        [TestMethod]
        public void UpdateRpdStatus_With_RunId_Having_Pom_Data_Missing()
        {
            var request = new Dtos.UpdateRpdStatus { isSuccessful = true, RunId = 1, UpdatedBy = "User1" };
            var result = this.calculatorInternalController?.UpdateRpdStatus(request);
            var objResult = result as ObjectResult;
            Assert.AreEqual(422, objResult?.StatusCode);
            Assert.AreEqual("PomData or Organisation Data is missing", objResult?.Value);
        }

        [TestMethod]
        public void UpdateRpdStatus_With_RunId_When_Successful()
        {
            var organisation = new OrganisationData
            {
                LoadTimestamp = DateTime.UtcNow,
                OrganisationName = "OrgName",
                OrganisationId = 1345,
                SubsidaryId = "Sub1d1",
                SubmissionPeriodDesc = "some-period-desc"
            };
            var pomData = new PomData
            {
                LoadTimeStamp = DateTime.Now,
                OrganisationId = 1567,
                SubsidaryId = "Sub1d1",
                PackagingType = "packtype",
                PackagingClass = "class1",
                PackagingMaterial = "Aluminium",
                PackagingMaterialWeight = 200,
                SubmissionPeriod = "2024-25",
                SubmissionPeriodDesc = "November 2024 to June 2025"
            };
            var pomDataList = new List<PomData> { pomData };
            var organisationDataList = new List<OrganisationData> { organisation };
            var mock = new Mock<IOrgAndPomWrapper>();
            mock.Setup(x => x.AnyPomData()).Returns(true);
            mock.Setup(x => x.AnyOrganisationData()).Returns(true);
            mock.Setup(x => x.GetOrganisationData()).Returns(organisationDataList);
            mock.Setup(x => x.GetPomData()).Returns(pomDataList);

            if (dbContext != null)
            {
                var controller = new CalculatorInternalController(
                    dbContext,
                    new RpdStatusDataValidator(mock.Object),
                    mock.Object,
                    new Mock<ICalcResultBuilder>().Object,
                    new Mock<ICalcResultsExporter<CalcResult>>().Object,
                    new Mock<ITransposePomAndOrgDataService>().Object
                );

                var request = new Dtos.UpdateRpdStatus { isSuccessful = true, RunId = 1, UpdatedBy = "User1" };
                var result = controller?.UpdateRpdStatus(request);

                var objResult = result as ObjectResult;
                Assert.AreEqual(201, objResult?.StatusCode);
                var calcRun = dbContext.CalculatorRuns.Single(x => x.Id == 1);
                Assert.IsNotNull(calcRun);
                Assert.AreEqual(2, calcRun.CalculatorRunClassificationId);
                Assert.IsNotNull(calcRun.CalculatorRunOrganisationDataMasterId);
                Assert.IsNotNull(calcRun.CalculatorRunPomDataMasterId);
            }

        }

        //[TestMethod]
        //public void Construct_ShouldReturnCalcResultDetail()
        //{
        //    var mock = new Mock<IOrgAndPomWrapper>();
        //    dbContext.LapcapDataMaster.RemoveRange(dbContext.LapcapDataMaster);
        //    dbContext.SaveChanges();
        //    dbContext.LapcapDataMaster.AddRange(GetLapcapMasterData().ToList());
        //    dbContext.SaveChanges();

        //    dbContext.DefaultParameterSettings.RemoveRange(dbContext.DefaultParameterSettings);
        //    dbContext.SaveChanges();
        //    dbContext.DefaultParameterSettings.AddRange(GetDefaultParameterSettingsMasterData().ToList());
        //    dbContext.SaveChanges();

        //    var mockBlobStorageService = new Mock<IBlobStorageService>();
        //    var CalcResultsExporter = new CalcResultsExporter(mockBlobStorageService.Object);
        //    var CalcResultDetailBuilder = new CalcResultDetailBuilder(dbContext);
        //    var CalcResultLapcapDataBuilder = new CalcResultLapcapDataBuilder(dbContext);
        //    var calcResultBuilder = new CalcResultBuilder(CalcResultDetailBuilder, CalcResultLapcapDataBuilder);
        //    if (dbContext != null)
        //    {
        //        var controller = new CalculatorInternalController(
        //            dbContext,
        //            new RpdStatusDataValidator(mock.Object),
        //            mock.Object,
        //            calcResultBuilder,
        //            CalcResultsExporter
        //        );
        //        var calResult = controller.PrepareCalcResults(new CalcResultsRequestDto() { RunId = 1 });
        //        var objResult = calResult as ObjectResult;
        //        Assert.IsNotNull(calResult);
        //        Assert.AreEqual(201, objResult?.StatusCode);
        //    }
        //}

        protected static IEnumerable<DefaultParameterSettingMaster> GetDefaultParameterSettingsMasterData()
        {
            var list = new List<DefaultParameterSettingMaster>
            {
                new() {
                    CreatedAt = DateTime.Now,
                    CreatedBy = "Test User1",
                    EffectiveFrom = DateTime.Now,
                    EffectiveTo = DateTime.Now,
                    Id = 1,
                },
                new() {
                    CreatedAt = DateTime.Now,
                    CreatedBy = "Test User2",
                    EffectiveFrom = DateTime.Now,
                    EffectiveTo = DateTime.Now,
                    Id = 2,
                }
            };

            return list;
        }

        protected static IEnumerable<LapcapDataMaster> GetLapcapMasterData()
        {
            var list = new List<LapcapDataMaster>
            {
                new() {
                    CreatedAt = DateTime.Now,
                    CreatedBy = "Test User1",
                    EffectiveFrom = DateTime.Now,
                    EffectiveTo = DateTime.Now,
                    Id = 1,
                    ProjectionYear = "2024-25",
                },
                new() {
                    CreatedAt = DateTime.Now,
                    CreatedBy = "Test User2",
                    EffectiveFrom = DateTime.Now,
                    EffectiveTo = DateTime.Now,
                    Id = 2,
                    ProjectionYear = "2024-25",
                }
            };

            return list;
        }
    }
}
