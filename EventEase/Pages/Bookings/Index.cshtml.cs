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

        [BindProperty(SupportsGet = true)]
        public int? EventTypeId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? IsAvailable { get; set; }

        public IList<EventType> EventTypes { get; set; } = new List<EventType>();

        public async Task OnGetAsync()
        {
            EventTypes = await _context.EventTypes
                .AsNoTracking()
                .OrderBy(et => et.EventTypeName)
                .ToListAsync();

            var query = _context.Bookings.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                if (SearchType == "bookingid")
                {
                    if (int.TryParse(SearchTerm, out int bookingId))
                    {
                        query = query.Where(b => b.BookingId == bookingId);
                    }
                }
                else if (SearchType == "eventname")
                {
                    query = query.Where(b => b.Event!.EventName.Contains(SearchTerm));
                }
                else
                {
                    query = query.Where(b =>
                        b.BookingId.ToString().Contains(SearchTerm) ||
                        b.CustomerName.Contains(SearchTerm) ||
                        b.CustomerEmail.Contains(SearchTerm) ||
                        b.Event!.EventName.Contains(SearchTerm));
                }
            }

            if (EventTypeId.HasValue)
            {
                query = query.Where(b => b.Event!.Venue!.EventTypeId == EventTypeId.Value);
            }

            if (StartDate.HasValue)
            {
                query = query.Where(b => b.Event!.EventDate >= StartDate.Value.Date);
            }

            if (EndDate.HasValue)
            {
                var exclusiveEndDate = EndDate.Value.Date.AddDays(1);
                query = query.Where(b => b.Event!.EventDate < exclusiveEndDate);
            }

            if (StartDate.HasValue && EndDate.HasValue && StartDate.Value.Date > EndDate.Value.Date)
            {
                ModelState.AddModelError(string.Empty, "The start date cannot be later than the end date.");
                query = query.Where(_ => false);
            }

            if (IsAvailable.HasValue)
            {
                query = query.Where(b => b.Event!.Venue!.IsAvailable == IsAvailable.Value);
            }

            Bookings = await query
                .OrderByDescending(b => b.BookingDate)
                .Select(b => new BookingDisplayView
                {
                    BookingId = b.BookingId,
                    CustomerName = b.CustomerName,
                    CustomerEmail = b.CustomerEmail,
                    BookingDate = b.BookingDate,
                    NumberOfTickets = b.NumberOfTickets,
                    EventId = b.Event!.EventId,
                    EventName = b.Event.EventName,
                    EventDate = b.Event.EventDate,
                    EventDescription = b.Event.Description,
                    VenueId = b.Event.Venue!.VenueId,
                    VenueName = b.Event.Venue.VenueName,
                    Location = b.Event.Venue.Location,
                    Capacity = b.Event.Venue.Capacity,
                    EventTypeId = b.Event.Venue.EventTypeId,
                    EventTypeName = b.Event.Venue.EventType == null
                        ? "Unassigned"
                        : b.Event.Venue.EventType.EventTypeName,
                    IsAvailable = b.Event.Venue.IsAvailable
                })
                .ToListAsync();
        }
    }
}
