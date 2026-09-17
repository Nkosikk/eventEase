using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Pages.Bookings
{
    public class IndexModel : PageModel
    {
        private readonly EventEaseDbContext _context;

        public IndexModel(EventEaseDbContext context)
        {
            _context = context;
        }

        public IList<BookingDisplayView> Bookings { get; set; } = new List<BookingDisplayView>();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchType { get; set; } = "all";

        public async Task OnGetAsync()
        {
            // Join Bookings with Events and Venues to create the display data
            IQueryable<BookingDisplayView> query = _context.Bookings
                .Include(b => b.Event)
                .ThenInclude(e => e.Venue)
                .AsNoTracking()
                .Select(b => new BookingDisplayView
                {
                    BookingId = b.BookingId,
                    CustomerName = b.CustomerName,
                    CustomerEmail = b.CustomerEmail,
                    BookingDate = b.BookingDate,
                    NumberOfTickets = b.NumberOfTickets,
                    EventId = b.Event.EventId,
                    EventName = b.Event.EventName,
                    EventDate = b.Event.EventDate,
                    EventDescription = b.Event.Description,
                    VenueId = b.Event.Venue.VenueId,
                    VenueName = b.Event.Venue.VenueName,
                    Location = b.Event.Venue.Location,
                    Capacity = b.Event.Venue.Capacity
                });

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                if (SearchType == "bookingid")
                {
                    // Search by BookingId
                    if (int.TryParse(SearchTerm, out int bookingId))
                    {
                        query = query.Where(b => b.BookingId == bookingId);
                    }
                }
                else if (SearchType == "eventname")
                {
                    // Search by Event Name
                    query = query.Where(b => b.EventName.Contains(SearchTerm));
                }
                else
                {
                    // Default: search in all fields
                    query = query.Where(b => 
                        b.BookingId.ToString().Contains(SearchTerm) ||
                        b.CustomerName.Contains(SearchTerm) ||
                        b.CustomerEmail.Contains(SearchTerm) ||
                        b.EventName.Contains(SearchTerm));
                }
            }

            Bookings = await query.OrderByDescending(b => b.BookingDate).ToListAsync();
        }
    }
}
