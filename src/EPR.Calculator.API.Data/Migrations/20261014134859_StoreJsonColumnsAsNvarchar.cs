using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class StoreJsonColumnsAsNvarchar : Migration
    {
        private static readonly (string Table, string Column)[] JsonColumns =
        [
            ("calc_result_producer_fees"              , "total"),
            ("calc_result_producer_fee_detail"        , "detail"),
            ("calc_result_modulation"                 , "material_modulations"),
            ("calc_result_smcw"                       , "material_totals"),
            ("calc_result_smcw_producer"              , "material_totals"),
            ("calc_result_cancelled_producer"         , "cancelled_producer"),
            ("calc_result_comms_cost"                 , "comms_cost"),
            ("calc_result_la_disposal_cost"           , "la_disposal_cost"),
            ("calc_result_lapcap_data"                , "lapcap"),
            ("calc_result_late_reporting_tonnage"     , "late_reporting_tonnage"),
            ("calc_result_one_plus_four_apportionment", "one_plus_four_apppointment"),
            ("calc_result_parameter_other_cost"       , "parameter_other_cost")
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var (table, column) in JsonColumns)
                ConvertColumn(migrationBuilder, table, column, "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var (table, column) in JsonColumns)
                ConvertColumn(migrationBuilder, table, column, "json");
        }

        // SQL Server won't ALTER COLUMN between json and nvarchar(max), so copy through a new column instead.
        private static void ConvertColumn(MigrationBuilder migrationBuilder, string table, string column, string type)
        {
            migrationBuilder.Sql($@"
                ALTER TABLE [{table}] ADD [{column}_converted] {type} NULL;
                EXEC(N'UPDATE [{table}] SET [{column}_converted] = CONVERT({type}, [{column}]);');
                ALTER TABLE [{table}] DROP COLUMN [{column}];
                EXEC sp_rename N'[{table}].[{column}_converted]', N'{column}', N'COLUMN';
                EXEC(N'ALTER TABLE [{table}] ALTER COLUMN [{column}] {type} NOT NULL;');");
        }
    }
}
