using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using EmployeeApp.Data;

namespace EmployeeApp.Pages.Calendar
{
    public class UserModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public void OnGet()
        {
        }

        public async Task<JsonResult> OnGetEventsAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            var leaves = await _context.Leaves
                .Where(l => l.UserId == user.Id && l.Status != "Rejected")
                .ToListAsync();

            var events = new List<object>();

            foreach (var leave in leaves)
            {
                var currentDate = leave.From.Date;
                while (currentDate <= leave.To.Date)
                {
                    events.Add(new
                    {
                        title = leave.LeaveType,
                        start = currentDate.ToString("yyyy-MM-dd"),
                        type = "Leave"
                    });
                    currentDate = currentDate.AddDays(1);
                }
            }

            int currentYear = DateTime.Now.Year;
            var holidays = new List<object>
            {
                new { title = "Diwali Holiday", start = new DateTime(currentYear, 10, 29).ToString("yyyy-MM-dd"), type = "Holiday" },
                new { title = "New Year Holiday", start = new DateTime(currentYear, 1, 1).ToString("yyyy-MM-dd"), type = "Holiday" }
            };


            events.AddRange(holidays);

            return new JsonResult(events);
        }
    }
}

