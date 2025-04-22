using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EmployeeApp.Data;
using EmployeeApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeApp.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 5;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Employee> Employees { get; set; } = new List<Employee>();
        public int TotalEmployees { get; set; }
        public int TotalDepartments { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public bool IsAdmin { get; set; }

        public async Task<IActionResult> OnGetAsync([FromQuery] int? page)
        {
            IsAdmin = User.IsInRole("Admin");

            TotalEmployees = await _context.Employees.CountAsync();
            TotalDepartments = await _context.Employees
                .Select(e => e.Department)
                .Distinct()
                .CountAsync();

            CurrentPage = page ?? 1;
            if (CurrentPage < 1) CurrentPage = 1;

            int totalEmployees = await _context.Employees.CountAsync();
            TotalPages = (int)Math.Ceiling(totalEmployees / (double)PageSize);
            if (CurrentPage > TotalPages && TotalPages > 0)
                CurrentPage = TotalPages;

            Employees = await _context.Employees
                .OrderBy(e => e.Id)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return Page();
        }

        public async Task<JsonResult> OnGetSearchEmployeesAsync(string query)
        {
            if (string.IsNullOrEmpty(query) || query.Length < 3)
            {
                return new JsonResult(new { isAdmin = User.IsInRole("Admin"), employees = new List<object>() });
            }

            var isAdmin = User.IsInRole("Admin");

            if (isAdmin)
            {
                var employees = await _context.Employees
                    .Where(e => e.Name.StartsWith(query))
                    .Select(e => new
                    {
                        e.Id,
                        e.Name,
                        e.Email,
                        e.PhoneNumber,
                        e.Department,
                        e.Salary
                    })
                    .ToListAsync();

                return new JsonResult(new { isAdmin = true, employees });
            }
            else
            {
                var employees = await _context.Employees
                    .Where(e => e.Name.StartsWith(query))
                    .Select(e => new
                    {
                        e.Id,
                        e.Name,
                        e.Email,
                        e.PhoneNumber,
                        e.Department
                    })
                    .ToListAsync();

                return new JsonResult(new { isAdmin = false, employees });
            }
        }
    }
}
