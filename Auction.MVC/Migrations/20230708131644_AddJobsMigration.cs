using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auction.MVC.Migrations
{
    public partial class AddJobsMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "jobs");

            migrationBuilder.CreateTable(
                name: "tbjob",
                schema: "jobs",
                columns: table => new
                {
                    JobId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    JsonMessage = table.Column<string>(type: "text", nullable: false),
                    QueueName = table.Column<string>(type: "text", nullable: false),
                    StartAfter = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExpireAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbjob", x => x.JobId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbjob",
                schema: "jobs");
        }
    }
}
