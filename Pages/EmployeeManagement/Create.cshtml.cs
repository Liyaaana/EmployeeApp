using EmployeeApp.Data;
using EmployeeApp.Models;
using EmployeeApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace EmployeeApp.Pages.EmployeeManagement
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly EmployeeService _employeeService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;  // Add this line

        // Modify the constructor to accept RoleManager<IdentityRole>
        public CreateModel(ApplicationDbContext context, EmployeeService employeeService, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _employeeService = employeeService;
            _userManager = userManager;
            _roleManager = roleManager; // Assign RoleManager to the field
        }

        public List<SelectListItem> Department { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Finance", Text = "Finance" },
            new SelectListItem { Value = "HR", Text = "HR" },
            new SelectListItem { Value = "IT", Text = "IT" },
            new SelectListItem { Value = "Marketing", Text = "Marketing" },
            new SelectListItem { Value = "Sales", Text = "Sales" }
        };

        [BindProperty]
        public EmployeeInputModel Employee { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if employee with same email or phone number already exists
            var existingEmployee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == Employee.Email || e.PhoneNumber == Employee.PhoneNumber);

            if (existingEmployee != null)
            {
                TempData["EmployeeExists"] = "An employee with this email or phone number already exists.";
                return RedirectToPage(); // This will redirect to the same page to show the alert
            }

            var employeeCode = _employeeService.GenerateEmployeeCode(Employee.Name);

            var newEmployee = new Employee
            {
                Name = Employee.Name,
                Email = Employee.Email,
                PhoneNumber = Employee.PhoneNumber,
                Department = Employee.Department,
                Salary = Employee.Salary,
                EmployeeCode = employeeCode // Assign the generated EmployeeCode
            };

            try
            {
                _context.Employees.Add(newEmployee);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error saving employee: {ex.Message}");
            }

            // Generate password using the first 3 letters of the employee's name + "@123"
            string passwordPrefix = Employee.Name.Length >= 3
                ? Employee.Name.Substring(0, 3) // Use first 3 letters of name
                : Employee.Name.PadRight(3, Employee.Name[Employee.Name.Length - 1]); // Repeat last letter for names with 2 letters or less

            var password = passwordPrefix + "@123"; // Append "@123" to the prefix

            var user = new ApplicationUser
            {
                UserName = Employee.Email,
                Email = Employee.Email,
                FullName = Employee.Name,
                PhoneNumber = Employee.PhoneNumber,
                Salary = Employee.Salary,
                Department = Employee.Department,
                EmployeeId = newEmployee.Id, // Link the employee
                EmployeeCode = employeeCode
            };

            // Check if "Admin" and "User" roles exist, create them if not
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Create the ApplicationUser with the generated password
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // Check if this is the first user and assign roles accordingly
                bool isFirstUser = !_userManager.Users.Any();

                if (isFirstUser)
                {
                    // Assign the first user as Admin
                    await _userManager.AddToRoleAsync(user, "Admin");
                }
                else
                {
                    // Assign all subsequent users as User
                    await _userManager.AddToRoleAsync(user, "User");
                }

                // Add success message using TempData
                TempData["EmployeeAdded"] = "Employee successfully added!";

                // Redirect to the Index page and show success message
                return RedirectToPage("/EmployeeManagement/Index");
            }

            // If there are errors during user creation, add them to the ModelState
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        public class EmployeeInputModel
        {
            [Required(ErrorMessage = "Name is required")]
            [RegularExpression(@"^(?! )[A-Za-z]{2,}(?: [A-Za-z]+)*$", ErrorMessage = "Full Name must contain at least 2 letters, only alphabets, and a single space between words.")]
            public string Name { get; set; }

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Enter a valid email address")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Phone number is required")]
            [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be 10 digits")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "Please select a department")]
            public string Department { get; set; }

            [Required(ErrorMessage = "Salary is required")]
            [Range(10000, 1000000, ErrorMessage = "Salary must be between ₹10,000 and ₹10,00,000")]
            public int Salary { get; set; }
        }
    }
}
