using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Tests.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

            TransposePomAndOrgDataService.Transpose(dbContext, 1);

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
