using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EmployeeApp.Data;
using EmployeeApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeApp.Pages.Calendar
{
    [Authorize]
    public class EmployeeModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeeModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<CalendarEvent> Events { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            var leaves = await _context.Leaves
                .Where(l => l.UserId == user.Id)
                .ToListAsync();

            Events = leaves.Select(l => new CalendarEvent
            {
                Title = l.LeaveType,
                Date = l.From,
                Type = "Leave"
            }).ToList();

            var holidays = new List<CalendarEvent>
            {
                new CalendarEvent { Title = "New Year's Day", Date = new DateTime(DateTime.Now.Year, 1, 1), Type = "Holiday" },
                new CalendarEvent { Title = "Independence Day", Date = new DateTime(DateTime.Now.Year, 8, 15), Type = "Holiday" },
                new CalendarEvent { Title = "Diwali", Date = new DateTime(DateTime.Now.Year, 11, 1), Type = "Holiday" }
            };

            Events.AddRange(holidays);
        }
    }
}
