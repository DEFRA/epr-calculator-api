using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UseRelativeYear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop old foreign keys and indices
            migrationBuilder.DropForeignKey(
                name: "FK_calculator_run_calculator_run_financial_years_financial_year",
                table: "calculator_run");

            migrationBuilder.DropForeignKey(
                name: "FK_default_parameter_setting_master_calculator_run_financial_years_parameter_year",
                table: "default_parameter_setting_master");

            migrationBuilder.DropForeignKey(
                name: "FK_lapcap_data_master_calculator_run_financial_years_projection_year",
                table: "lapcap_data_master");

            migrationBuilder.DropPrimaryKey(
                name: "PK_calculator_run_financial_years",
                table: "calculator_run_financial_years");

            migrationBuilder.DropIndex(
                name: "IX_lapcap_data_master_projection_year",
                table: "lapcap_data_master");

            migrationBuilder.DropIndex(
                name: "IX_default_parameter_setting_master_parameter_year",
                table: "default_parameter_setting_master");

            migrationBuilder.DropIndex(
                name: "IX_calculator_run_financial_year",
                table: "calculator_run");

            migrationBuilder.DropIndex(
                name: "IX_index_calculator_run",
                table: "calculator_run");

            // Rename table
            migrationBuilder.RenameTable(
                name: "calculator_run_financial_years",
                newName: "calculator_run_relative_years");

            // Add relative_year columns
            migrationBuilder.AddColumn<int>(
                name: "relative_year",
                table: "lapcap_data_master",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "relative_year",
                table: "default_parameter_setting_master",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "relative_year",
                table: "calculator_run_pom_data_master",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "relative_year",
                table: "calculator_run_organization_data_master",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "relative_year",
                table: "calculator_run_relative_years",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "relative_year",
                table: "calculator_run",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Update relative_year columns
            migrationBuilder.Sql(@"UPDATE lapcap_data_master                      SET relative_year = CAST(LEFT(projection_year, 4) AS INT)");
            migrationBuilder.Sql(@"UPDATE default_parameter_setting_master        SET relative_year = CAST(LEFT(parameter_year, 4) AS INT)");
            migrationBuilder.Sql(@"UPDATE calculator_run_pom_data_master          SET relative_year = calendar_year + 1");
            migrationBuilder.Sql(@"UPDATE calculator_run_organization_data_master SET relative_year = calendar_year + 1");
            migrationBuilder.Sql(@"UPDATE calculator_run_relative_years           SET relative_year = CAST(LEFT(financial_Year, 4) AS INT)");
            migrationBuilder.Sql(@"UPDATE calculator_run                          SET relative_year = CAST(LEFT(financial_Year, 4) AS INT)");

            // Drop old year columns
            migrationBuilder.DropColumn(
                name: "projection_year",
                table: "lapcap_data_master");

            migrationBuilder.DropColumn(
                name: "parameter_year",
                table: "default_parameter_setting_master");

            migrationBuilder.DropColumn(
                name: "calendar_year",
                table: "calculator_run_pom_data_master");

            migrationBuilder.DropColumn(
                name: "calendar_year",
                table: "calculator_run_organization_data_master");

            migrationBuilder.DropColumn(
                name: "financial_Year",
                table: "calculator_run_relative_years");

            migrationBuilder.DropColumn(
                name: "financial_year",
                table: "calculator_run");

            // Add relative_year primary keys and indicies
            migrationBuilder.AddPrimaryKey(
                name: "PK_calculator_run_relative_years",
                table: "calculator_run_relative_years",
                column: "relative_year");

            migrationBuilder.CreateIndex(
                name: "IX_lapcap_data_master_relative_year",
                table: "lapcap_data_master",
                column: "relative_year");

            migrationBuilder.CreateIndex(
                name: "IX_default_parameter_setting_master_relative_year",
                table: "default_parameter_setting_master",
                column: "relative_year");

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_pom_data_master_relative_year",
                table: "calculator_run_pom_data_master",
                column: "relative_year");

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_organization_data_master_relative_year",
                table: "calculator_run_organization_data_master",
                column: "relative_year");

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_relative_year",
                table: "calculator_run",
                column: "relative_year");

            migrationBuilder.CreateIndex(
                name: "IX_index_calculator_run",
                table: "calculator_run",
                columns: new[] { "calculator_run_classification_id", "relative_year", "is_billing_file_generating", "id" })
                .Annotation("SqlServer:Clustered", false)
                .Annotation("SqlServer:Include", new[] { "name", "created_by", "created_at", "updated_by", "updated_at", "calculator_run_organization_data_master_id", "calculator_run_pom_data_master_id", "default_parameter_setting_master_id", "lapcap_data_master_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_calculator_run_calculator_run_relative_years_relative_year",
                table: "calculator_run",
                column: "relative_year",
                principalTable: "calculator_run_relative_years",
                principalColumn: "relative_year",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_calculator_run_organization_data_master_calculator_run_relative_years_relative_year",
                table: "calculator_run_organization_data_master",
                column: "relative_year",
                principalTable: "calculator_run_relative_years",
                principalColumn: "relative_year",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_calculator_run_pom_data_master_calculator_run_relative_years_relative_year",
                table: "calculator_run_pom_data_master",
                column: "relative_year",
                principalTable: "calculator_run_relative_years",
                principalColumn: "relative_year",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_default_parameter_setting_master_calculator_run_relative_years_relative_year",
                table: "default_parameter_setting_master",
                column: "relative_year",
                principalTable: "calculator_run_relative_years",
                principalColumn: "relative_year",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_lapcap_data_master_calculator_run_relative_years_relative_year",
                table: "lapcap_data_master",
                column: "relative_year",
                principalTable: "calculator_run_relative_years",
                principalColumn: "relative_year",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop relative_year foreign keys and indices
            migrationBuilder.DropForeignKey(
                name: "FK_calculator_run_calculator_run_relative_years_relative_year",
                table: "calculator_run");

            migrationBuilder.DropForeignKey(
                name: "FK_calculator_run_organization_data_master_calculator_run_relative_years_relative_year",
                table: "calculator_run_organization_data_master");

            migrationBuilder.DropForeignKey(
                name: "FK_calculator_run_pom_data_master_calculator_run_relative_years_relative_year",
                table: "calculator_run_pom_data_master");

            migrationBuilder.DropForeignKey(
                name: "FK_default_parameter_setting_master_calculator_run_relative_years_relative_year",
                table: "default_parameter_setting_master");

            migrationBuilder.DropForeignKey(
                name: "FK_lapcap_data_master_calculator_run_relative_years_relative_year",
                table: "lapcap_data_master");

            migrationBuilder.DropPrimaryKey(
                name: "PK_calculator_run_relative_years",
                table: "calculator_run_relative_years");

            migrationBuilder.DropIndex(
                name: "IX_lapcap_data_master_relative_year",
                table: "lapcap_data_master");

            migrationBuilder.DropIndex(
                name: "IX_default_parameter_setting_master_relative_year",
                table: "default_parameter_setting_master");

            migrationBuilder.DropIndex(
                name: "IX_calculator_run_pom_data_master_relative_year",
                table: "calculator_run_pom_data_master");

            migrationBuilder.DropIndex(
                name: "IX_calculator_run_organization_data_master_relative_year",
                table: "calculator_run_organization_data_master");

            migrationBuilder.DropIndex(
                name: "IX_calculator_run_relative_year",
                table: "calculator_run");

            migrationBuilder.DropIndex(
                name: "IX_index_calculator_run",
                table: "calculator_run");

            // Rename table
            migrationBuilder.RenameTable(
                name: "calculator_run_relative_years",
                newName: "calculator_run_financial_years");

            // Add old year columns back
            migrationBuilder.AddColumn<string>(
                name: "projection_year",
                table: "lapcap_data_master",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "parameter_year",
                table: "default_parameter_setting_master",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "calendar_year",
                table: "calculator_run_pom_data_master",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "calendar_year",
                table: "calculator_run_organization_data_master",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "financial_Year",
                table: "calculator_run_financial_years",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "financial_year",
                table: "calculator_run",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            // Update old columns
            migrationBuilder.Sql(@"UPDATE lapcap_data_master                        SET projection_year = CAST(relative_year AS VARCHAR(4)) + '-' + RIGHT('0' + CAST((relative_year + 1) % 100 AS VARCHAR(2)), 2)");
            migrationBuilder.Sql(@"UPDATE default_parameter_setting_master          SET parameter_year  = CAST(relative_year AS VARCHAR(4)) + '-' + RIGHT('0' + CAST((relative_year + 1) % 100 AS VARCHAR(2)), 2)");
            migrationBuilder.Sql(@"UPDATE calculator_run_pom_data_master            SET calendar_year   = relative_year - 1");
            migrationBuilder.Sql(@"UPDATE calculator_run_organization_data_master   SET calendar_year   = relative_year - 1");
            migrationBuilder.Sql(@"UPDATE calculator_run_financial_years            SET financial_Year  = CAST(relative_year AS VARCHAR(4)) + '-' + RIGHT('0' + CAST((relative_year + 1) % 100 AS VARCHAR(2)), 2)");
            migrationBuilder.Sql(@"UPDATE calculator_run                            SET financial_year  = CAST(relative_year AS VARCHAR(4)) + '-' + RIGHT('0' + CAST((relative_year + 1) % 100 AS VARCHAR(2)), 2)");

            migrationBuilder.DropColumn(
                name: "relative_year",
                table: "lapcap_data_master");

            migrationBuilder.DropColumn(
                name: "relative_year",
                table: "default_parameter_setting_master");

            migrationBuilder.DropColumn(
                name: "relative_year",
                table: "calculator_run_pom_data_master");

            migrationBuilder.DropColumn(
                name: "relative_year",
                table: "calculator_run_organization_data_master");

            migrationBuilder.DropColumn(
                name: "relative_year",
                table: "calculator_run_financial_years");

            migrationBuilder.DropColumn(
                name: "relative_year",
                table: "calculator_run");

            // Add back year primary keys and indicies
            migrationBuilder.AddPrimaryKey(
                name: "PK_calculator_run_financial_years",
                table: "calculator_run_financial_years",
                column: "financial_Year");

            migrationBuilder.CreateIndex(
                name: "IX_lapcap_data_master_projection_year",
                table: "lapcap_data_master",
                column: "projection_year");

            migrationBuilder.CreateIndex(
                name: "IX_default_parameter_setting_master_parameter_year",
                table: "default_parameter_setting_master",
                column: "parameter_year");

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_financial_year",
                table: "calculator_run",
                column: "financial_year");

            migrationBuilder.CreateIndex(
                name: "IX_index_calculator_run",
                table: "calculator_run",
                columns: new[] { "calculator_run_classification_id", "financial_year", "is_billing_file_generating", "id" })
                .Annotation("SqlServer:Clustered", false)
                .Annotation("SqlServer:Include", new[] { "name", "created_by", "created_at", "updated_by", "updated_at", "calculator_run_organization_data_master_id", "calculator_run_pom_data_master_id", "default_parameter_setting_master_id", "lapcap_data_master_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_calculator_run_calculator_run_financial_years_financial_year",
                table: "calculator_run",
                column: "financial_year",
                principalTable: "calculator_run_financial_years",
                principalColumn: "financial_Year",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_default_parameter_setting_master_calculator_run_financial_years_parameter_year",
                table: "default_parameter_setting_master",
                column: "parameter_year",
                principalTable: "calculator_run_financial_years",
                principalColumn: "financial_Year",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_lapcap_data_master_calculator_run_financial_years_projection_year",
                table: "lapcap_data_master",
                column: "projection_year",
                principalTable: "calculator_run_financial_years",
                principalColumn: "financial_Year",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
