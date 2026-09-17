using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventEase.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingDisplayView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create the BookingDisplayView that consolidates booking information
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
                    e.Description as EventDescription,
                    v.VenueId,
                    v.VenueName,
                    v.Location,
                    v.Capacity
                FROM Bookings b
                INNER JOIN Events e ON b.EventId = e.EventId
                INNER JOIN Venues v ON e.VenueId = v.VenueId
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.BookingDisplayView");
        }
    }
}
