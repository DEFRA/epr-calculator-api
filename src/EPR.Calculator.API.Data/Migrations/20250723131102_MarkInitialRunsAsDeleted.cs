using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class MarkInitialRunsAsDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Wrapped in EXEC() so that SQL Server resolves the column name when it runs, rather than when the batch
            // containing it is compiled: calculator_run_classification_id is renamed by a later migration
            // (SplitRunClassification), and the IF NOT EXISTS guards in idempotent scripts don't prevent the resulting
            // Msg 207 (invalid column name), as they're only evaluated at run time.
            migrationBuilder.Sql("EXEC(N'update dbo.calculator_run SET calculator_run_classification_id  = 6 where calculator_run_classification_id in (7,8)')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No need to revert this change as it is a one-way migration.
        }
    }
}
