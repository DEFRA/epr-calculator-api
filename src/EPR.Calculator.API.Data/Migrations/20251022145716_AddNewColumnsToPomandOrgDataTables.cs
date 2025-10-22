using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewColumnsToPomandOrgDataTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "submitter_org_id",
                table: "pom_data",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "leaver_code",
                table: "organisation_data",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "leaver_date",
                table: "organisation_data",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
               name: "joiner_date",
               table: "organisation_data",
               type: "nvarchar(max)",
               nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "submitter_org_id",
                table: "organisation_data",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "submitter_org_id",
                table: "pom_data");

            migrationBuilder.DropColumn(
                name: "joiner_date",
                table: "organisation_data");

            migrationBuilder.DropColumn(
                name: "leaver_code",
                table: "organisation_data");

            migrationBuilder.DropColumn(
                name: "leaver_date",
                table: "organisation_data");

            migrationBuilder.DropColumn(
                name: "submitter_org_id",
                table: "organisation_data");
        }
    }
}
