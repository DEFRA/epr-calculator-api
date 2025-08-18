using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProducerIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
              name: "IX_producer_resultfile_suggested_billing_instruction_producer_id",
              table: "producer_resultfile_suggested_billing_instruction",
              column: "producer_id");

            migrationBuilder.CreateIndex(
              name: "IX_producer_detail_producer_id",
              table: "producer_detail",
              column: "producer_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
              name: "IX_producer_resultfile_suggested_billing_instruction_producer_id",
              table: "producer_resultfile_suggested_billing_instruction");

            migrationBuilder.DropIndex(
              name: "IX_producer_detail_producer_id",
              table: "producer_detail");
        }
    }
}
