using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateMasterDataForCalcResultsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "cost_type",
                columns: new[] { "id", "code", "name", "description" },
                values: new object[,]
                {
                    {"1", "1", "Fee for LA Disposal Costs", "Fee for LA Disposal Costs" },
                    {"2", "4", "LA Data Prep Charge", "LA Data Prep Charge"}
                });

            migrationBuilder.InsertData(
               table: "country",
               columns: new[] { "id", "code", "name", "description" },
               values: new object[,]
               {
                   {"1","ENG","England","England" },
                   {"2", "WLS","Wales","Wales" },
                   {"3", "SCT","Scotland","Scotland",},
                   {"4", "NIR","Northern Ireland","Northern Ireland" }
               });

            migrationBuilder.InsertData(
               table: "material",
               columns: new[] { "id", "code", "name", "description" },
               values: new object[,]
               {
                   {"1", "AL","Aluminium","Aluminium" },
                   {"2", "FC","Fibre composite","Fibre composite" },
                   {"3", "GL","Glass","Glass",},
                   {"4", "PC","Paper or card","Paper or card" },
                   {"5", "PL","Plastic","Plastic" },
                   {"6", "ST","Steel","Steel"},
                   {"7", "WD","Wood","Wood"},
                   {"8", "OT","Other materials","Other materials"}
               });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //As we are inserting default values to these tables we do not need the Down function implementation.
        }
    }
}