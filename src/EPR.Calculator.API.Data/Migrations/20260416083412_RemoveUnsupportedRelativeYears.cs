using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnsupportedRelativeYears : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                delete from calculator_run_billing_file_metadata where calculator_run_id in (select id from calculator_run where relative_year in (2023, 2024));
            ");

            migrationBuilder.Sql(@"
                delete from calculator_run_csvfile_metadata where calculator_run_id in (select id from calculator_run where relative_year in (2023, 2024));
            ");

            migrationBuilder.Sql(@"
                delete from country_apportionment where calculator_run_id in (select id from calculator_run where relative_year in (2023, 2024));
            ");

            migrationBuilder.Sql(@"
                delete from error_report where calculator_run_id in (select id from calculator_run where relative_year in (2023, 2024));
            ");

            migrationBuilder.Sql(@"
                delete from producer_designated_run_invoice_instruction where calculator_run_id in (select id from calculator_run where relative_year in (2023, 2024));
            ");

            migrationBuilder.Sql(@"
                delete from producer_detail where calculator_run_id in (select id from calculator_run where relative_year in (2023, 2024));
            ");

            migrationBuilder.Sql(@"
                delete from producer_invoiced_material_net_tonnage where calculator_run_id in (select id from calculator_run where relative_year in (2023, 2024));
            ");

            migrationBuilder.Sql(@"
                delete from producer_resultfile_suggested_billing_instruction where calculator_run_id in (select id from calculator_run where relative_year in (2023, 2024));
            ");

            migrationBuilder.Sql(@"
                DELETE FROM calculator_run WHERE relative_year IN (2023, 2024);
            ");
            migrationBuilder.Sql(@"
                delete from calculator_run_organization_data_master where relative_year in (2023, 2024);
            ");

            migrationBuilder.Sql(@"
                delete from calculator_run_pom_data_master where relative_year in (2023, 2024);
            ");

            migrationBuilder.Sql(@"
                delete from lapcap_data_detail where lapcap_data_master_id in (select id from lapcap_data_master where relative_year in (2023, 2024));
            ");

            migrationBuilder.Sql(@"
                delete from lapcap_data_master where relative_year in (2023, 2024);
            ");

            migrationBuilder.Sql(@"
                delete from default_parameter_setting_detail where default_parameter_setting_master_id in (select id from default_parameter_setting_master where relative_year in (2023, 2024));
            ");

            migrationBuilder.Sql(@"
                delete from default_parameter_setting_master where relative_year in (2023, 2024);
            ");

            migrationBuilder.Sql(@"
                delete from calculator_run_relative_years where relative_year in (2023, 2024);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "calculator_run_relative_years",
                columns: new[] { "relative_year", "description" },
                values: new object[] { "2023", null }
            );

            migrationBuilder.InsertData(
                table: "calculator_run_relative_years",
                columns: new[] { "relative_year", "description" },
                values: new object[] { "2024", null }
            );
        }
    }
}
