using Microsoft.EntityFrameworkCore.Migrations;
using System.Text;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOtherParamLapcap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sb = new StringBuilder();
            sb.AppendLine("update dbo.lapcap_data_template_master ");
            sb.AppendLine("set material = 'Other materials'");
            sb.AppendLine("where material like 'Other'");
            var sqlString = sb.ToString();
            migrationBuilder.Sql(sqlString);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sb = new StringBuilder();
            sb.AppendLine("update dbo.lapcap_data_template_master ");
            sb.AppendLine("set material = 'Other'");
            sb.AppendLine("where material like 'Other materials'");
            var sqlString = sb.ToString();
            migrationBuilder.Sql(sqlString);
        }
    }
}
