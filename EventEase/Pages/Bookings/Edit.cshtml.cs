using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;

namespace EventEase.Pages.Bookings
{
    public class EditModel : PageModel
    {
        private readonly EventEaseDbContext _context;
        private readonly BookingValidationService _validationService;

        public EditModel(EventEaseDbContext context, BookingValidationService validationService)
        {
            _context = context;
            _validationService = validationService;
        }

        [BindProperty]
        public Booking Booking { get; set; } = new Booking();

        public SelectList EventList { get; set; } = null!;
        public string? ErrorMessage { get; set; }

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
            var validationResult = _validationService.ValidateBooking(Booking);
            if (!validationResult.IsSuccess)
            {
                ErrorMessage = string.Join(" ", validationResult.Errors);
                ModelState.AddModelError("", validationResult.Message);
                await LoadEventsAsync();
                return Page();
            }

            var availabilityResult = await _validationService.ValidateBookingVenueAvailability(
                Booking.EventId,
                Booking.BookingId);
            if (!availabilityResult.IsSuccess)
            {
                ErrorMessage = string.Join(" ", availabilityResult.Errors);
                ModelState.AddModelError("", availabilityResult.Message);
                await LoadEventsAsync();
                return Page();
            }

            if (!ModelState.IsValid)
            {
                await LoadEventsAsync();
                return Page();
            }

            _context.Attach(Booking).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }

        private async Task LoadEventsAsync()
        {
            EventList = new SelectList(
                await _context.Events.OrderBy(e => e.EventName).ToListAsync(),
                "EventId",
                "EventName",
                Booking.EventId);
        }
    }
}