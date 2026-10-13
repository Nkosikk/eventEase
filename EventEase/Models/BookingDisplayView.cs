using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    /// <summary>
    /// View model representing the BookingDisplayView database view.
    /// This view consolidates booking information from Venue, Event, and Booking tables
    /// for easier management and display by booking specialists.
    /// </summary>
    [Table("BookingDisplayView", Schema = "dbo")]
    public class BookingDisplayView
    {
        public int BookingId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerEmail { get; set; } = string.Empty;

        public DateTime BookingDate { get; set; }

        public int NumberOfTickets { get; set; }

        public int EventId { get; set; }

        public string EventName { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public string EventDescription { get; set; } = string.Empty;

        public int VenueId { get; set; }

        public string VenueName { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public int? EventTypeId { get; set; }

        public string EventTypeName { get; set; } = string.Empty;

        public bool IsAvailable { get; set; }
    }
}
