using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.TypeConfigurations
{
    [ExcludeFromCodeCoverage]
    public class CalculatorRunOrganisationDataMasterConfiguration : IEntityTypeConfiguration<CalculatorRunOrganisationDataMaster>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<CalculatorRunOrganisationDataMaster> builder)
        {
            builder.ToTable("calculator_run_organization_data_master");

            builder.Property(p => p.Id)
                   .HasColumnName("id");

            builder.Property(p => p.RelativeYear)
                   .HasColumnName("relative_year");

            builder.Property(p => p.EffectiveFrom)
                   .HasColumnName("effective_from");

            builder.Property(p => p.EffectiveTo)
                   .HasColumnName("effective_to");

            builder.Property(p => p.CreatedBy)
                   .HasColumnName("created_by");

            builder.Property(p => p.CreatedAt)
                   .HasColumnName("created_at");

            builder.HasMany(e => e.Details)
                   .WithOne(e => e.CalculatorRunOrganisationDataMaster)
                   .HasForeignKey(e => e.CalculatorRunOrganisationDataMasterId)
                   .IsRequired(true);

            builder.HasMany(e => e.RunDetails)
                   .WithOne(e => e.CalculatorRunOrganisationDataMaster)
                   .HasForeignKey(e => e.CalculatorRunOrganisationDataMasterId);
        }
    }
}
