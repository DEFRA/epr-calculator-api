using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProjectedProducersNotShown : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The same rule as CalcResultProjectedProducers.CompleteRamProducerIds: a producer whose H1 and H2 rows have
            // no tonnage without a RAM rating isn't shown in the projected producers sections. The rows are kept in
            // _bk tables so Down can restore them.
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS #complete_ram_producers;

                SELECT calculator_run_id, producer_id
                INTO #complete_ram_producers
                FROM (
                    SELECT calculator_run_id, producer_id, household_tonnage_without_ram, public_bin_tonnage_without_ram, hdc_tonnage_without_ram FROM transform_projected_h1
                    UNION ALL
                    SELECT calculator_run_id, producer_id, household_tonnage_without_ram, public_bin_tonnage_without_ram, hdc_tonnage_without_ram FROM transform_projected_h2
                ) projected
                GROUP BY calculator_run_id, producer_id
                HAVING MAX(CASE WHEN household_tonnage_without_ram > 0 OR public_bin_tonnage_without_ram > 0 OR ISNULL(hdc_tonnage_without_ram, 0) > 0 THEN 1 ELSE 0 END) = 0;

                SELECT h1.* INTO transform_projected_h1_bk FROM transform_projected_h1 h1
                JOIN #complete_ram_producers c ON c.calculator_run_id = h1.calculator_run_id AND c.producer_id = h1.producer_id;

                SELECT h2.* INTO transform_projected_h2_bk FROM transform_projected_h2 h2
                JOIN #complete_ram_producers c ON c.calculator_run_id = h2.calculator_run_id AND c.producer_id = h2.producer_id;

                DELETE h1 FROM transform_projected_h1 h1 JOIN transform_projected_h1_bk bk ON bk.id = h1.id;
                DELETE h2 FROM transform_projected_h2 h2 JOIN transform_projected_h2_bk bk ON bk.id = h2.id;

                DROP TABLE #complete_ram_producers;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var table in new[] { "transform_projected_h1", "transform_projected_h2" })
            {
                migrationBuilder.Sql($@"
                    DECLARE @columns nvarchar(max) = (
                        SELECT STRING_AGG(QUOTENAME(name), ', ') WITHIN GROUP (ORDER BY column_id)
                        FROM sys.columns WHERE object_id = OBJECT_ID('{table}'));
                    EXEC(N'SET IDENTITY_INSERT {table} ON;
                           INSERT INTO {table} (' + @columns + N') SELECT ' + @columns + N' FROM {table}_bk;
                           SET IDENTITY_INSERT {table} OFF;
                           DROP TABLE {table}_bk;');
                ");
            }
        }
    }
}
