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

        public class LeaveInputModel : IValidatableObject
        {
            [Required(ErrorMessage = "Leave type is required")]
            public string LeaveType { get; set; }

            [Required(ErrorMessage = "Start date is required")]
            [DataType(DataType.Date)]
            [CustomFromDate(ErrorMessage = "Start date cannot be in the past.")]
            public DateTime From { get; set; }

            [Required(ErrorMessage = "End date is required")]
            [DataType(DataType.Date)]
            public DateTime To { get; set; }

            [Required(ErrorMessage = "Description is required")]
            public string Description { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (To < From)
                {
                    yield return new ValidationResult("End date cannot be earlier than start date.", new[] { nameof(To) });
                }
            }
        }

        public class CustomFromDateAttribute : ValidationAttribute
        {
            public override bool IsValid(object value)
            {
                if (value is DateTime date)
                {
                    return date.Date >= DateTime.Today;
                }
                return false;
            }
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

            // Log the model state errors
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine("Model error: " + error.ErrorMessage); // Debug the error message
            }

            // Additional server-side validation
            if (Leave.To < Leave.From)
            {
                ModelState.AddModelError("Leave.To", "To date cannot be earlier than From date.");
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
                Department = user.Department,
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
