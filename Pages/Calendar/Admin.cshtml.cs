using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EmployeeApp.Data;
using EmployeeApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeApp.Pages.Calendar
{
    [Authorize(Roles = "Admin")]
    public class AdminModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AdminModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<CalendarEvent> Events { get; set; }

        public async Task OnGetAsync()
        {
            var leaves = await _context.Leaves.Include(l => l.User).ToListAsync();
            Events = leaves.Select(l => new CalendarEvent
            {
                Title = $"{l.User.FullName} - {l.LeaveType}",
                Date = l.From,
                Type = "Leave",
                EmployeeName = l.User.FullName
            }).ToList();

            var holidays = new List<CalendarEvent>
            {
                new CalendarEvent { Title = "New Year's Day", Date = new DateTime(DateTime.Now.Year, 1, 1), Type = "Holiday" },
                new CalendarEvent { Title = "Independence Day", Date = new DateTime(DateTime.Now.Year, 8, 15), Type = "Holiday" },
                new CalendarEvent { Title = "Diwali", Date = new DateTime(DateTime.Now.Year, 11, 1), Type = "Holiday" },
                new CalendarEvent { Title = "Christmas", Date = new DateTime(DateTime.Now.Year, 12, 25), Type = "Holiday" }

            };

            Events.AddRange(holidays);
        }
    }
}
