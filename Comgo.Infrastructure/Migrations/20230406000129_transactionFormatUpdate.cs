using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Comgo.Infrastructure.Migrations
{
    public partial class transactionFormatUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "DebitAccount",
                table: "Transactions",
                newName: "DebitAddress");

            migrationBuilder.RenameColumn(
                name: "CreditAccount",
                table: "Transactions",
                newName: "CreditAddress");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DebitAddress",
                table: "Transactions",
                newName: "DebitAccount");

            migrationBuilder.RenameColumn(
                name: "CreditAddress",
                table: "Transactions",
                newName: "CreditAccount");

            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
