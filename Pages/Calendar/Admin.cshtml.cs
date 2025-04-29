using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeApp.Models;
using Microsoft.EntityFrameworkCore;
using EmployeeApp.Data;

namespace EmployeeApp.Pages.Calendar
{
    public class AdminModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AdminModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
        }

        public async Task<JsonResult> OnGetEventsAsync()
        {
            var leaves = await _context.Leaves
                .Include(l => l.User)
                .Where(l => l.Status != "Rejected")
                .ToListAsync();

            var events = new List<object>();

            foreach (var leave in leaves)
            {
                if (leave.User != null)
                {
                    var currentDate = leave.From.Date;
                    while (currentDate <= leave.To.Date)
                    {
                        events.Add(new
                        {
                            title = $"{leave.User.FullName} - {leave.LeaveType}",
                            start = currentDate.ToString("yyyy-MM-dd"),
                            type = "Leave"
                        });
                        currentDate = currentDate.AddDays(1);
                    }
                }
            }

            int currentYear = DateTime.Now.Year;
            var holidays = new List<object>
            {
                new { title = "May1 Holiday", start = new DateTime(currentYear, 5, 1).ToString("yyyy-MM-dd"), type = "Holiday" },
                new { title = "Diwali Holiday", start = new DateTime(currentYear, 10, 29).ToString("yyyy-MM-dd"), type = "Holiday" },
                new { title = "Christmas Holiday", start = new DateTime(currentYear, 12, 25).ToString("yyyy-MM-dd"), type = "Holiday" },
                new { title = "New Year Holiday", start = new DateTime(currentYear, 1, 1).ToString("yyyy-MM-dd"), type = "Holiday" }
            };

            events.AddRange(holidays);

            return new JsonResult(events);
        }

    }
}
