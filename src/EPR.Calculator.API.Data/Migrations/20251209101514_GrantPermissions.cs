using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class GrantPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Grant EXECUTE on stored procedures to PUBLIC
            migrationBuilder.Sql(@"GRANT EXEC ON [dbo].[InsertInvoiceDetailsAtProducerLevel] TO PUBLIC;");
            migrationBuilder.Sql(@"GRANT EXEC ON [dbo].[CreateRunPom] TO PUBLIC;");
            migrationBuilder.Sql(@"GRANT EXEC ON [dbo].[CreateRunOrganization] TO PUBLIC;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revoke EXECUTE on specific stored procedures from PUBLIC
            migrationBuilder.Sql(@"REVOKE EXECUTE ON [dbo].[InsertInvoiceDetailsAtProducerLevel] FROM PUBLIC;");
            migrationBuilder.Sql(@"REVOKE EXECUTE ON [dbo].[CreateRunPom] FROM PUBLIC;");
            migrationBuilder.Sql(@"REVOKE EXECUTE ON [dbo].[CreateRunOrganization] FROM PUBLIC;");
        }
    }
}
