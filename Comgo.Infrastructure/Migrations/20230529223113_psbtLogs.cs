using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Comgo.Infrastructure.Migrations
{
    public partial class psbtLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PSBTs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InitialPSBT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserSignedPSBT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SystemSignedPSBT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FinalizedPSBT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShouldProcessPSBT = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PSBTs", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PSBTs");
        }
    }
}
