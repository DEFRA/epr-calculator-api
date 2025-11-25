using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddErrorTypesReferenceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_error_type_name",
                table: "error_type");

            migrationBuilder.InsertData(
                table: "error_type",
                columns: new[] { "id", "description", "name" },
                values: new object[,]
                {
                    { 1, "Where there is a misalignment between the POM files structure and the Registration file structure re Producer IDs and Subsidiary IDs", "Missing Registration Data" },
                    { 2, "Where there is more than one entry for a producer that has an obligated leaver code.", "Conflicting Obligations (Leaver Codes)" },
                    { 3, "Where there is more than one entry for a producer that has a blank leaver code and where there are no obligated leaver code entries", "Conflicting Obligations (Blank)" },
                    { 4, "Where a producer is flagged with a leaver code of 11 (Insolvent) or 12 (No longer performing a producer function)", "No longer trading" },
                    { 5, "Where a producer only appears with Not Obligated leaver codes.", "Not Obligated" },
                    { 6, "Where the producer is only flagged as a leaver of a compliance scheme (Leaver Code 13 and 14)", "Compliance Scheme Leaver" },
                    { 7, "Where a producer leaves a compliance scheme and is obligated as a direct producer.", "Compliance Scheme to Direct Producer" },
                    { 8, "Where a producer has an entry for a non-valid leaver code.", "Invalid Leaver Code" },
                    { 9, "Catch all for other errors", "Unknown error" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "error_type",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "error_type",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "error_type",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "error_type",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "error_type",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "error_type",
                keyColumn: "id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "error_type",
                keyColumn: "id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "error_type",
                keyColumn: "id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "error_type",
                keyColumn: "id",
                keyValue: 9);

            migrationBuilder.CreateIndex(
                name: "IX_error_type_name",
                table: "error_type",
                column: "name",
                unique: true);
        }
    }
}
