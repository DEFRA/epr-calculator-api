using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EPR.Calculator.API.Data.Configurations;

public class CalculatorRunConfiguration : IEntityTypeConfiguration<CalculatorRun>
{
    public void Configure(EntityTypeBuilder<CalculatorRun> builder)
    {
        builder.HasIndex(e => new { e.CalculatorRunClassificationId, e.FinancialYearId, e.IsBillingFileGenerating, e.Id })
                    .HasDatabaseName("IX_index_calculator_run")
                    .IncludeProperties(e => new
                    {
                        e.Name,
                        e.CreatedBy,
                        e.CreatedAt,
                        e.UpdatedBy,
                        e.UpdatedAt,
                        e.CalculatorRunOrganisationDataMasterId,
                        e.CalculatorRunPomDataMasterId,
                        e.DefaultParameterSettingMasterId,
                        e.LapcapDataMasterId
                    })
                    .IsClustered(false);
    }
}