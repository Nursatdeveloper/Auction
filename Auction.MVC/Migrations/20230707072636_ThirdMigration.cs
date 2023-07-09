using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auction.MVC.Migrations
{
    public partial class ThirdMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long[]>(
                name: "TradeIds",
                schema: "users",
                table: "tbuser",
                type: "bigint[]",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TradeIds",
                schema: "users",
                table: "tbuser");
        }
    }
}
