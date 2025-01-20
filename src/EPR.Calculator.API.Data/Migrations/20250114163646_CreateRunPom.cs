using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateRunPom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("GO\r\n/****** Object:  StoredProcedure [dbo].[CreateRunPom]    Script Date: 14/01/2025 16:36:44 ******/\r\nSET ANSI_NULLS ON\r\nGO\r\nSET QUOTED_IDENTIFIER ON\r\nGO\r\n-- =============================================\r\n-- Author:      Uday Denduluri\r\n-- Create Date: 01/13/2025\r\n-- Description: Creates Org and Pom Run tables from Staging Tables.\r\n-- =============================================\r\nCREATE PROCEDURE [dbo].[CreateRunPom]\r\n(\r\n    -- Add the parameters for the stored procedure here\r\n    @RunId int,\r\n\t@calendarYear varchar(400),\r\n\t@createdBy varchar(400)\r\n)\r\nAS\r\nBEGIN\r\n    -- SET NOCOUNT ON added to prevent extra result sets from\r\n    -- interfering with SELECT statements.\r\n    SET NOCOUNT ON\r\n\r\n\tdeclare @DateNow datetime, @pomDataMasterid int\r\n\tSET @DateNow = GETDATE()\r\n\r\n\tdeclare @oldCalcRunPomMasterId int\r\n    SET @oldCalcRunPomMasterId = (select top 1 id from dbo.calculator_run_pom_data_master order by id desc)\r\n\tUpdate calculator_run_pom_data_master SET effective_to = @DateNow WHERE id = @oldCalcRunPomMasterId\r\n\r\n\tINSERT into dbo.calculator_run_pom_data_master\r\n\t(calendar_year, created_at, created_by, effective_from, effective_to)\r\n\tvalues\r\n\t(@calendarYear, @DateNow, @createdBy, @DateNow, NULL)\r\n\r\n\tSET @pomDataMasterid  = CAST(scope_identity() AS int);\r\n\r\n\tINSERT into \r\n\t\tdbo.calculator_run_pom_data_detail\r\n\t\t(calculator_run_pom_data_master_id, \r\n\t\t\tload_ts,\r\n\t\t\torganisation_id,\r\n\t\t\tpackaging_activity,\r\n\t\t\tpackaging_type,\r\n\t\t\tpackaging_class,\r\n\t\t\tpackaging_material,\r\n\t\t\tpackaging_material_weight,\r\n\t\t\tsubmission_period,\r\n\t\t\tsubmission_period_desc,\r\n\t\t\tsubsidiary_id)\r\n\tSELECT  @pomDataMasterid,\r\n\t\t\tload_ts,\r\n\t\t\torganisation_id,\r\n\t\t\tpackaging_activity,\r\n\t\t\tpackaging_type,\r\n\t\t\tpackaging_class,\r\n\t\t\tpackaging_material,\r\n\t\t\tpackaging_material_weight,\r\n\t\t\tsubmission_period,\r\n\t\t\tsubmission_period_desc,\r\n\t\t\tsubsidiary_id\r\n\t\t\tfrom \r\n\t\t\tdbo.pom_data\r\n\r\n\t Update dbo.calculator_run Set calculator_run_pom_data_master_id = @pomDataMasterid where id = @RunId\r\n\r\nEND\r\n");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[CreateRunPom]");
        }
    }
}
