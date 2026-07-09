using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Services;

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

            InvoiceDetailsService.GetCurrentYearInvoicedTotalAfterThisRun("Rejected", "CANCEL", currentYearInvoicedToDate, invoiceAmount).ShouldBe(currentYearInvoicedToDate);
            InvoiceDetailsService.GetCurrentYearInvoicedTotalAfterThisRun("Accepted", "CANCEL", currentYearInvoicedToDate, invoiceAmount).ShouldBeNull();
            InvoiceDetailsService.GetCurrentYearInvoicedTotalAfterThisRun("Rejected", "INITIAL", currentYearInvoicedToDate, invoiceAmount).ShouldBeNull();
            InvoiceDetailsService.GetCurrentYearInvoicedTotalAfterThisRun("Rejected", "REBILL", currentYearInvoicedToDate, invoiceAmount).ShouldBe(currentYearInvoicedToDate);
            InvoiceDetailsService.GetCurrentYearInvoicedTotalAfterThisRun("Rejected", "REBILL", null, invoiceAmount).ShouldBe(0m);
            InvoiceDetailsService.GetCurrentYearInvoicedTotalAfterThisRun("Accepted", "INITIAL", currentYearInvoicedToDate, invoiceAmount).ShouldBe(currentYearInvoicedToDate + invoiceAmount);
            InvoiceDetailsService.GetCurrentYearInvoicedTotalAfterThisRun("Accepted", "INITIAL", null, invoiceAmount).ShouldBe(invoiceAmount);
            InvoiceDetailsService.GetCurrentYearInvoicedTotalAfterThisRun("Accepted", "INITIAL", currentYearInvoicedToDate, null).ShouldBe(currentYearInvoicedToDate);
            InvoiceDetailsService.GetCurrentYearInvoicedTotalAfterThisRun("Accepted", "REBILL", currentYearInvoicedToDate, invoiceAmount).ShouldBe(invoiceAmount);
            InvoiceDetailsService.GetCurrentYearInvoicedTotalAfterThisRun("Accepted", "REBILL", currentYearInvoicedToDate, null).ShouldBe(0m);
            InvoiceDetailsService.GetCurrentYearInvoicedTotalAfterThisRun("Accepted", "REBILL", null, null).ShouldBe(0m);
        }

        [TestMethod]
        public void GetInvoiceAmount()
        {
            const decimal totalProducerBill = 1000m;
            const decimal liabilityDifference = 500m;

            InvoiceDetailsService.GetInvoiceAmount("Rejected", "INITIAL", totalProducerBill, null).ShouldBeNull();
            InvoiceDetailsService.GetInvoiceAmount("Accepted", "INITIAL", totalProducerBill, null).ShouldBe(totalProducerBill);
            InvoiceDetailsService.GetInvoiceAmount("Accepted", "REBILL", totalProducerBill, null).ShouldBe(totalProducerBill);
            InvoiceDetailsService.GetInvoiceAmount("Accepted", "DELTA", totalProducerBill, liabilityDifference).ShouldBe(liabilityDifference);
            InvoiceDetailsService.GetInvoiceAmount("Accepted", "UNKNOWN", totalProducerBill, null).ShouldBeNull();
            InvoiceDetailsService.GetInvoiceAmount(null, "UNKNOWN", totalProducerBill, null).ShouldBeNull();
        }

        [TestMethod]
        public void GetOutstandingBalance()
        {
            const decimal totalProducerBill = 1000m;
            const decimal liabilityDifference = 500m;

            InvoiceDetailsService.GetOutstandingBalance("Rejected", "INITIAL", totalProducerBill, liabilityDifference).ShouldBe(totalProducerBill);
            InvoiceDetailsService.GetOutstandingBalance("Rejected", "REBILL" , totalProducerBill, liabilityDifference).ShouldBe(liabilityDifference);
            InvoiceDetailsService.GetOutstandingBalance("Accepted", "INITIAL", totalProducerBill, liabilityDifference).ShouldBeNull();
            InvoiceDetailsService.GetOutstandingBalance("Accepted", "REBILL" , totalProducerBill, liabilityDifference).ShouldBeNull();
            InvoiceDetailsService.GetOutstandingBalance(null      , "INITIAL", totalProducerBill, liabilityDifference).ShouldBeNull();
            InvoiceDetailsService.GetOutstandingBalance(null      , "REBILL" , totalProducerBill, liabilityDifference).ShouldBeNull();
            InvoiceDetailsService.GetOutstandingBalance("Rejected", "INITIAL", null             , liabilityDifference).ShouldBeNull();
            InvoiceDetailsService.GetOutstandingBalance("Rejected", "REBILL" , totalProducerBill, null               ).ShouldBeNull();
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

            result.ShouldBe(2);
            this.DbContext.ProducerDesignatedRunInvoiceInstruction.Count().ShouldBe(2);

            var insertedInstruction1 = this.DbContext.ProducerDesignatedRunInvoiceInstruction.Where(x => x.ProducerId == instruction1).FirstOrDefault();
            insertedInstruction1.ShouldNotBeNull();
            insertedInstruction1.CalculatorRunId.ShouldBe(runId);
            insertedInstruction1.InvoiceAmount.ShouldBe(1000m);
            insertedInstruction1.CurrentYearInvoicedTotalAfterThisRun.ShouldBe(1500m);
            insertedInstruction1.OutstandingBalance.ShouldBeNull();
            insertedInstruction1.BillingInstructionId.ShouldBe($"{runId}_{instruction1}");

            var insertedInstruction2 = this.DbContext.ProducerDesignatedRunInvoiceInstruction.Where(x => x.ProducerId == instruction2).FirstOrDefault();
            insertedInstruction2.ShouldNotBeNull();
            insertedInstruction2.CalculatorRunId.ShouldBe(runId);
            insertedInstruction2.InvoiceAmount.ShouldBe(2000m);
            insertedInstruction2.CurrentYearInvoicedTotalAfterThisRun.ShouldBe(2000m);
            insertedInstruction2.OutstandingBalance.ShouldBeNull();
            insertedInstruction2.BillingInstructionId.ShouldBe($"{runId}_{instruction2}");
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

            result.ShouldBe(0);
            this.DbContext.ProducerDesignatedRunInvoiceInstruction.Count().ShouldBe(0);
        }
    }
}
