using EmployeeApp.Models;
using EmployeeApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace EmployeeApp.Pages.Leaves
{
    public class AdminModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AdminModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<LeaveViewModel> Leaves { get; set; }

        public void OnGet()
        {
            Leaves = _context.Leaves
                .Select(l => new LeaveViewModel
                {
                    Id = l.Id,
                    EmployeeId = l.User.EmployeeCode,
                    EmployeeName = l.User.FullName,
                    LeaveType = l.LeaveType,
                    From = l.From,
                    To = l.To,
                    AppliedDate = l.AppliedDate,
                    Days = (int)(l.To - l.From).TotalDays + 1,
                    Status = l.Status,
                    Description = l.Description,
                    Department = l.Department
                })
                .ToList();
        }

        public IActionResult OnGetLeaveDetails(int id)
        {
            var leave = _context.Leaves
                .Where(l => l.Id == id)
                .Select(l => new LeaveViewModel
                {
                    Id = l.Id,
                    EmployeeId = l.User.EmployeeCode,
                    EmployeeName = l.User.FullName,
                    LeaveType = l.LeaveType,
                    From = l.From,
                    To = l.To,
                    Description = l.Description,
                    Department = l.Department,
                    Status = l.Status,
                    Days = (int)(l.To - l.From).TotalDays + 1
                })
                .FirstOrDefault();

            return Partial("_LeaveDetailsPartial", leave);
        }

        public IActionResult OnPostUpdateStatus(int id, string status)
        {
            var leave = _context.Leaves.FirstOrDefault(l => l.Id == id);
            if (leave != null)
            {
                leave.Status = status;
                _context.SaveChanges();
            }

            return RedirectToPage("Admin");
        }

    }
}
