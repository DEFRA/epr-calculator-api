using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.Configurations;

public class PDRunInvoiceInstructionConfiguration : IEntityTypeConfiguration<ProducerDesignatedRunInvoiceInstruction>
{
    public void Configure(EntityTypeBuilder<ProducerDesignatedRunInvoiceInstruction> builder)
    {
        builder.HasIndex(e => new { e.CalculatorRunId, e.ProducerId, e.Id })
                    .HasDatabaseName("IX_index_producer_designated_run_invoice")
                    .IncludeProperties(e => new
                    {
                        e.CurrentYearInvoicedTotalAfterThisRun,
                        e.InvoiceAmount,
                        e.OutstandingBalance,
                        e.BillingInstructionId,
                        e.InstructionConfirmedDate,
                        e.InstructionConfirmedBy
                    })
                    .IsClustered(false);
    }
}