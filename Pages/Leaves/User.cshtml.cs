using System.ComponentModel.DataAnnotations;
using EmployeeApp.Data;
using EmployeeApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EmployeeApp.Pages.Leaves
{
    public class UserModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<LeaveViewModel> Leaves { get; set; } = new();

        [BindProperty]
        public LeaveInputModel Leave { get; set; }

        public class LeaveInputModel
        {
            [Required(ErrorMessage = "Leave type is required")]
            public string LeaveType { get; set; }

            [Required(ErrorMessage = "Start date is required")]
            public DateTime From { get; set; }

            [Required(ErrorMessage = "End date is required")]
            public DateTime To { get; set; }

            [Required(ErrorMessage = "Description is required")]
            public string Description { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToPage("/Account/Login");

            Leaves = await _context.Leaves
                .Where(l => l.UserId == currentUser.Id)
                .OrderByDescending(l => l.AppliedDate)
                .Select(l => new LeaveViewModel
                {
                    Id = l.Id,
                    LeaveType = l.LeaveType,
                    From = l.From,
                    To = l.To,
                    Status = l.Status,
                    Description = l.Description,
                    AppliedDate = l.AppliedDate
                })
                .ToListAsync();

            return Page();
        }

        public PartialViewResult OnGetAddForm()
        {
            return new PartialViewResult
            {
                ViewName = "_AddLeavePartial",
                ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<LeaveInputModel>(ViewData, new LeaveInputModel())
            };
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostSubmitLeaveAsync()
        {
            if (!ModelState.IsValid)
            {
                return new PartialViewResult
                {
                    ViewName = "_AddLeavePartial",
                    ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<LeaveInputModel>(ViewData, Leave)
                };
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var newLeave = new Leave
            {
                UserId = user.Id,
                LeaveType = Leave.LeaveType,
                From = Leave.From,
                To = Leave.To,
                Description = Leave.Description,
                Status = "Pending",
                AppliedDate = DateTime.Now
            };

            _context.Leaves.Add(newLeave);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        public async Task<PartialViewResult> OnGetTablePartial()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var leaves = await _context.Leaves
                .Where(l => l.UserId == currentUser.Id)
                .OrderByDescending(l => l.AppliedDate)
                .Select(l => new LeaveViewModel
                {
                    Id = l.Id,
                    LeaveType = l.LeaveType,
                    From = l.From,
                    To = l.To,
                    Status = l.Status,
                    Description = l.Description,
                    AppliedDate = l.AppliedDate
                })
                .ToListAsync();

            return new PartialViewResult
            {
                ViewName = "_LeaveTablePartial",
                ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<List<LeaveViewModel>>(ViewData, leaves)
            };
        }

    }
}
