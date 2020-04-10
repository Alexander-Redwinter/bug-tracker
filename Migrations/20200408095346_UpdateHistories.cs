using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BugTracker.Migrations
{
    public partial class UpdateHistories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "TicketHistories",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUser",
                table: "TicketHistories",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeyValue",
                table: "TicketHistories",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewValue",
                table: "TicketHistories",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldValue",
                table: "TicketHistories",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "When",
                table: "TicketHistories",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                table: "TicketHistories");

            migrationBuilder.DropColumn(
                name: "ApplicationUser",
                table: "TicketHistories");

            migrationBuilder.DropColumn(
                name: "KeyValue",
                table: "TicketHistories");

            migrationBuilder.DropColumn(
                name: "NewValue",
                table: "TicketHistories");

            migrationBuilder.DropColumn(
                name: "OldValue",
                table: "TicketHistories");

            migrationBuilder.DropColumn(
                name: "When",
                table: "TicketHistories");
        }
    }
}
