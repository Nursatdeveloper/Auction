using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auction.MVC.Migrations
{
    public partial class SecondMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "users");

            migrationBuilder.RenameTable(
                name: "tbuser",
                schema: "user",
                newName: "tbuser",
                newSchema: "users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "user");

            migrationBuilder.RenameTable(
                name: "tbuser",
                schema: "users",
                newName: "tbuser",
                newSchema: "user");
        }
    }
}
