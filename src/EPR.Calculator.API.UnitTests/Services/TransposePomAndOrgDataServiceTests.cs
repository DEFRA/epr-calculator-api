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
        [TestInitialize]
        public void Setup()
        {
            dbContext.CalculatorRunPomDataDetails.AddRange(GetCalculatorRunPomDataDetails());
            dbContext.SaveChanges();

            dbContext.CalculatorRunOrganisationDataDetails.AddRange(GetCalculatorRunOrganisationDataDetails());
            dbContext.SaveChanges();
        }

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

            var service = new TransposePomAndOrgDataService(dbContext);
            service.Transpose(3);

            var producerDetail = dbContext.ProducerDetail.FirstOrDefault();
            Assert.AreEqual(expectedResult.ProducerId, producerDetail.ProducerId);
            Assert.AreEqual(expectedResult.ProducerName, producerDetail.ProducerName);
            Assert.AreEqual(expectedResult.SubsidiaryId, producerDetail.SubsidiaryId);
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
                Material = new Material
                {
                    Id = 4,
                    Code = "PC",
                    Name = "Paper or card",
                    Description = "Paper or card"
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

            var service = new TransposePomAndOrgDataService(dbContext);
            service.Transpose(3);

            var producerReportedMaterial = dbContext.ProducerReportedMaterial.FirstOrDefault();
            Assert.AreEqual(expectedResult.ProducerDetailId, producerReportedMaterial.ProducerDetailId);
            Assert.AreEqual(expectedResult.Material.Code, producerReportedMaterial.Material.Code);
            Assert.AreEqual(expectedResult.Material.Name, producerReportedMaterial.Material.Name);
            Assert.AreEqual(expectedResult.ProducerDetail.ProducerId, producerReportedMaterial.ProducerDetail.ProducerId);
            Assert.AreEqual(expectedResult.ProducerDetail.ProducerName, producerReportedMaterial.ProducerDetail.ProducerName);
        }
    }
}
