using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLapcapTotalCostFromValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [lapcap_data_template_master] SET [total_cost_from] = -999999999.99 WHERE [total_cost_from] = 0.00");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [lapcap_data_template_master] SET [total_cost_from] = 0.00 WHERE [total_cost_from] = -999999999.99");
        }
    }
}
