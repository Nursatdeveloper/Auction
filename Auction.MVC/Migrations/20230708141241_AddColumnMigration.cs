using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auction.MVC.Migrations
{
    public partial class AddColumnMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "auction",
                table: "tbtrade",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                schema: "auction",
                table: "tbtrade");
        }
    }
}
