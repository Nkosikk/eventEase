using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;

namespace EventEase.Pages.Venues
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
            // Validate venue data
            var validationResult = _validationService.ValidateVenue(Venue);
            if (!validationResult.IsSuccess)
            {
                ErrorMessage = string.Join(" ", validationResult.Errors);
                ModelState.AddModelError("", validationResult.Message);
                return Page();
            }

            if (!ModelState.IsValid) return Page();

            try
            {
                _context.Attach(Venue).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while updating the venue. Please try again.";
                ModelState.AddModelError("", ErrorMessage);
                return Page();
            }
        }
    }
}
