using Dapper;
using EmployeeApp.Data;
using EmployeeApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace EmployeeApp.Pages.Auditing
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;

        public IndexModel(IConfiguration config, ApplicationDbContext context)
        {
            _config = config;
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string EmployeeCode { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 8;

        public List<AuditLogViewModel> Logs { get; set; } = new List<AuditLogViewModel>();
        public int TotalRecords { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            var parameters = new
            {
                StartDate,
                EndDate,
                EmployeeCode = string.IsNullOrEmpty(EmployeeCode) ? null : EmployeeCode,
                PageNumber,
                PageSize
            };

            using var multi = await connection.QueryMultipleAsync(
                "GetEmployeeAuditLogs",
                parameters,
                commandType: CommandType.StoredProcedure);

            // Read total count first
            var totalCountRow = await multi.ReadFirstOrDefaultAsync<dynamic>();
            TotalRecords = totalCountRow?.TotalCount ?? 0;


            if (TotalRecords > 0)
            {
                Logs = (await multi.ReadAsync<AuditLogViewModel>()).ToList();
            }
            else
            {
                Logs = new List<AuditLogViewModel>(); // explicitly clear logs
            }

            return Page();
        }


        public JsonResult OnGetValidateEmployeeCode(string code)
        {
            bool exists = _context.Users.Any(u => u.EmployeeCode == code);
            return new JsonResult(new { isValid = exists });
        }
    }
}
