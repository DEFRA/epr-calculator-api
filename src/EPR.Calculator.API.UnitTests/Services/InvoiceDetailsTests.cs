using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Services;
using FluentAssertions;

namespace EPR.Calculator.API.UnitTests.Services
{
    [TestClass]
    public class InvoiceDetailsTests : InMemoryApplicationDbContext
    {
        private InvoiceDetails invoiceDetails = null!;

        public TestContext TestContext { get; set; } = null!;

        [TestInitialize]
        public void Setup()
        {
            this.invoiceDetails = new InvoiceDetails(this.DbContext);
            this.DbContext.ProducerResultFileSuggestedBillingInstruction.RemoveRange(this.DbContext.ProducerResultFileSuggestedBillingInstruction);
            this.DbContext.ProducerDesignatedRunInvoiceInstruction.RemoveRange(this.DbContext.ProducerDesignatedRunInvoiceInstruction);
            this.DbContext.SaveChanges();
        }

        [TestMethod]
        public void GetCurrentYearInvoicedTotalAfterThisRun()
        {
            const decimal currentYearInvoicedToDate = 100m;
            const decimal invoiceAmount = 50m;

            InvoiceDetails.GetCurrentYearInvoicedTotalAfterThisRun("Rejected", "CANCEL", currentYearInvoicedToDate, invoiceAmount).Should().Be(currentYearInvoicedToDate);
            InvoiceDetails.GetCurrentYearInvoicedTotalAfterThisRun("Accepted", "CANCEL", currentYearInvoicedToDate, invoiceAmount).Should().BeNull();
            InvoiceDetails.GetCurrentYearInvoicedTotalAfterThisRun("Rejected", "INITIAL", currentYearInvoicedToDate, invoiceAmount).Should().BeNull();
            InvoiceDetails.GetCurrentYearInvoicedTotalAfterThisRun("Rejected", "REBILL", currentYearInvoicedToDate, invoiceAmount).Should().Be(currentYearInvoicedToDate);
            InvoiceDetails.GetCurrentYearInvoicedTotalAfterThisRun("Rejected", "DEBILL", currentYearInvoicedToDate, invoiceAmount).Should().Be(currentYearInvoicedToDate);
            InvoiceDetails.GetCurrentYearInvoicedTotalAfterThisRun("Accepted", "INITIAL", currentYearInvoicedToDate, invoiceAmount).Should().Be(currentYearInvoicedToDate + invoiceAmount);
            InvoiceDetails.GetCurrentYearInvoicedTotalAfterThisRun("Accepted", "REBILL", null, invoiceAmount).Should().Be(invoiceAmount);
            InvoiceDetails.GetCurrentYearInvoicedTotalAfterThisRun("Accepted", "REBILL", currentYearInvoicedToDate, null).Should().Be(currentYearInvoicedToDate);
            InvoiceDetails.GetCurrentYearInvoicedTotalAfterThisRun("Accepted", "REBILL", null, null).Should().Be(0m);
        }

        [TestMethod]
        public void GetInvoiceAmount()
        {
            const decimal totalProducerBill = 1000m;
            const decimal liabilityDifference = 500m;

            InvoiceDetails.GetInvoiceAmount("Rejected", "INITIAL", totalProducerBill, null).Should().BeNull();
            InvoiceDetails.GetInvoiceAmount("Accepted", "INITIAL", totalProducerBill, null).Should().Be(totalProducerBill);
            InvoiceDetails.GetInvoiceAmount("Accepted", "REBILL", totalProducerBill, null).Should().Be(totalProducerBill);
            InvoiceDetails.GetInvoiceAmount("Accepted", "DELTA", totalProducerBill, liabilityDifference).Should().Be(liabilityDifference);
            InvoiceDetails.GetInvoiceAmount("Accepted", "UNKNOWN", totalProducerBill, null).Should().BeNull();
            InvoiceDetails.GetInvoiceAmount(null, "UNKNOWN", totalProducerBill, null).Should().BeNull();
        }

        [TestMethod]
        public void GetOutstandingBalance()
        {
            const decimal totalProducerBill = 1000m;
            const decimal liabilityDifference = 500m;

            InvoiceDetails.GetOutstandingBalance("Rejected", "INITIAL", totalProducerBill, null).Should().Be(totalProducerBill);
            InvoiceDetails.GetOutstandingBalance("Rejected", "REBILL", totalProducerBill, liabilityDifference).Should().Be(liabilityDifference);
            InvoiceDetails.GetOutstandingBalance("Accepted", "INITIAL", totalProducerBill, liabilityDifference).Should().BeNull();
        }

