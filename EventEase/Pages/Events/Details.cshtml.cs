using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Pages.Events
{
    public class DetailsModel : PageModel
    {
        private readonly EventEaseDbContext _context;

        public DetailsModel(EventEaseDbContext context)
        {
            _context = context;
        }

        public Event Event { get; set; } = new Event();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Bookings)
                .FirstOrDefaultAsync(e => e.EventId == id);
            if (ev == null) return NotFound();

            Event = ev;
            return Page();
        }
    }
}