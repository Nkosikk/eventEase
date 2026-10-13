using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventEase.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTypeAndVenueAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.BookingDisplayView");

            migrationBuilder.AddColumn<int>(
                name: "EventTypeId",
                table: "Venues",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Venues",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                table: "Bookings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "EventTypes",
                columns: table => new
                {
                    EventTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventTypeName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTypes", x => x.EventTypeId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Venues_EventTypeId",
                table: "Venues",
                column: "EventTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Venues_EventTypes_EventTypeId",
                table: "Venues",
                column: "EventTypeId",
                principalTable: "EventTypes",
                principalColumn: "EventTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
                CREATE VIEW dbo.BookingDisplayView AS
                SELECT
                    b.BookingId,
                    b.CustomerName,
                    b.CustomerEmail,
                    b.BookingDate,
                    b.NumberOfTickets,
                    e.EventId,
                    e.EventName,
                    e.EventDate,
                    e.Description AS EventDescription,
                    v.VenueId,
                    v.VenueName,
                    v.Location,
                    v.Capacity,
                    v.EventTypeId,
                    COALESCE(et.EventTypeName, 'Unassigned') AS EventTypeName,
                    v.IsAvailable
                FROM Bookings b
                INNER JOIN Events e ON b.EventId = e.EventId
                INNER JOIN Venues v ON e.VenueId = v.VenueId
                LEFT JOIN EventTypes et ON v.EventTypeId = et.EventTypeId
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.BookingDisplayView");

            migrationBuilder.DropForeignKey(
                name: "FK_Venues_EventTypes_EventTypeId",
                table: "Venues");

            migrationBuilder.DropTable(
                name: "EventTypes");

            migrationBuilder.DropIndex(
                name: "IX_Venues_EventTypeId",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "EventTypeId",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Venues");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.Sql(@"
                CREATE VIEW dbo.BookingDisplayView AS
                SELECT
                    b.BookingId,
                    b.CustomerName,
                    b.CustomerEmail,
                    b.BookingDate,
                    b.NumberOfTickets,
                    e.EventId,
                    e.EventName,
                    e.EventDate,
                    e.Description AS EventDescription,
                    v.VenueId,
                    v.VenueName,
                    v.Location,
                    v.Capacity
                FROM Bookings b
                INNER JOIN Events e ON b.EventId = e.EventId
                INNER JOIN Venues v ON e.VenueId = v.VenueId
            ");
        }
    }
}
