using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    [ExcludeFromCodeCoverage]
    /// <inheritdoc />
    public partial class _202407311405_UpdateTemplateMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert data safely using SQL
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM [default_parameter_template_master]
                    WHERE [parameter_unique_ref] = 'TONT-AD'
                )
                BEGIN
                    IF EXISTS (
                        SELECT * FROM [sys].[identity_columns]
                        WHERE [object_id] = OBJECT_ID('default_parameter_template_master')
                    )
                        SET IDENTITY_INSERT [default_parameter_template_master] ON;

                    INSERT INTO [default_parameter_template_master]
                    ([parameter_unique_ref], [parameter_type], [parameter_category], [valid_Range_from], [valid_Range_to])
                    VALUES ('TONT-AD', 'Amount Decrease', 'Tonnage change threshold', 0.00, 999999999.99);

                    IF EXISTS (
                        SELECT * FROM [sys].[identity_columns]
                        WHERE [object_id] = OBJECT_ID('default_parameter_template_master')
                    )
                        SET IDENTITY_INSERT [default_parameter_template_master] OFF;
                END
            ");

            // Update reference to new parameter (if needed)
            migrationBuilder.Sql(@"
                UPDATE [default_parameter_setting_detail]
                SET [parameter_unique_ref] = 'TONT-AD'
                WHERE [parameter_unique_ref] = 'TONT-DI';
            ");

            // Delete the old template entry
            migrationBuilder.Sql(@"
                DELETE FROM [default_parameter_template_master]
                WHERE [parameter_unique_ref] = 'TONT-DI';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Optional: Add logic to reverse the insert/update/delete if ever needed
        }
    }
}