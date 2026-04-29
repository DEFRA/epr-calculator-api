using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class RunClassificationUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_calculator_run_calculator_run_classification_calculator_run_classification_id",
                table: "calculator_run");

            migrationBuilder.DropTable(
                name: "calculator_run_classification");

            migrationBuilder.DropIndex(
                name: "IX_calculator_run_calculator_run_classification_id",
                table: "calculator_run");

            migrationBuilder.DropIndex(
                name: "IX_index_calculator_run",
                table: "calculator_run");

            migrationBuilder.AddColumn<string>(
                name: "classification",
                table: "calculator_run",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE calculator_run SET classification = 'None'                             WHERE calculator_run_classification_id = 1
                UPDATE calculator_run SET classification = 'Running'                          WHERE calculator_run_classification_id = 2
                UPDATE calculator_run SET classification = 'Unclassified'                     WHERE calculator_run_classification_id = 3
                UPDATE calculator_run SET classification = 'TestRun'                          WHERE calculator_run_classification_id = 4
                UPDATE calculator_run SET classification = 'Errored'                          WHERE calculator_run_classification_id = 5
                UPDATE calculator_run SET classification = 'Deleted'                          WHERE calculator_run_classification_id = 6
                UPDATE calculator_run SET classification = 'InitialRunCompleted'              WHERE calculator_run_classification_id = 7
                UPDATE calculator_run SET classification = 'InitialRun'                       WHERE calculator_run_classification_id = 8
                UPDATE calculator_run SET classification = 'InterimRecalculationRun'          WHERE calculator_run_classification_id = 9
                UPDATE calculator_run SET classification = 'FinalRun'                         WHERE calculator_run_classification_id = 10
                UPDATE calculator_run SET classification = 'FinalRecalculationRun'            WHERE calculator_run_classification_id = 11
                UPDATE calculator_run SET classification = 'InterimRecalculationRunCompleted' WHERE calculator_run_classification_id = 12
                UPDATE calculator_run SET classification = 'FinalRecalculationRunCompleted'   WHERE calculator_run_classification_id = 13
                UPDATE calculator_run SET classification = 'FinalRunCompleted'                WHERE calculator_run_classification_id = 14
                UPDATE calculator_run SET classification = 'Unknown'                          WHERE classification is null
                """);

            migrationBuilder.AlterColumn<string>(
                name: "classification",
                table: "calculator_run",
                maxLength: 50,
                nullable: false,
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "calculator_run_classification_id",
                table: "calculator_run");

            migrationBuilder.CreateIndex(
                name: "IX_index_calculator_run",
                table: "calculator_run",
                columns: new[] { "relative_year", "classification", "billing_run_status", "id" })
                .Annotation("SqlServer:Clustered", false)
                .Annotation("SqlServer:Include", new[] { "name", "created_by", "created_at", "updated_by", "updated_at", "calculator_run_organization_data_master_id", "calculator_run_pom_data_master_id", "default_parameter_setting_master_id", "lapcap_data_master_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_index_calculator_run",
                table: "calculator_run");

            migrationBuilder.CreateTable(
                name: "calculator_run_classification",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_by = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    status = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calculator_run_classification", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "calculator_run_classification",
                columns: new[] { "id", "created_by", "status" },
                values: new object[,]
                {
                    { 1, "System", "IN THE QUEUE" },
                    { 2, "System", "RUNNING" },
                    { 3, "System", "UNCLASSIFIED" },
                    { 4, "System", "TEST RUN" },
                    { 5, "System", "ERROR" },
                    { 6, "System", "DELETED" },
                    { 7, "System", "INITIAL RUN COMPLETED" },
                    { 8, "System", "INITIAL RUN" },
                    { 9, "System", "INTERIM RE-CALCULATION RUN" },
                    { 10, "System", "FINAL RUN" },
                    { 11, "System", "FINAL RE-CALCULATION RUN" },
                    { 12, "System", "INTERIM RE-CALCULATION RUN COMPLETED" },
                    { 13, "System", "FINAL RE-CALCULATION RUN COMPLETED" },
                    { 14, "System", "FINAL RUN COMPLETED" }
                });

            migrationBuilder.AddColumn<int>(
                name: "calculator_run_classification_id",
                table: "calculator_run",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE calculator_run SET calculator_run_classification_id = 1  WHERE classification = 'None'
                UPDATE calculator_run SET calculator_run_classification_id = 2  WHERE classification = 'Running'
                UPDATE calculator_run SET calculator_run_classification_id = 3  WHERE classification = 'Unclassified'
                UPDATE calculator_run SET calculator_run_classification_id = 4  WHERE classification = 'TestRun'
                UPDATE calculator_run SET calculator_run_classification_id = 5  WHERE classification = 'Errored'
                UPDATE calculator_run SET calculator_run_classification_id = 6  WHERE classification = 'Deleted'
                UPDATE calculator_run SET calculator_run_classification_id = 7  WHERE classification = 'InitialRunCompleted'
                UPDATE calculator_run SET calculator_run_classification_id = 8  WHERE classification = 'InitialRun'
                UPDATE calculator_run SET calculator_run_classification_id = 9  WHERE classification = 'InterimRecalculationRun'
                UPDATE calculator_run SET calculator_run_classification_id = 10 WHERE classification = 'FinalRun'
                UPDATE calculator_run SET calculator_run_classification_id = 11 WHERE classification = 'FinalRecalculationRun'
                UPDATE calculator_run SET calculator_run_classification_id = 12 WHERE classification = 'InterimRecalculationRunCompleted'
                UPDATE calculator_run SET calculator_run_classification_id = 13 WHERE classification = 'FinalRecalculationRunCompleted'
                UPDATE calculator_run SET calculator_run_classification_id = 14 WHERE classification = 'FinalRunCompleted'
                UPDATE calculator_run SET calculator_run_classification_id = 1  WHERE classification = 'Unknown'
                """);

            migrationBuilder.AlterColumn<int>(
                name: "calculator_run_classification_id",
                table: "calculator_run",
                nullable: false,
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "classification",
                table: "calculator_run");

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_calculator_run_classification_id",
                table: "calculator_run",
                column: "calculator_run_classification_id");

            migrationBuilder.CreateIndex(
                name: "IX_index_calculator_run",
                table: "calculator_run",
                columns: new[] { "relative_year", "calculator_run_classification_id", "billing_run_status", "id" })
                .Annotation("SqlServer:Clustered", false)
                .Annotation("SqlServer:Include", new[] { "name", "created_by", "created_at", "updated_by", "updated_at", "calculator_run_organization_data_master_id", "calculator_run_pom_data_master_id", "default_parameter_setting_master_id", "lapcap_data_master_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_calculator_run_calculator_run_classification_calculator_run_classification_id",
                table: "calculator_run",
                column: "calculator_run_classification_id",
                principalTable: "calculator_run_classification",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
