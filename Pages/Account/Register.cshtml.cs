using EmployeeApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using EmployeeApp.Services;

namespace EmployeeApp.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly EmployeeService _employeeService;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            EmployeeService employeeService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _employeeService = employeeService;
        }

        [BindProperty]
        public RegisterInput Input { get; set; }

        public void OnGet() { }

        public async Task<JsonResult> OnPostRegisterAsync(string fullName, string email, string password, string department)
        {
            // Check if the admin is logged in, otherwise deny registration
            var currentUser = await _userManager.GetUserAsync(User);
            if (!await _userManager.IsInRoleAsync(currentUser, "Admin"))
                return new JsonResult(new { success = false, error = "Only admin can create employees." });

            if (await _userManager.FindByEmailAsync(email) != null)
                return new JsonResult(new { success = false, error = "An account with this email already exists." });

            if (string.IsNullOrWhiteSpace(fullName) || fullName.Length < 2)
                return new JsonResult(new { success = false, error = "Full Name must be at least 2 characters long." });

            if (!new EmailAddressAttribute().IsValid(email))
                return new JsonResult(new { success = false, error = "Invalid email format." });

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                Department = department,
                EmployeeCode = _employeeService.GenerateEmployeeCode(fullName)
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return new JsonResult(new { success = false, error = errors });
            }

            // Assign default role for the employee
            await _userManager.AddToRoleAsync(user, "User");

            // Log in the new user if needed
            // await _signInManager.SignInAsync(user, isPersistent: false);

            return new JsonResult(new { success = true });
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
