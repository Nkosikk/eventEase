using EventEase.Data;
using EventEase.Models;
using EventEase.Pages.Bookings;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Tests;

public class BookingsIndexTests
{
    [Fact]
    public async Task OnGetAsync_WithEventTypeFilter_ReturnsMatchingBookings()
    {
        await using var context = await CreateContextAsync();
        var page = new IndexModel(context) { EventTypeId = 2 };

        await page.OnGetAsync();

        var booking = Assert.Single(page.Bookings);
        Assert.Equal(3, booking.BookingId);
        Assert.Equal("Wedding", booking.EventTypeName);
    }

    [Fact]
    public async Task OnGetAsync_WithDateRange_IncludesEntireEndDate()
    {
        await using var context = await CreateContextAsync();
        var page = new IndexModel(context)
        {
            StartDate = new DateTime(2026, 6, 15),
            EndDate = new DateTime(2026, 6, 15)
        };

        await page.OnGetAsync();

        var booking = Assert.Single(page.Bookings);
        Assert.Equal(2, booking.BookingId);
    }

    [Fact]
    public async Task OnGetAsync_WithAvailabilityFilter_ReturnsMatchingBookings()
    {
        await using var context = await CreateContextAsync();
        var page = new IndexModel(context) { IsAvailable = false };

        await page.OnGetAsync();

        var booking = Assert.Single(page.Bookings);
        Assert.Equal(2, booking.BookingId);
        Assert.False(booking.IsAvailable);
    }

    [Fact]
    public async Task OnGetAsync_WithCombinedFilters_ReturnsOnlyCommonMatches()
    {
        await using var context = await CreateContextAsync();
        var page = new IndexModel(context)
        {
            EventTypeId = 1,
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 6, 30),
            IsAvailable = true
        };

        await page.OnGetAsync();

        var booking = Assert.Single(page.Bookings);
        Assert.Equal(1, booking.BookingId);
    }

    [Fact]
    public async Task VenuesIndex_WithUnassignedEventType_LoadsLegacyVenue()
    {
        await using var context = await CreateContextAsync();
        context.Venues.Add(new Venue
        {
            VenueId = 4,
            VenueName = "Legacy Venue",
            Location = "Test location",
            Capacity = 100,
            EventTypeId = null,
            IsAvailable = true
        });
        await context.SaveChangesAsync();
        var page = new EventEase.Pages.Venues.IndexModel(context);

        await page.OnGetAsync();

        Assert.Contains(page.Venues, venue => venue.VenueId == 4 && venue.EventTypeId == null);
    }

    [Fact]
    public async Task BookingsIndex_WithUnassignedEventType_LabelsLegacyBooking()
    {
        await using var context = await CreateContextAsync();
        var legacyVenue = new Venue
        {
            VenueId = 4,
            VenueName = "Legacy Venue",
            Location = "Test location",
            Capacity = 100,
            EventTypeId = null,
            IsAvailable = true
        };
        var legacyEvent = CreateEvent(4, legacyVenue, new DateTime(2026, 8, 1));
        context.AddRange(legacyVenue, legacyEvent, CreateBooking(4, legacyEvent));
        await context.SaveChangesAsync();
        var page = new IndexModel(context);

        await page.OnGetAsync();

        var booking = Assert.Single(page.Bookings, booking => booking.BookingId == 4);
        Assert.Null(booking.EventTypeId);
        Assert.Equal("Unassigned", booking.EventTypeName);
    }

    private static async Task<EventEaseDbContext> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<EventEaseDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new EventEaseDbContext(options);

        var conference = new EventType { EventTypeId = 1, EventTypeName = "Conference" };
        var wedding = new EventType { EventTypeId = 2, EventTypeName = "Wedding" };
        var availableConferenceVenue = CreateVenue(1, conference, true);
        var unavailableConferenceVenue = CreateVenue(2, conference, false);
        var availableWeddingVenue = CreateVenue(3, wedding, true);
        var conferenceEvent = CreateEvent(1, availableConferenceVenue, new DateTime(2026, 6, 10, 9, 0, 0));
        var unavailableEvent = CreateEvent(2, unavailableConferenceVenue, new DateTime(2026, 6, 15, 18, 0, 0));
        var weddingEvent = CreateEvent(3, availableWeddingVenue, new DateTime(2026, 7, 1, 12, 0, 0));

        context.AddRange(
            conference,
            wedding,
            availableConferenceVenue,
            unavailableConferenceVenue,
            availableWeddingVenue,
            conferenceEvent,
            unavailableEvent,
            weddingEvent,
            CreateBooking(1, conferenceEvent),
            CreateBooking(2, unavailableEvent),
            CreateBooking(3, weddingEvent));
        await context.SaveChangesAsync();

        return context;
    }

    private static Venue CreateVenue(int venueId, EventType eventType, bool isAvailable)
    {
        return new Venue
        {
            VenueId = venueId,
            VenueName = $"Venue {venueId}",
            Location = "Test location",
            Capacity = 100,
            EventTypeId = eventType.EventTypeId,
            EventType = eventType,
            IsAvailable = isAvailable
        };
    }

    private static EventEase.Models.Event CreateEvent(int eventId, Venue venue, DateTime eventDate)
    {
        return new EventEase.Models.Event
        {
            EventId = eventId,
            EventName = $"Event {eventId}",
            EventDate = eventDate,
            VenueId = venue.VenueId,
            Venue = venue
        };
    }

    private static Booking CreateBooking(int bookingId, EventEase.Models.Event eventItem)
    {
        return new Booking
        {
            BookingId = bookingId,
            CustomerName = $"Customer {bookingId}",
            CustomerEmail = $"customer{bookingId}@example.com",
            BookingDate = new DateTime(2026, 1, bookingId),
            NumberOfTickets = 1,
            EventId = eventItem.EventId,
            Event = eventItem
        };
    }
}
