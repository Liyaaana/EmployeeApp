using EmployeeApp.Data;
using EmployeeApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace EmployeeApp.Pages.EmployeeManagement
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Used to populate the Department dropdown
        public List<SelectListItem> Department { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Finance", Text = "Finance" },
            new SelectListItem { Value = "HR", Text = "HR" },
            new SelectListItem { Value = "IT", Text = "IT" },
            new SelectListItem { Value = "Marketing", Text = "Marketing" },
            new SelectListItem { Value = "Sales", Text = "Sales" }
        };

        // Input model for form binding and validation
        [BindProperty]
        public EmployeeInputModel Employee { get; set; }

        public void OnGet()
        {
            // Nothing to do for now, Departments are already initialized
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var newEmployee = new Employee
            {
                Name = Employee.Name,
                Email = Employee.Email,
                PhoneNumber = Employee.PhoneNumber,
                Department = Employee.Department,
                Salary = Employee.Salary
            };

            _context.Employees.Add(newEmployee);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
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
