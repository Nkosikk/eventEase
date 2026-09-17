using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;

namespace EventEase.Pages.Events
{
    public class DeleteModel : PageModel
    {
        private readonly EventEaseDbContext _context;
        private readonly BookingValidationService _validationService;

        public DeleteModel(EventEaseDbContext context, BookingValidationService validationService)
        {
            _context = context;
            _validationService = validationService;
        }

        [BindProperty]
        public Event Event { get; set; } = new Event();

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            Event = ev;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var ev = await _context.Events.FindAsync(Event.EventId);
            if (ev == null) return NotFound();

            // Check if event can be deleted (no active bookings)
            var canDeleteResult = await _validationService.CanDeleteEvent(ev.EventId);
            if (!canDeleteResult.IsSuccess)
            {
                ErrorMessage = string.Join(" ", canDeleteResult.Errors);
                ModelState.AddModelError("", canDeleteResult.Message);
                Event = ev;
                return Page();
            }

            try
            {
                _context.Events.Remove(ev);
                await _context.SaveChangesAsync();
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while deleting the event. Please try again.";
                ModelState.AddModelError("", ErrorMessage);
                Event = ev;
                return Page();
            }
        }
    }
}