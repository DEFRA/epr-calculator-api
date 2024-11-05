using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Tests.Controllers;
using EPR.Calculator.API.Wrapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests.Services
{
    [TestClass]
    public class TransposePomAndOrgDataServiceTests : BaseControllerTest
    {
        [TestMethod]
        public void Transpose_Should_Return_Correct_Producer_Detail()
        {
            var expectedResult = new ProducerDetail
            {
                Id = 1,
                ProducerId = 1,
                SubsidiaryId = "SUBSID1",
                ProducerName = "UPU LIMITED",
                CalculatorRunId = 1,
                CalculatorRun = new CalculatorRun()
            };

            var pomDataList = new List<PomData>();
            pomDataList.Add(new PomData
            {
                OrganisationId = 1,
                SubsidaryId = "SUBSID1",
                SubmissionPeriod = "2023-P3",
                PackagingActivity = null,
                PackagingType = "CW",
                PackagingClass = "O1",
                PackagingMaterial = "PC",
                PackagingMaterialWeight = 1000,
                LoadTimeStamp = DateTime.Now,
                SubmissionPeriodDesc = "July to December 2023"
            });

#
            var mockWrapper = new Mock<IOrgAndPomWrapper>();

            mockWrapper.Setup(x => x.GetPomData()).Returns(pomDataList);

            var service = new TransposePomAndOrgDataService(dbContext, mockWrapper.Object);

            service.Transpose(1);



            var producerDetail = dbContext.ProducerDetail.FirstOrDefault();
            Assert.AreEqual(expectedResult, producerDetail);
        }

        [TestMethod]
        public void Transpose_Should_Return_Correct_Producer_Reported_Material()
        {
            var expectedResult = new ProducerReportedMaterial
            {
                Id = 1,
                MaterialId = 4,
                ProducerDetailId = 1,
                PackagingType = "CW",
                PackagingTonnage = 1,
                Material = new Material{
                    Id = 1,
                    Code = "AL",
                    Name = "Aluminium",
                    Description = "Aluminium"
                },
                ProducerDetail = new ProducerDetail
                {
                    Id = 1,
                    ProducerId = 1,
                    SubsidiaryId = "SUBSID1",
                    ProducerName = "UPU LIMITED",
                    CalculatorRunId = 1,
                    CalculatorRun = new CalculatorRun()
                }
            };

            TransposePomAndOrgDataService.Transpose(dbContext, 1);

            var producerReportedMaterial = dbContext.ProducerReportedMaterial.FirstOrDefault();
            Assert.AreEqual(expectedResult, producerReportedMaterial);
        }
    }
}
