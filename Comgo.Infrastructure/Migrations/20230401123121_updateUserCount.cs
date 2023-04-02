using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Comgo.Infrastructure.Migrations
{
    public partial class updateUserCount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropTable(
                name: "UserCustodies");

            migrationBuilder.DropColumn(
                name: "AdminUserId",
                table: "Signatures");

            migrationBuilder.DropColumn(
                name: "UserSigTree",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "UserKey",
                table: "Signatures",
                newName: "UserSafeDetails");

            migrationBuilder.RenameColumn(
                name: "SystemKey",
                table: "Signatures",
                newName: "UserPubKey");

            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "Signatures",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserCount",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Signatures_AppUserId",
                table: "Signatures",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Signatures_AspNetUsers_AppUserId",
                table: "Signatures",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Signatures_AspNetUsers_AppUserId",
                table: "Signatures");

            migrationBuilder.DropIndex(
                name: "IX_Signatures_AppUserId",
                table: "Signatures");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "Signatures");

            migrationBuilder.DropColumn(
                name: "UserCount",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "UserSafeDetails",
                table: "Signatures",
                newName: "UserKey");

            migrationBuilder.RenameColumn(
                name: "UserPubKey",
                table: "Signatures",
                newName: "SystemKey");

            migrationBuilder.AddColumn<string>(
                name: "AdminUserId",
                table: "Signatures",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserSigTree",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KeyPhrase = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Units_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserCustodies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCustodies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCustodies_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Units_AppUserId",
                table: "Units",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCustodies_AppUserId",
                table: "UserCustodies",
                column: "AppUserId");
        }
    }
}
