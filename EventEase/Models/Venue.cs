using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Venue
    {
        public int VenueId { get; set; }

        [Required]
        public string VenueName { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int Capacity { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        // Navigation property
        public List<Event>? Events { get; set; }
    }
}