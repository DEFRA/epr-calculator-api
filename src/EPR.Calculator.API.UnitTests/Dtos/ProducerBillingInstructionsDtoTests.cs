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
                CalculatorRunId = 1,
            };

            Assert.AreEqual("Test Producer", dto.ProducerName);
            Assert.AreEqual(123, dto.ProducerId);
            Assert.AreEqual("Invoice", dto.SuggestedBillingInstruction);
            Assert.AreEqual(100.50m, dto.SuggestedInvoiceAmount);
            Assert.AreEqual("Accepted", dto.BillingInstructionAcceptReject);
            Assert.AreEqual(1, dto.CalculatorRunId);
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
                RunId = 10,
                SearchQuery = searchQuery,
                PageNumber = 2,
                PageSize = 50,
            };

            Assert.AreEqual(10, dto.RunId);
            Assert.AreEqual(searchQuery, dto.SearchQuery);
            Assert.AreEqual(2, dto.PageNumber);
            Assert.AreEqual(50, dto.PageSize);
        }

        [TestMethod]
        public void CanSetAndGetProperties_ProducerBillingInstructionsResponseDto()
        {
            var record = new ProducerBillingInstructionsDto { ProducerId = 1, ProducerName = "A", SuggestedBillingInstruction = "Invoice", SuggestedInvoiceAmount = 1.0m, BillingInstructionAcceptReject = "Accepted", CalculatorRunId = 1 };
            var dto = new ProducerBillingInstructionsResponseDto
            {
                StatusCode = HttpStatusCode.OK,
                Records = new List<ProducerBillingInstructionsDto> { record },
                TotalRecords = 1,
                PageNumber = 1,
                PageSize = 10,
            };

            Assert.AreEqual(HttpStatusCode.OK, dto.StatusCode);
            Assert.AreEqual(1, dto.Records.Count);
            Assert.AreEqual(1, dto.TotalRecords);
            Assert.AreEqual(1, dto.PageNumber);
            Assert.AreEqual(10, dto.PageSize);
        }
    }
}