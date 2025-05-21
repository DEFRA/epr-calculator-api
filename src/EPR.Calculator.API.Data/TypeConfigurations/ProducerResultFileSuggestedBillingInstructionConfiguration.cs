using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations
{
    [ExcludeFromCodeCoverage]
    public class ProducerResultFileSuggestedBillingInstructionConfiguration : IEntityTypeConfiguration<ProducerResultFileSuggestedBillingInstruction>
    {
        public void Configure(EntityTypeBuilder<ProducerResultFileSuggestedBillingInstruction> builder)
        {
            builder.ToTable("producer_resultfile_suggested_billing_instruction");

            builder.Property(p => p.Id)
                   .HasColumnName("id")
                   .IsRequired();

            builder.Property(p => p.CalculatorRunId)
                   .HasColumnName("calculator_run_id")
                   .IsRequired();

            builder.Property(p => p.ProducerId)
                  .HasColumnName("producer_id")
                  .IsRequired();

            builder.Property(p => p.TotalProducerBillWithBadDebt)
                   .HasColumnName("total_producer_bill_with_bad_debt")
                   .HasPrecision(18, 2)
                   .IsRequired();

            builder.Property(p => p.CurrentYearInvoiceTotalToDate)
                   .HasColumnName("current_year_invoice_total_to_date")
                   .HasPrecision(18, 2);

            builder.Property(p => p.TonnageChangeSinceLastInvoice)
                   .HasColumnName("tonnage_change_since_last_invoice")
                   .HasMaxLength(4000);

            builder.Property(p => p.AmountLiabilityDifferenceCalcVsPrev)
                   .HasColumnName("amount_liability_difference_calc_vs_prev")
                   .HasPrecision(18, 2);

            builder.Property(p => p.MaterialPoundThresholdBreached)
                   .HasColumnName("material_pound_threshold_breached")
                   .HasMaxLength(4000);

            builder.Property(p => p.TonnagePoundThresholdBreached)
                   .HasColumnName("tonnage_pound_threshold_breached")
                   .HasMaxLength(4000);

            builder.Property(p => p.PercentageLiabilityDifferenceCalcVsPrev)
                   .HasColumnName("percentage_liability_difference_calc_vs_prev")
                   .HasPrecision(18, 2);

            builder.Property(p => p.MaterialPercentageThresholdBreached)
                   .HasColumnName("material_percentage_threshold_breached")
                   .HasMaxLength(4000);

            builder.Property(p => p.TonnagePercentageThresholdBreached)
                   .HasColumnName("tonnage_percentage_threshold_breached")
                   .HasMaxLength(4000);

            builder.Property(p => p.SuggestedBillingInstruction)
                   .HasColumnName("suggested_billing_instruction")
                   .HasMaxLength(4000)
                   .IsRequired();

            builder.Property(p => p.SuggestedInvoiceAmount)
                   .HasColumnName("suggested_invoice_amount")
                   .HasPrecision(18, 2)
                   .IsRequired();

            builder.Property(p => p.BillingInstructionAcceptReject)
                   .HasColumnName("billing_instruction_accept_reject")
                   .HasMaxLength(4000);

            builder.Property(p => p.ReasonForRejection)
                   .HasColumnName("reason_for_rejection")
                   .HasMaxLength(4000);

            builder.Property(p => p.LastModifiedAcceptRejectBy)
                   .HasColumnName("last_modified_accept_reject_by")
                   .HasMaxLength(500);

            builder.Property(p => p.LastModifiedAcceptReject)
                   .HasColumnName("last_modified_accept_reject");
        }
    }
}
