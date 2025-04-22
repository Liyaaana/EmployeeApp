using EmployeeApp.Data;
using EmployeeApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeApp.Pages.EmployeeManagement
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 5;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Employee> Employees { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        // Pagination + Listing
        public async Task OnGetAsync([FromQuery] int? page)
        {
            CurrentPage = page ?? 1;
            if (CurrentPage < 1) CurrentPage = 1;

            int totalEmployees = await _context.Employees.CountAsync();
            TotalPages = (int)System.Math.Ceiling(totalEmployees / (double)PageSize);

            if (CurrentPage > TotalPages && TotalPages > 0)
                CurrentPage = TotalPages;

            Employees = await _context.Employees
                .OrderBy(e => e.Id)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        // Delete employee by AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return new JsonResult(new { success = false, message = "Employee not found" });
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        // AJAX Search handler
        public async Task<IActionResult> OnGetSearchAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 3)
            {
                return new JsonResult(new List<object>());
            }

            var matchedEmployees = await _context.Employees
                .Where(e => e.Name.ToLower().Contains(term.ToLower()))
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

            return new JsonResult(matchedEmployees);
        }
    }
}
