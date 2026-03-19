using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixRecreatePomOrgDataTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop and recreate pom/org data staging tables -  Current stored data is irrelevant.
            // Required because Synapse Pipelines may have altered them independent of this migration.
            migrationBuilder.DropTable(name: "organisation_data");

            migrationBuilder.Sql(@"
                CREATE TABLE [dbo].[organisation_data](
	                [organisation_id] [int] NOT NULL,
	                [subsidiary_id] [nvarchar](400) NULL,
	                [organisation_name] [nvarchar](400) NOT NULL,
	                [load_ts] [datetime2](7) NOT NULL,
	                [trading_name] [nvarchar](400) NULL,
	                [obligation_status] [nvarchar](10) NOT NULL,
	                [submitter_id] [uniqueidentifier] NULL,
	                [error_code] [nvarchar](max) NULL,
	                [num_days_obligated] [int] NULL,
	                [status_code] [nvarchar](max) NULL,
	                [joiner_date] [nvarchar](50) NULL,
	                [leaver_date] [nvarchar](50) NULL
                ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
                ");

            migrationBuilder.DropTable(name: "pom_data");

            migrationBuilder.Sql(@"
                CREATE TABLE [dbo].[pom_data](
	                [organisation_id] [int] NULL,
	                [subsidiary_id] [nvarchar](400) NULL,
	                [submission_period] [nvarchar](400) NULL,
	                [packaging_activity] [nvarchar](400) NULL,
	                [packaging_type] [nvarchar](400) NULL,
	                [packaging_class] [nvarchar](400) NULL,
	                [packaging_material] [nvarchar](max) NULL,
	                [packaging_material_weight] [float] NULL,
	                [load_ts] [datetime2](7) NOT NULL,
	                [submission_period_desc] [nvarchar](max) NULL,
	                [submitter_id] [uniqueidentifier] NULL
                ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
                ");
        }
    }
}