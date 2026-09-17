using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;

namespace EventEase.Pages.Bookings
{
    public class CreateModel : PageModel
    {
        private readonly EventEaseDbContext _context;
        private readonly BookingValidationService _validationService;

        public CreateModel(EventEaseDbContext context, BookingValidationService validationService)
        {
            _context = context;
            _validationService = validationService;
        }

        [BindProperty]
        public Booking Booking { get; set; } = new Booking();

        public SelectList EventList { get; set; } = null!;
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync(int? eventId)
        {
            await LoadEventsAsync();

            // Pre-select the event if eventId is provided
            if (eventId.HasValue)
            {
                Booking.EventId = eventId.Value;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validate booking data
            var validationResult = _validationService.ValidateBooking(Booking);
            if (!validationResult.IsSuccess)
            {
                ErrorMessage = string.Join(" ", validationResult.Errors);
                ModelState.AddModelError("", validationResult.Message);
                await LoadEventsAsync();
                return Page();
            }

            if (!ModelState.IsValid)
            {
                await LoadEventsAsync();
                return Page();
            }

            try
            {
                _context.Bookings.Add(Booking);
                await _context.SaveChangesAsync();
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while saving the booking. Please try again.";
                ModelState.AddModelError("", ErrorMessage);
                await LoadEventsAsync();
                return Page();
            }
        }

        private async Task LoadEventsAsync()
        {
            EventList = new SelectList(await _context.Events.OrderBy(e => e.EventName).ToListAsync(), "EventId", "EventName");
        }
    }
}
