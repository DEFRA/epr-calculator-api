using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionPeriodLookup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "submission_period_lookup",
                columns: table => new
                {
                    submission_period = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    submission_period_desc = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    days_in_submission_period = table.Column<int>(type: "int", nullable: false),
                    days_in_whole_period = table.Column<int>(type: "int", nullable: false),
                    scaleup_factor = table.Column<decimal>(type: "decimal(16,12)", precision: 16, scale: 12, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_submission_period_lookup", x => x.submission_period);
                });

            migrationBuilder.InsertData(
                table: "submission_period_lookup",
                columns: new[] {
                    "submission_period",
                    "submission_period_desc",
                    "start_date",
                    "end_date",
                    "days_in_submission_period",
                    "days_in_whole_period",
                    "scaleup_factor"
                },
                values: new object[,]
                {
                    {
                        "2024-P1",
                        "January to June 2024",
                        new DateTime(2024, 01, 01, 00, 00, 00, 000, DateTimeKind.Local),
                        new DateTime(2024, 06, 30, 23, 59, 59, 000, DateTimeKind.Local),
                        182,
                        182,
                        1
                    },
                    {
                       "2024-P2",
                        "April to June 2024",
                        new DateTime(2024, 04, 01, 00, 00, 00, 000, DateTimeKind.Local),
                        new DateTime(2024, 06, 30, 23, 59, 59, 000, DateTimeKind.Local),
                        91,
                        182,
                        2
                    },
                    {
                        "2024-P3",
                        "May to June 2024",
                        new DateTime(2024, 05, 01, 00, 00, 00, 000, DateTimeKind.Local),
                        new DateTime(2024, 06, 30, 23, 59, 59, 000, DateTimeKind.Local),
                        61,
                        182,
                        2.983606557377
                    },
                    {
                        "2024-P4",
                        "July to December 2024",
                        new DateTime(2024, 07, 01, 00, 00, 00, 000, DateTimeKind.Local),
                        new DateTime(2024, 12, 31, 23, 59, 59, 000, DateTimeKind.Local),
                        184,
                        184,
                        1
                    }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "submission_period_lookup");
        }
    }
}
