using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateErrorTypesSeedDataReplaceUnknownType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "error_type",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "description", "name" },
                values: new object[] { "Where a leaver or joiner date falls outside of the calendar year boundary.", "Date input issue" });

            migrationBuilder.InsertData(
                table: "error_type",
                columns: new[] { "id", "description", "name" },
                values: new object[] { 10, "Where a Organisation (Producer or Subsidiary) ID does not conform to the 6 digit structure.", "Invalid Organisation ID" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "error_type",
                keyColumn: "id",
                keyValue: 10);

            migrationBuilder.UpdateData(
                table: "error_type",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "description", "name" },
                values: new object[] { "Catch all for other errors", "Unknown error" });
        }
    }
}
