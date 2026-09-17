using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;

namespace EventEase.Pages.Events
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
        public Event Event { get; set; } = new Event();

        public SelectList VenueList { get; set; } = null!;
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadVenuesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validate event data
            var validationResult = _validationService.ValidateEvent(Event);
            if (!validationResult.IsSuccess)
            {
                ErrorMessage = string.Join(" ", validationResult.Errors);
                ModelState.AddModelError("", validationResult.Message);
                await LoadVenuesAsync();
                return Page();
            }

            // Check venue availability
            var availabilityResult = await _validationService.ValidateVenueAvailability(Event.VenueId, Event.EventDate);
            if (!availabilityResult.IsSuccess)
            {
                ErrorMessage = string.Join(" ", availabilityResult.Errors);
                ModelState.AddModelError("", availabilityResult.Message);
                await LoadVenuesAsync();
                return Page();
            }

            if (!ModelState.IsValid)
            {
                await LoadVenuesAsync();
                return Page();
            }

            try
            {
                // Set default ImageUrl if not provided
                if (string.IsNullOrWhiteSpace(Event.ImageUrl))
                {
                    Event.ImageUrl = "https://via.placeholder.com/300x200?text=Event";
                }

                _context.Events.Add(Event);
                await _context.SaveChangesAsync();
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while saving the event. Please try again.";
                ModelState.AddModelError("", ErrorMessage);
                await LoadVenuesAsync();
                return Page();
            }
        }

        private async Task LoadVenuesAsync()
        {
            VenueList = new SelectList(await _context.Venues.OrderBy(v => v.VenueName).ToListAsync(), "VenueId", "VenueName");
        }
    }
}