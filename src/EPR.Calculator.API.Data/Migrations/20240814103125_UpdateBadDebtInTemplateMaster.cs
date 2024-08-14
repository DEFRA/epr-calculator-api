using Microsoft.EntityFrameworkCore.Migrations;
using System.Text;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBadDebtInTemplateMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sb = new StringBuilder();
            sb.AppendLine("update dbo.default_parameter_template_master");
            sb.AppendLine("set parameter_type = 'Bad debt provision',");
            sb.AppendLine("parameter_category = 'Percentage'");
            sb.AppendLine("where parameter_type like '%Bad debt provision percentage%'");
            var sqlString = sb.ToString();
            migrationBuilder.Sql(sqlString);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
