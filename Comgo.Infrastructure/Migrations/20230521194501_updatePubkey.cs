using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Comgo.Infrastructure.Migrations
{
    public partial class updatePubkey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublicKey",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicKey",
                table: "AspNetUsers");
        }
    }
}
