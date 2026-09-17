using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;

namespace EventEase.Pages.Venues
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
        public Venue Venue { get; set; } = new Venue();

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound();

            Venue = venue;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var venue = await _context.Venues.FindAsync(Venue.VenueId);
            if (venue == null) return NotFound();

            // Check if venue can be deleted (no active bookings)
            var canDeleteResult = await _validationService.CanDeleteVenue(venue.VenueId);
            if (!canDeleteResult.IsSuccess)
            {
                ErrorMessage = string.Join(" ", canDeleteResult.Errors);
                ModelState.AddModelError("", canDeleteResult.Message);
                return Page();
            }

            try
            {
                _context.Venues.Remove(venue);
                await _context.SaveChangesAsync();
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while deleting the venue. Please try again.";
                ModelState.AddModelError("", ErrorMessage);
                return Page();
            }
        }
    }
}
