using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Pages.Bookings
{
    public class DetailsModel : PageModel
    {
        private readonly EventEaseDbContext _context;

        public DetailsModel(EventEaseDbContext context)
        {
            _context = context;
        }

        public Booking Booking { get; set; } = new Booking();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var b = await _context.Bookings.Include(bk => bk.Event).FirstOrDefaultAsync(bk => bk.BookingId == id);
            if (b == null) return NotFound();

            Booking = b;
            return Page();
        }
    }
}