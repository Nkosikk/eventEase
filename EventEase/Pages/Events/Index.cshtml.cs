using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Pages.Events
{
    public class IndexModel : PageModel
    {
        private readonly EventEaseDbContext _context;

        public IndexModel(EventEaseDbContext context)
        {
            _context = context;
        }

        public IList<Event> Events { get; set; } = new List<Event>();

        public async Task OnGetAsync()
        {
            Events = await _context.Events.Include(e => e.Venue).ToListAsync();
        }
    }
}