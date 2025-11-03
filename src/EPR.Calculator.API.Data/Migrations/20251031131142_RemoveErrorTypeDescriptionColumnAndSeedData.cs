using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveErrorTypeDescriptionColumnAndSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                table: "error_type");

            migrationBuilder.CreateIndex(
                name: "IX_error_type_name",
                table: "error_type",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_error_type_name",
                table: "error_type");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "error_type",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "error_type",
                keyColumn: "id",
                keyValue: 1,
                column: "description",
                value: "Where there is a misalignment between the POM files structure and the Registration file structure re Producer IDs and Subsidiary IDs");

            migrationBuilder.UpdateData(
                table: "error_type",
                keyColumn: "id",
                keyValue: 2,
                column: "description",
                value: "Where there is more than one entry for a producer that has an obligated leaver code.");

            migrationBuilder.UpdateData(
                table: "error_type",
                keyColumn: "id",
                keyValue: 3,
                column: "description",
                value: "Where there is more than one entry for a producer that has a blank leaver code and where there are no obligated leaver code entries");

            migrationBuilder.UpdateData(
                table: "error_type",
                keyColumn: "id",
                keyValue: 4,
                column: "description",
                value: "Where a producer is flagged with a leaver code of 11 (Insolvent) or 12 (No longer performing a producer function)");

            migrationBuilder.UpdateData(
                table: "error_type",
                keyColumn: "id",
                keyValue: 5,
                column: "description",
                value: "Where a producer only appears with Not Obligated leaver codes.");

            migrationBuilder.UpdateData(
                table: "error_type",
                keyColumn: "id",
                keyValue: 6,
                column: "description",
                value: "Where the producer is only flagged as a leaver of a compliance scheme (Leaver Code 13 and 14)");

            migrationBuilder.UpdateData(
                table: "error_type",
                keyColumn: "id",
                keyValue: 7,
                column: "description",
                value: "Where a producer leaves a compliance scheme and is obligated as a direct producer.");

            migrationBuilder.UpdateData(
                table: "error_type",
                keyColumn: "id",
                keyValue: 8,
                column: "description",
                value: "Where a producer has an entry for a non-valid leaver code.");

            migrationBuilder.UpdateData(
                table: "error_type",
                keyColumn: "id",
                keyValue: 9,
                column: "description",
                value: "Where a leaver or joiner date falls outside of the calendar year boundary.");

            migrationBuilder.UpdateData(
                table: "error_type",
                keyColumn: "id",
                keyValue: 10,
                column: "description",
                value: "Where a Organisation (Producer or Subsidiary) ID does not conform to the 6 digit structure.");
        }
    }
}
