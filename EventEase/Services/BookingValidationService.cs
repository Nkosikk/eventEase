using EventEase.Data;
using EventEase.Models;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Services
{
    /// <summary>
    /// Service for validating booking and venue/event operations.
    /// Handles business logic validation such as preventing double bookings
    /// and restricting deletion of booked venues/events.
    /// </summary>
    public class BookingValidationService
    {
        private readonly EventEaseDbContext _context;

        public BookingValidationService(EventEaseDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Validates that a venue is not double-booked for the same date and time.
        /// </summary>
        public async Task<ValidationResult> ValidateVenueAvailability(int venueId, DateTime eventDate)
        {
            // Check if venue already has an event scheduled for this date
            var existingEvent = await _context.Events
                .Where(e => e.VenueId == venueId && 
                           e.EventDate.Date == eventDate.Date)
                .FirstOrDefaultAsync();

            if (existingEvent != null)
            {
                return ValidationResult.Failure(
                    "Venue is already booked for this date",
                    $"Venue already has event '{existingEvent.EventName}' scheduled for {existingEvent.EventDate:yyyy-MM-dd}")
                ;
            }

            return ValidationResult.Success("Venue is available");
        }

        /// <summary>
        /// Validates that a venue can be deleted (no active bookings).
        /// </summary>
        public async Task<ValidationResult> CanDeleteVenue(int venueId)
        {
            var hasActiveBookings = await _context.Events
                .Where(e => e.VenueId == venueId)
                .AnyAsync();

            if (hasActiveBookings)
            {
                return ValidationResult.Failure(
                    "Cannot delete venue",
                    "This venue has associated events with active bookings. Please delete all events first."
                );
            }

            return ValidationResult.Success("Venue can be deleted");
        }

        /// <summary>
        /// Validates that an event can be deleted (no active bookings).
        /// </summary>
        public async Task<ValidationResult> CanDeleteEvent(int eventId)
        {
            var hasBookings = await _context.Bookings
                .Where(b => b.EventId == eventId)
                .AnyAsync();

            if (hasBookings)
            {
                return ValidationResult.Failure(
                    "Cannot delete event",
                    "This event has active bookings. Please delete all bookings first or contact support."
                );
            }

            return ValidationResult.Success("Event can be deleted");
        }

        /// <summary>
        /// Validates venue required fields to prevent crashes on incomplete data.
        /// </summary>
        public ValidationResult ValidateVenue(Venue venue)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(venue.VenueName))
                errors.Add("Venue name is required");

            if (venue.Capacity <= 0)
                errors.Add("Capacity must be greater than 0");

            if (string.IsNullOrWhiteSpace(venue.Location))
                errors.Add("Location is required");

            if (errors.Any())
                return ValidationResult.Failure("Venue validation failed", errors.ToArray());

            return ValidationResult.Success("Venue is valid");
        }

        /// <summary>
        /// Validates event required fields to prevent crashes on incomplete data.
        /// </summary>
        public ValidationResult ValidateEvent(Event evt)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(evt.EventName))
                errors.Add("Event name is required");

            if (evt.EventDate == default(DateTime))
                errors.Add("Event date is required");

            if (evt.VenueId <= 0)
                errors.Add("Venue selection is required");

            if (errors.Any())
                return ValidationResult.Failure("Event validation failed", errors.ToArray());

            return ValidationResult.Success("Event is valid");
        }

        /// <summary>
        /// Validates booking required fields to prevent crashes on incomplete data.
        /// </summary>
        public ValidationResult ValidateBooking(Booking booking)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(booking.CustomerName))
                errors.Add("Customer name is required");

            if (string.IsNullOrWhiteSpace(booking.CustomerEmail))
                errors.Add("Customer email is required");
            else if (!booking.CustomerEmail.Contains("@"))
                errors.Add("Customer email format is invalid");

            if (booking.NumberOfTickets <= 0)
                errors.Add("Number of tickets must be greater than 0");

            if (booking.EventId <= 0)
                errors.Add("Event selection is required");

            if (errors.Any())
                return ValidationResult.Failure("Booking validation failed", errors.ToArray());

            return ValidationResult.Success("Booking is valid");
        }
    }
}
