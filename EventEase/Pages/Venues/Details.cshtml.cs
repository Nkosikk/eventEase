using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Pages.Venues
{
    public class DetailsModel : PageModel
    {
        private readonly EventEaseDbContext _context;

        public DetailsModel(EventEaseDbContext context)
        {
            _context = context;
        }

        public Venue Venue { get; set; } = new Venue();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound();

            Venue = venue;
            return Page();
        }
    }
}