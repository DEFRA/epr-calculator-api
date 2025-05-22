using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations
{
    [ExcludeFromCodeCoverage]
    public class ProducerDesignatedRunInvoiceInstructionConfiguration : IEntityTypeConfiguration<ProducerDesignatedRunInvoiceInstruction>
    {
        public void Configure(EntityTypeBuilder<ProducerDesignatedRunInvoiceInstruction> builder)
        {
            builder.ToTable("producer_designated_run_invoice_instruction");

            builder.Property(p => p.Id)
                   .HasColumnName("id")
                   .IsRequired();

            builder.Property(p => p.ProducerId)
                  .HasColumnName("producer_id")
                  .IsRequired();

            builder.Property(p => p.CalculatorRunId)
                   .HasColumnName("calculator_run_id")
                   .IsRequired();

            builder.Property(p => p.CurrentYearInvoicedTotalAfterThisRun)
                   .HasColumnName("current_year_invoiced_total_after_this_run")
                   .HasPrecision(18, 2);

            builder.Property(p => p.InvoiceAmount)
                   .HasColumnName("invoice_amount")
                   .HasPrecision(18, 2);

            builder.Property(p => p.OutstandingBalance)
                   .HasColumnName("outstanding_balance")
                   .HasPrecision(18, 2);

            builder.Property(p => p.BillingInstructionId)
                   .HasColumnName("billing_instruction_id")
                   .HasMaxLength(4000);

            builder.Property(p => p.InstructionConfirmedDate)
                   .HasColumnName("instruction_confirmed_date");

            builder.Property(p => p.InstructionConfirmedBy)
                   .HasColumnName("instruction_confirmed_by")
                   .HasMaxLength(4000);
        }
    }
}
