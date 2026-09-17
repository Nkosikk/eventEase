using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Pages.Venues
{
    public class IndexModel : PageModel
    {
        private readonly EventEaseDbContext _context;

        public IndexModel(EventEaseDbContext context)
        {
            _context = context;
        }

        public IList<Venue> Venues { get; set; } = new List<Venue>();

        public async Task OnGetAsync()
        {
            Venues = await _context.Venues.ToListAsync();
        }
    }
}