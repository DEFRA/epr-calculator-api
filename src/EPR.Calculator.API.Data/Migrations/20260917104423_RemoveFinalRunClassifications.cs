using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFinalRunClassifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Runs referencing a retired classification must be remapped before the lookup rows are
            // deleted, otherwise the calculator_run foreign key rejects the deletes. Ids are written
            // literally because this migration must keep working as RunClassification evolves.
            migrationBuilder.Sql(
                """
                -- 'IN THE QUEUE' (1) is not expected to exist; anything left in it is treated as errored.
                UPDATE [calculator_run]
                SET [calculator_run_classification_id] = 5
                WHERE [calculator_run_classification_id] = 1;

                -- 'FINAL RUN' (10) and 'FINAL RE-CALCULATION RUN' (11) become 'RECALCULATION RUN' (9).
                UPDATE [calculator_run]
                SET [calculator_run_classification_id] = 9
                WHERE [calculator_run_classification_id] IN (10, 11);

                -- Their completed equivalents become 'RECALCULATION RUN COMPLETED' (12).
                UPDATE [calculator_run]
                SET [calculator_run_classification_id] = 12
                WHERE [calculator_run_classification_id] IN (13, 14);
                """);

            migrationBuilder.DeleteData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 14);

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 2,
                column: "created_by",
                value: "System User");

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 3,
                column: "created_by",
                value: "System User");

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 4,
                column: "created_by",
                value: "System User");

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 5,
                column: "created_by",
                value: "System User");

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 8,
                column: "created_by",
                value: "System User");

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_by", "status" },
                values: new object[] { "System User", "RECALCULATION RUN" });

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 12,
                column: "status",
                value: "RECALCULATION RUN COMPLETED");
        }

        /// <inheritdoc />
        /// <remarks>
        ///     Restores the lookup rows only. The remap in <c>Up</c> is lossy - once 'final' and
        ///     'interim' runs have been merged there is no way to tell them apart again - so runs
        ///     stay on their remapped classification.
        /// </remarks>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 2,
                column: "created_by",
                value: "Test User");

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 3,
                column: "created_by",
                value: "Test User");

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 4,
                column: "created_by",
                value: "Test User");

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 5,
                column: "created_by",
                value: "Test User");

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 8,
                column: "created_by",
                value: "Test user");

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_by", "status" },
                values: new object[] { "Test user", "INTERIM RE-CALCULATION RUN" });

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 12,
                column: "status",
                value: "INTERIM RE-CALCULATION RUN COMPLETED");

            migrationBuilder.InsertData(
                table: "calculator_run_classification",
                columns: new[] { "id", "created_by", "status" },
                values: new object[,]
                {
                    { 1, "Test User", "IN THE QUEUE" },
                    { 10, "Test user", "FINAL RUN" },
                    { 11, "Test user", "FINAL RE-CALCULATION RUN" },
                    { 13, "System User", "FINAL RE-CALCULATION RUN COMPLETED" },
                    { 14, "System User", "FINAL RUN COMPLETED" }
                });
        }
    }
}
