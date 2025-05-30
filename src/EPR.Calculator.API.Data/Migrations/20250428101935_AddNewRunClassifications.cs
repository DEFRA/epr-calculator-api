﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewRunClassifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
               table: "calculator_run_classification",
               columns: new[]
               {
                    "status",
                    "created_by",
               },
               values: new object[,]
               {
                    {
                        "INITIAL RUN",
                        "Test user",
                    },
                    {
                        "INTERIM RE-CALCULATION RUN",
                        "Test user",
                    },
                    {
                        "FINAL RUN",
                        "Test user",
                    },
                    {
                        "FINAL RE-CALCULATION RUN",
                        "Test user",
                    },
               });

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 4,
                column: "status",
                value: "TEST RUN"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Master Data no Down
        }
    }
}
