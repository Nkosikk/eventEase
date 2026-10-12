using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;

namespace EventEase.Pages.Events
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
        public Event Event { get; set; } = new Event();

        public SelectList VenueList { get; set; } = null!;
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            Event = ev;
            VenueList = new SelectList(_context.Venues, "VenueId", "VenueName", ev.VenueId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validate event data
            var validationResult = _validationService.ValidateEvent(Event);
            if (!validationResult.IsSuccess)
            {
                ErrorMessage = string.Join(" ", validationResult.Errors);
                ModelState.AddModelError("", validationResult.Message);
                VenueList = new SelectList(await _context.Venues.OrderBy(v => v.VenueName).ToListAsync(), "VenueId", "VenueName", Event.VenueId);
                return Page();
            }

            var availabilityResult = await _validationService.ValidateVenueAvailability(
                Event.VenueId,
                Event.EventDate,
                Event.EventId);
            if (!availabilityResult.IsSuccess)
            {
                ErrorMessage = string.Join(" ", availabilityResult.Errors);
                ModelState.AddModelError("", availabilityResult.Message);
                VenueList = new SelectList(await _context.Venues.OrderBy(v => v.VenueName).ToListAsync(), "VenueId", "VenueName", Event.VenueId);
                return Page();
            }

            if (!ModelState.IsValid)
            {
                VenueList = new SelectList(await _context.Venues.OrderBy(v => v.VenueName).ToListAsync(), "VenueId", "VenueName", Event.VenueId);
                return Page();
            }

            try
            {
                _context.Attach(Event).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while updating the event. Please try again.";
                ModelState.AddModelError("", ErrorMessage);
                VenueList = new SelectList(await _context.Venues.OrderBy(v => v.VenueName).ToListAsync(), "VenueId", "VenueName", Event.VenueId);
                return Page();
            }
        }
    }
}