using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Pages.Bookings
{
    public class EditModel : PageModel
    {
        private readonly EventEaseDbContext _context;

        public EditModel(EventEaseDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Booking Booking { get; set; } = new Booking();

        public SelectList EventList { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var b = await _context.Bookings.FindAsync(id);
            if (b == null) return NotFound();

            Booking = b;
            EventList = new SelectList(_context.Events, "EventId", "EventName", b.EventId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                EventList = new SelectList(await _context.Events.OrderBy(e => e.EventName).ToListAsync(), "EventId", "EventName", Booking.EventId);
                return Page();
            }

            _context.Attach(Booking).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}