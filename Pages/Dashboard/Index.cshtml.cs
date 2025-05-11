using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EmployeeApp.Data;
using EmployeeApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

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

        [BindProperty(SupportsGet = true)] // allows model binding from GET requests (query string) instead of POST requests only
        public EmployeeSearch Filter { get; set; }

        public List<Employee> Employees { get; set; } = new();
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

            CurrentPage = page ?? 1; // if page is null then page = 1
            if (CurrentPage < 1) CurrentPage = 1;

            int totalEmployees = await _context.Employees.CountAsync();
            TotalPages = (int)Math.Ceiling(totalEmployees / (double)PageSize);
            if (CurrentPage > TotalPages && TotalPages > 0)
                CurrentPage = TotalPages;

            Employees = await _context.Employees
                .OrderBy(e => e.Name)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return Page();
        }

        public async Task<JsonResult> OnGetFilterEmployees(string name, string department, decimal? minSalary, decimal? maxSalary)
        {
            IsAdmin = User.IsInRole("Admin");

            var nameParam = new SqlParameter("@SearchTerm", string.IsNullOrWhiteSpace(name) ? DBNull.Value : name);
            var deptParam = new SqlParameter("@Department", string.IsNullOrWhiteSpace(department) ? DBNull.Value : department);
            var minParam = new SqlParameter("@MinSalary", minSalary.HasValue ? minSalary : DBNull.Value);
            var maxParam = new SqlParameter("@MaxSalary", maxSalary.HasValue ? maxSalary : DBNull.Value);

            var results = await _context.EmployeeDtos
                .FromSqlRaw("EXEC SearchEmployees @SearchTerm, @Department, @MinSalary, @MaxSalary",
                    nameParam, deptParam, minParam, maxParam)
                .ToListAsync();

            return new JsonResult(new
            {
                employees = results.Select(e => new
                {
                    name = e.Name,
                    email = e.Email,
                    phoneNumber = e.PhoneNumber,
                    department = e.Department,
                    salary = e.Salary
                }),
                isAdmin = IsAdmin
            });
        }
    }
}