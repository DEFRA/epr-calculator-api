using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Services;
using FluentAssertions;

namespace EPR.Calculator.API.UnitTests.Services
{
    [TestClass]
    public class InvoiceDetailsServiceTests : InMemoryApplicationDbContext
    {
        private InvoiceDetailsService invoiceDetailsService = null!;

        public TestContext TestContext { get; set; } = null!;

        [TestInitialize]
        public void Setup()
        {
            this.invoiceDetailsService = new InvoiceDetailsService(this.DbContext);
            this.DbContext.ProducerResultFileSuggestedBillingInstruction.RemoveRange(this.DbContext.ProducerResultFileSuggestedBillingInstruction);
            this.DbContext.ProducerDesignatedRunInvoiceInstruction.RemoveRange(this.DbContext.ProducerDesignatedRunInvoiceInstruction);
            this.DbContext.SaveChanges();
        }

        [TestMethod]
        public void GetCurrentYearInvoicedTotalAfterThisRun()
        {
            const decimal currentYearInvoicedToDate = 100m;
            const decimal invoiceAmount = 50m;

            InvoiceDetailsService.GetCurrentYearInvoicedTotalAfterThisRun("Rejected", "CANCEL", currentYearInvoicedToDate, invoiceAmount).Should().Be(currentYearInvoicedToDate);
            InvoiceDetailsService.GetCurrentYearInvoicedTotalAfterThisRun("Accepted", "CANCEL", currentYearInvoicedToDate, invoiceAmount).Should().BeNull();
            InvoiceDetailsService.GetCurrentYearInvoicedTotalAfterThisRun("Rejected", "INITIAL", currentYearInvoicedToDate, invoiceAmount).Should().BeNull();
            InvoiceDetailsService.GetCurrentYearInvoicedTotalAfterThisRun("Rejected", "REBILL", currentYearInvoicedToDate, invoiceAmount).Should().Be(currentYearInvoicedToDate);
            InvoiceDetailsService.GetCurrentYearInvoicedTotalAfterThisRun("Rejected", "REBILL", null, invoiceAmount).Should().Be(0m);
            InvoiceDetailsService.GetCurrentYearInvoicedTotalAfterThisRun("Accepted", "INITIAL", currentYearInvoicedToDate, invoiceAmount).Should().Be(currentYearInvoicedToDate + invoiceAmount);
            InvoiceDetailsService.GetCurrentYearInvoicedTotalAfterThisRun("Accepted", "INITIAL", null, invoiceAmount).Should().Be(invoiceAmount);
            InvoiceDetailsService.GetCurrentYearInvoicedTotalAfterThisRun("Accepted", "INITIAL", currentYearInvoicedToDate, null).Should().Be(currentYearInvoicedToDate);
            InvoiceDetailsService.GetCurrentYearInvoicedTotalAfterThisRun("Accepted", "REBILL", currentYearInvoicedToDate, invoiceAmount).Should().Be(invoiceAmount);
            InvoiceDetailsService.GetCurrentYearInvoicedTotalAfterThisRun("Accepted", "REBILL", currentYearInvoicedToDate, null).Should().Be(0m);
            InvoiceDetailsService.GetCurrentYearInvoicedTotalAfterThisRun("Accepted", "REBILL", null, null).Should().Be(0m);
        }

        [TestMethod]
        public void GetInvoiceAmount()
        {
            const decimal totalProducerBill = 1000m;
            const decimal liabilityDifference = 500m;

            InvoiceDetailsService.GetInvoiceAmount("Rejected", "INITIAL", totalProducerBill, null).Should().BeNull();
            InvoiceDetailsService.GetInvoiceAmount("Accepted", "INITIAL", totalProducerBill, null).Should().Be(totalProducerBill);
            InvoiceDetailsService.GetInvoiceAmount("Accepted", "REBILL", totalProducerBill, null).Should().Be(totalProducerBill);
            InvoiceDetailsService.GetInvoiceAmount("Accepted", "DELTA", totalProducerBill, liabilityDifference).Should().Be(liabilityDifference);
            InvoiceDetailsService.GetInvoiceAmount("Accepted", "UNKNOWN", totalProducerBill, null).Should().BeNull();
            InvoiceDetailsService.GetInvoiceAmount(null, "UNKNOWN", totalProducerBill, null).Should().BeNull();
        }

        [TestMethod]
        public void GetOutstandingBalance()
        {
            const decimal totalProducerBill = 1000m;
            const decimal liabilityDifference = 500m;

            InvoiceDetailsService.GetOutstandingBalance("Rejected", "INITIAL", totalProducerBill, null).Should().Be(totalProducerBill);
            InvoiceDetailsService.GetOutstandingBalance("Rejected", "REBILL", totalProducerBill, liabilityDifference).Should().Be(liabilityDifference);
            InvoiceDetailsService.GetOutstandingBalance("Accepted", "INITIAL", totalProducerBill, liabilityDifference).Should().BeNull();
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

            await this.DbContext.ProducerResultFileSuggestedBillingInstruction.AddRangeAsync(sourceRows, TestContext.CancellationTokenSource.Token);
            await this.DbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

            var result = await this.invoiceDetailsService.InsertInvoiceDetailsAtProducerLevel(
                runId,
                instructionConfirmedDate,
                instructionConfirmedBy,
                TestContext.CancellationTokenSource.Token);

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
            insertedInstruction2.CurrentYearInvoicedTotalAfterThisRun.Should().Be(2000m);
            insertedInstruction2.OutstandingBalance.Should().BeNull();
            insertedInstruction2.BillingInstructionId.Should().Be($"{runId}_{instruction2}");
        }

        [TestMethod]
        public async Task InsertInvoiceDetailsAtProducerLevel_WhenNoSourceRows()
        {
            const int runId = 1;
            var instructionConfirmedDate = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc);
            const string instructionConfirmedBy = "TestUser";

            var result = await this.invoiceDetailsService.InsertInvoiceDetailsAtProducerLevel(
                runId,
                instructionConfirmedDate,
                instructionConfirmedBy,
                TestContext.CancellationTokenSource.Token);

            result.Should().Be(0);
            this.DbContext.ProducerDesignatedRunInvoiceInstruction.Should().HaveCount(0);
        }
    }
}
