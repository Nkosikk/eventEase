using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;

namespace EventEase.Pages.Venues
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
        public Venue Venue { get; set; } = new Venue();

        public SelectList EventTypeList { get; set; } = null!;
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadEventTypesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validate venue data
            var validationResult = _validationService.ValidateVenue(Venue);
            if (!validationResult.IsSuccess)
            {
                ErrorMessage = string.Join(" ", validationResult.Errors);
                ModelState.AddModelError("", validationResult.Message);
                await LoadEventTypesAsync();
                return Page();
            }

            if (!ModelState.IsValid)
            {
                await LoadEventTypesAsync();
                return Page();
            }

            try
            {
                _context.Venues.Add(Venue);
                await _context.SaveChangesAsync();
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while saving the venue. Please try again.";
                ModelState.AddModelError("", ErrorMessage);
                await LoadEventTypesAsync();
                return Page();
            }
        }

        private async Task LoadEventTypesAsync()
        {
            EventTypeList = new SelectList(
                await _context.EventTypes.OrderBy(et => et.EventTypeName).ToListAsync(),
                "EventTypeId",
                "EventTypeName");
        }
    }
}
