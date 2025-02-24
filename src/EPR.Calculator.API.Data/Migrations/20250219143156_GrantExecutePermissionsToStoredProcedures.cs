using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class GrantExecutePermissionsToStoredProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Grant EXECUTE permission to the stored procedures
            migrationBuilder.Sql("GRANT EXECUTE ON dbo.CreateRunOrganization TO dbo;");
            migrationBuilder.Sql("GRANT EXECUTE ON dbo.CreateRunPom TO dbo;");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revoke EXECUTE permission in case of rollback
            migrationBuilder.Sql("REVOKE EXECUTE ON dbo.CreateRunOrganization TO dbo;");
            migrationBuilder.Sql("REVOKE EXECUTE ON dbo.CreateRunPom TO dbo;");
        }
    }
}
