using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Customer name must be between 2 and 100 characters")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string CustomerEmail { get; set; } = string.Empty;

        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        [Range(1, int.MaxValue, ErrorMessage = "Number of tickets must be at least 1")]
        public int NumberOfTickets { get; set; }

        [Range(1, int.MaxValue)]
        public int EventId { get; set; }
        public Event? Event { get; set; }
    }
}
