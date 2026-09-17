using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Pages.Bookings
{
    public class DeleteModel : PageModel
    {
        private readonly EventEaseDbContext _context;

        public DeleteModel(EventEaseDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Booking Booking { get; set; } = new Booking();

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var b = await _context.Bookings.FindAsync(id);
            if (b == null) return NotFound();

            Booking = b;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var b = await _context.Bookings.FindAsync(Booking.BookingId);
            if (b == null) return NotFound();

            try
            {
                _context.Bookings.Remove(b);
                await _context.SaveChangesAsync();
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while deleting the booking. Please try again.";
                ModelState.AddModelError("", ErrorMessage);
                Booking = b;
                return Page();
            }
        }
    }
}