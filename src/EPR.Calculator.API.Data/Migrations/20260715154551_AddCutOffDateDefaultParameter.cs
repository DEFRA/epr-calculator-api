using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCutOffDateDefaultParameter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "parameter_value",
                table: "default_parameter_setting_detail",
                type: "nvarchar(max)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.InsertData(
                table: "default_parameter_template_master",
                columns: new[] { "parameter_unique_ref", "parameter_category", "parameter_type", "valid_Range_from", "valid_Range_to" },
                values: new object[] { "COFF-DT", "Optional Date", "Cut-off date", 0m, 0m });

            migrationBuilder.Sql(@"
                INSERT INTO default_parameter_setting_detail
                (
                    default_parameter_setting_master_id,
                    parameter_unique_ref,
                    parameter_value
                )
                SELECT
                    dpsm.Id,
                    'COFF-DT',
                    'NA'
                FROM default_parameter_setting_master dpsm
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM default_parameter_setting_detail dpsd
                    WHERE dpsd.default_parameter_setting_master_id = dpsm.Id
                    AND dpsd.parameter_unique_ref = 'COFF-DT'
                );
            ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "COFF-DT");

            // remove as value as does not convert to decimal
            migrationBuilder.DeleteData(
                table: "default_parameter_setting_detail",
                keyColumn: "parameter_unique_ref",
                keyValue: "COFF-DT");

            migrationBuilder.AlterColumn<decimal>(
                name: "parameter_value",
                table: "default_parameter_setting_detail",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldPrecision: 18,
                oldScale: 3);
        }
    }
}
