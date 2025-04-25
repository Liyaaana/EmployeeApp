using EmployeeApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using EmployeeApp.Data;
using Microsoft.AspNetCore.Authorization;

namespace EmployeeApp.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }


        [BindProperty]
        public RegisterInput Input { get; set; }

        public void OnGet() { }

        public async Task<JsonResult> OnPostRegisterAsync(string fullName, string email, string password, string department)
        {
            if (await _userManager.FindByEmailAsync(email) != null)
                return new JsonResult(new { success = false, error = "An account with this email already exists." });

            if (string.IsNullOrWhiteSpace(fullName) || fullName.Length < 2)
                return new JsonResult(new { success = false, error = "Full Name must be at least 2 characters long." });

            if (!new EmailAddressAttribute().IsValid(email))
                return new JsonResult(new { success = false, error = "Invalid email format." });

            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^A-Za-z\d\s]).{6,}$"))
                return new JsonResult(new { success = false, error = "Password must include at least one uppercase, one lowercase, one number, and one special character (excluding spaces)." });

            bool isFirstUser = !_userManager.Users.Any();

            // 🔍 Find the existing employee
            var existingEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
            if (existingEmployee == null)
            {
                return new JsonResult(new { success = false, error = "No employee found with this email. Please add employee first." });
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                Department = department,
                EmployeeCode = existingEmployee.EmployeeCode,
                EmployeeId = existingEmployee.Id // Link ApplicationUser to Employee
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return new JsonResult(new { success = false, error = errors });
            }

            if (!await _roleManager.RoleExistsAsync("Admin"))
                await _roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new IdentityRole("User"));

            if (isFirstUser)
                await _userManager.AddToRoleAsync(user, "Admin");
            else
                await _userManager.AddToRoleAsync(user, "User");

            await _signInManager.SignInAsync(user, isPersistent: false);

            // Fetch the role after it's been assigned
            return new JsonResult(new { success = true, message = "User registered successfully." });
        }


        public class RegisterInput
        {
            [Required(ErrorMessage = "Full Name is required")]
            [RegularExpression(@"^(?! )[A-Za-z]{2,}(?: [A-Za-z]+)*$", ErrorMessage = "Full Name must contain at least 2 letters, only alphabets, and a single space between words.")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email format")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Password is required")]
            [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
            [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^A-Za-z\d\s]).{6,}$",
                ErrorMessage = "Password must be at least 6 characters long, have at least one uppercase letter, one lowercase letter, one number, and one special character excluding space.")]
            public string Password { get; set; }
        }
    }
}
