using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Comgo.Infrastructure.Migrations
{
    public partial class updateDescriptor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Descriptor",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descriptor",
                table: "AspNetUsers");
        }
    }
}
