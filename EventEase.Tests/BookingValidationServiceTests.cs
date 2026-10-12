using EventEase.Data;
using EventEase.Models;
using EventEase.Services;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Tests;

public class BookingValidationServiceTests
{
    [Fact]
    public async Task ValidateVenueAvailability_WhenVenueHasEventOnSameDate_ReturnsFailure()
    {
        await using var context = CreateContext();
        context.Events.Add(CreateEvent(1, 10, new DateTime(2026, 10, 15, 9, 0, 0)));
        await context.SaveChangesAsync();
        var service = new BookingValidationService(context);

        var result = await service.ValidateVenueAvailability(
            10,
            new DateTime(2026, 10, 15, 18, 0, 0));

        Assert.False(result.IsSuccess);
        Assert.Contains("already has event", result.Errors.Single());
    }

    [Fact]
    public async Task ValidateVenueAvailability_WhenEditingSameEvent_ReturnsSuccess()
    {
        await using var context = CreateContext();
        context.Events.Add(CreateEvent(1, 10, new DateTime(2026, 10, 15, 9, 0, 0)));
        await context.SaveChangesAsync();
        var service = new BookingValidationService(context);

        var result = await service.ValidateVenueAvailability(
            10,
            new DateTime(2026, 10, 15, 18, 0, 0),
            1);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CanDeleteVenue_WhenAssociatedEventHasBooking_ReturnsFailure()
    {
        await using var context = CreateContext();
        context.Venues.Add(CreateVenue(10));
        context.Events.Add(CreateEvent(1, 10, new DateTime(2026, 10, 15)));
        context.Bookings.Add(CreateBooking(1, 1));
        await context.SaveChangesAsync();
        var service = new BookingValidationService(context);

        var result = await service.CanDeleteVenue(10);

        Assert.False(result.IsSuccess);
        Assert.Contains("active bookings", result.Errors.Single());
    }

    [Fact]
    public async Task CanDeleteVenue_WhenAssociatedEventHasNoBookings_ReturnsSuccess()
    {
        await using var context = CreateContext();
        context.Venues.Add(CreateVenue(10));
        context.Events.Add(CreateEvent(1, 10, new DateTime(2026, 10, 15)));
        await context.SaveChangesAsync();
        var service = new BookingValidationService(context);

        var result = await service.CanDeleteVenue(10);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CanDeleteEvent_WhenEventHasBooking_ReturnsFailure()
    {
        await using var context = CreateContext();
        context.Venues.Add(CreateVenue(10));
        context.Events.Add(CreateEvent(1, 10, new DateTime(2026, 10, 15)));
        context.Bookings.Add(CreateBooking(1, 1));
        await context.SaveChangesAsync();
        var service = new BookingValidationService(context);

        var result = await service.CanDeleteEvent(1);

        Assert.False(result.IsSuccess);
        Assert.Contains("active bookings", result.Errors.Single());
    }

    [Fact]
    public async Task ValidateBookingVenueAvailability_WhenVenueAndDateHaveBooking_ReturnsFailure()
    {
        await using var context = CreateContext();
        context.Venues.Add(CreateVenue(10));
        context.Events.AddRange(
            CreateEvent(1, 10, new DateTime(2026, 10, 15, 9, 0, 0)),
            CreateEvent(2, 10, new DateTime(2026, 10, 15, 18, 0, 0)));
        context.Bookings.Add(CreateBooking(1, 1));
        await context.SaveChangesAsync();
        var service = new BookingValidationService(context);

        var result = await service.ValidateBookingVenueAvailability(2);

        Assert.False(result.IsSuccess);
        Assert.Contains("already has a booking", result.Errors.Single());
    }

    [Fact]
    public async Task ValidateBookingVenueAvailability_WhenEditingSameBooking_ReturnsSuccess()
    {
        await using var context = CreateContext();
        context.Venues.Add(CreateVenue(10));
        context.Events.Add(CreateEvent(1, 10, new DateTime(2026, 10, 15, 9, 0, 0)));
        context.Bookings.Add(CreateBooking(1, 1));
        await context.SaveChangesAsync();
        var service = new BookingValidationService(context);

        var result = await service.ValidateBookingVenueAvailability(1, 1);

        Assert.True(result.IsSuccess);
    }

    private static EventEaseDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<EventEaseDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new EventEaseDbContext(options);
    }

    private static EventEase.Models.Event CreateEvent(int eventId, int venueId, DateTime eventDate)
    {
        return new EventEase.Models.Event
        {
            EventId = eventId,
            EventName = $"Event {eventId}",
            EventDate = eventDate,
            VenueId = venueId
        };
    }

    private static Venue CreateVenue(int venueId)
    {
        return new Venue
        {
            VenueId = venueId,
            VenueName = $"Venue {venueId}",
            Location = "Test location",
            Capacity = 100
        };
    }

    private static Booking CreateBooking(int bookingId, int eventId)
    {
        return new Booking
        {
            BookingId = bookingId,
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            NumberOfTickets = 1,
            EventId = eventId
        };
    }
}
