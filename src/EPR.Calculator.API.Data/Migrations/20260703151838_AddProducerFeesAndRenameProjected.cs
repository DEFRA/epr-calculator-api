using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProducerFeesAndRenameProjected : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "producer_reported_material_projected",
                newName: "producer_material_packaging");

            migrationBuilder.RenameIndex(
                name: "IX_producer_reported_material_projected_material_id",
                newName: "IX_producer_material_packaging_material_id",
                table: "producer_material_packaging");

            migrationBuilder.RenameIndex(
                name: "IX_producer_reported_material_projected_producer_detail_id",
                newName: "IX_producer_material_packaging_producer_detail_id",
                table: "producer_material_packaging");

            migrationBuilder.AlterColumn<string>(
                name: "billing_json_filename",
                table: "calculator_run_billing_file_metadata",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400);

            migrationBuilder.AlterColumn<string>(
                name: "billing_csv_filename",
                table: "calculator_run_billing_file_metadata",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400);

            migrationBuilder.CreateTable(
                name: "calc_result_producer_fees",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calculator_run_id = table.Column<int>(type: "int", nullable: false),
                    details = table.Column<string>(type: "json", nullable: false),
                    total = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calc_result_producer_fees", x => x.id);
                });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "calc_result_producer_fees");

            migrationBuilder.RenameTable(
                name: "producer_material_packaging",
                newName: "producer_reported_material_projected");

            migrationBuilder.RenameIndex(
                name: "IX_producer_material_packaging_material_id",
                newName: "IX_producer_reported_material_projected_material_id",
                table: "producer_reported_material_projected");

            migrationBuilder.RenameIndex(
                name: "IX_producer_material_packaging_producer_detail_id",
                newName: "IX_producer_reported_material_projected_producer_detail_id",
                table: "producer_reported_material_projected");

            migrationBuilder.AlterColumn<string>(
                name: "billing_json_filename",
                table: "calculator_run_billing_file_metadata",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "billing_csv_filename",
                table: "calculator_run_billing_file_metadata",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400,
                oldNullable: true);

        }
    }
}
