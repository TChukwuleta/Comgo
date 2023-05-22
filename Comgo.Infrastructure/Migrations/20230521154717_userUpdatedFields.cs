using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Comgo.Infrastructure.Migrations
{
    public partial class userUpdatedFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsWalletCreated",
                table: "AspNetUsers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WalletName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsWalletCreated",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "WalletName",
                table: "AspNetUsers");
        }
    }
}
