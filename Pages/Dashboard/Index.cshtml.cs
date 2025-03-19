using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeApp.Data;
using EmployeeApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EmployeeApp.Pages.Dashboard
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Employee> Employees { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(SearchQuery))
            {
                Employees = _context.Employees.ToList();
            }
            else
            {
                Employees = _context.Employees
                    .Where(e => e.Name.StartsWith(SearchQuery))
                    .ToList();
            }

            return Page();
        }
    }
}