        [TestMethod]
        public async Task InsertInvoiceDetailsAtProducerLevel()
        {
            const int runId = 1;
            const int otherRunId = 2;
            var instructionConfirmedDate = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc);
            const string instructionConfirmedBy = "TestUser";
            var instruction1 = 100;
            var instruction2 = 101;
            var sourceRows = new List<ProducerResultFileSuggestedBillingInstruction>
            {
                new()
                {
                    CalculatorRunId = runId,
                    ProducerId = instruction1,
                    SuggestedBillingInstruction = "INITIAL",
                    BillingInstructionAcceptReject = "Accepted",
                    TotalProducerBillWithBadDebt = 1000m,
                    AmountLiabilityDifferenceCalcVsPrev = null,
                    CurrentYearInvoiceTotalToDate = 500m
                },
                new()
                {
                    CalculatorRunId = runId,
                    ProducerId = instruction2,
                    SuggestedBillingInstruction = "REBILL",
                    BillingInstructionAcceptReject = "Accepted",
                    TotalProducerBillWithBadDebt = 2000m,
                    AmountLiabilityDifferenceCalcVsPrev = 500m,
                    CurrentYearInvoiceTotalToDate = 800m
                },
                new()
                {
                    CalculatorRunId = otherRunId,
                    ProducerId = 101,
                    SuggestedBillingInstruction = "DELTA",
                    BillingInstructionAcceptReject = "Rejected",
                    TotalProducerBillWithBadDebt = 2000m,
                    AmountLiabilityDifferenceCalcVsPrev = null,
                    CurrentYearInvoiceTotalToDate = 800m
                }
            };

            await this.DbContext.ProducerResultFileSuggestedBillingInstruction.AddRangeAsync(sourceRows);
            await this.DbContext.SaveChangesAsync();

            var result = await this.invoiceDetails.InsertInvoiceDetailsAtProducerLevel(
                runId,
                instructionConfirmedDate,
                instructionConfirmedBy,
                default);

            result.Should().Be(2);
            this.DbContext.ProducerDesignatedRunInvoiceInstruction.Should().HaveCount(2);

            var insertedInstruction1 = this.DbContext.ProducerDesignatedRunInvoiceInstruction.Where(x => x.ProducerId == instruction1).FirstOrDefault();
            insertedInstruction1.Should().NotBeNull();
            insertedInstruction1.CalculatorRunId.Should().Be(runId);
            insertedInstruction1.InvoiceAmount.Should().Be(1000m);
            insertedInstruction1.CurrentYearInvoicedTotalAfterThisRun.Should().Be(1500m);
            insertedInstruction1.OutstandingBalance.Should().BeNull();
            insertedInstruction1.BillingInstructionId.Should().Be($"{runId}_{instruction1}");

            var insertedInstruction2 = this.DbContext.ProducerDesignatedRunInvoiceInstruction.Where(x => x.ProducerId == instruction2).FirstOrDefault();
            insertedInstruction2.Should().NotBeNull();
            insertedInstruction2.CalculatorRunId.Should().Be(runId);
            insertedInstruction2.InvoiceAmount.Should().Be(2000m);
            insertedInstruction2.CurrentYearInvoicedTotalAfterThisRun.Should().Be(2800m);
            insertedInstruction2.OutstandingBalance.Should().BeNull();
            insertedInstruction2.BillingInstructionId.Should().Be($"{runId}_{instruction2}");
        }

        [TestMethod]
        public async Task InsertInvoiceDetailsAtProducerLevel_WhenNoSourceRows()
        {
            const int runId = 1;
            var instructionConfirmedDate = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc);
            const string instructionConfirmedBy = "TestUser";

            var result = await this.invoiceDetails.InsertInvoiceDetailsAtProducerLevel(
                runId,
                instructionConfirmedDate,
                instructionConfirmedBy,
                default);

            result.Should().Be(0);
            this.DbContext.ProducerDesignatedRunInvoiceInstruction.Should().HaveCount(0);
        }
    }
}
