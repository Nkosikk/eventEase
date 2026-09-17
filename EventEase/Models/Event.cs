using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required]
        public string EventName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        // Foreign key to Venue
        [Range(1, int.MaxValue)]
        public int VenueId { get; set; }
        public Venue? Venue { get; set; }

        // Navigation
        public List<Booking>? Bookings { get; set; }
    }
}