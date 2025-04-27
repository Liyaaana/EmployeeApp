using EmployeeApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;

namespace EmployeeApp.Pages.Profile
{
    [Authorize(Roles = "User")]
    public class UserModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public UserModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public UserViewModel Profile { get; set; }

        public IActionResult OnGet()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return NotFound();

            Profile = new UserViewModel
            {
                FullName = user.FullName,
                EmployeeCode = user.EmployeeCode,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Department = user.Department,
                Salary = user.Salary
            };

            return Page();
        }

        public class UserViewModel
        {
            public string FullName { get; set; }
            public string EmployeeCode { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string Department { get; set; }
            public decimal Salary { get; set; }
        }
    }
}
