using System.Net;
using EPR.Calculator.API.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests.Dtos
{
    [TestClass]
    public class ProducerBillingInstructionsDtoTests
    {
        [TestMethod]
        public void CanSetAndGetProperties_ProducerBillingInstructionsDto()
        {
            var dto = new ProducerBillingInstructionsDto
            {
                ProducerName = "Test Producer",
                ProducerId = 123,
                SuggestedBillingInstruction = "Invoice",
                SuggestedInvoiceAmount = 100.50m,
                BillingInstructionAcceptReject = "Accepted",
            };

            Assert.AreEqual("Test Producer", dto.ProducerName);
            Assert.AreEqual(123, dto.ProducerId);
            Assert.AreEqual("Invoice", dto.SuggestedBillingInstruction);
            Assert.AreEqual(100.50m, dto.SuggestedInvoiceAmount);
            Assert.AreEqual("Accepted", dto.BillingInstructionAcceptReject);
        }

        [TestMethod]
        public void CanSetAndGetProperties_ProducerBillingInstructionsRequestDto()
        {
            var searchQuery = new ProducerBillingInstructionsSearchQueryDto
            {
                OrganisationId = 5,
                Status = new List<string> { "Accepted", "Rejected" },
            };

            var dto = new ProducerBillingInstructionsRequestDto
            {
                SearchQuery = searchQuery,
                PageNumber = 2,
                PageSize = 50,
            };

            Assert.AreEqual(searchQuery, dto.SearchQuery);
            Assert.AreEqual(2, dto.PageNumber);
            Assert.AreEqual(50, dto.PageSize);
        }

        [TestMethod]
        public void CanSetAndGetProperties_ProducerBillingInstructionsResponseDto()
        {
            var record = new ProducerBillingInstructionsDto { ProducerId = 1, ProducerName = "A", SuggestedBillingInstruction = "Invoice", SuggestedInvoiceAmount = 1.0m, BillingInstructionAcceptReject = "Accepted" };
            var dto = new ProducerBillingInstructionsResponseDto
            {
                Records = new List<ProducerBillingInstructionsDto> { record },
                TotalRecords = 1,
                TotalRecordsByStatus = new List<ProducerBillingInstructionsStatus>
                {
                    new ProducerBillingInstructionsStatus { Status = "Accepted", TotalRecords = 1 },
                    new ProducerBillingInstructionsStatus { Status = "Rejected", TotalRecords = 0 },
                },
                PageNumber = 1,
                PageSize = 10,
                RunName = "Test Run",
                CalculatorRunId = 1,
            };

            Assert.AreEqual("Test Run", dto.RunName);
            Assert.AreEqual(1, dto.CalculatorRunId);
            Assert.AreEqual(1, dto.Records.Count);
            Assert.AreEqual(1, dto.TotalRecords);
            Assert.AreEqual(1, dto.PageNumber);
            Assert.AreEqual(10, dto.PageSize);
        }
    }
}