using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFutureSubmissionPeriods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            AddYear(migrationBuilder, 2025);
            AddYear(migrationBuilder, 2026);
            AddYear(migrationBuilder, 2027);
            AddYear(migrationBuilder, 2028);
            AddYear(migrationBuilder, 2029);
            AddYear(migrationBuilder, 2030);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            RemoveYear(migrationBuilder, 2025);
            RemoveYear(migrationBuilder, 2026);
            RemoveYear(migrationBuilder, 2027);
            RemoveYear(migrationBuilder, 2028);
            RemoveYear(migrationBuilder, 2029);
            RemoveYear(migrationBuilder, 2030);
        }

        private static void AddYear(MigrationBuilder migrationBuilder, int year)
        {
            migrationBuilder.InsertData(
                table: "submission_period_lookup",
                columns: new[]
                {
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
                        $"{year}-P1",
                        $"January to June {year}",
                        new DateTime(year, 01, 01, 00, 00, 00, 000, DateTimeKind.Local),
                        new DateTime(year, 06, 30, 23, 59, 59, 000, DateTimeKind.Local),
                        182,
                        182,
                        1
                    },
                    {
                        $"{year}-P4",
                        $"July to December {year}",
                        new DateTime(year, 07, 01, 00, 00, 00, 000, DateTimeKind.Local),
                        new DateTime(year, 12, 31, 23, 59, 59, 000, DateTimeKind.Local),
                        184,
                        184,
                        1
                    }
                });
        }

        private static void RemoveYear(MigrationBuilder migrationBuilder, int year)
        {
            migrationBuilder.DeleteData(
                table: "submission_period_lookup",
                keyColumn: "submission_period",
                keyValue: $"{year}-P1");
            
            migrationBuilder.DeleteData(
                table: "submission_period_lookup",
                keyColumn: "submission_period",
                keyValue: $"{year}-P4");
        }
    }
}
