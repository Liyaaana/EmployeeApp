using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EmployeeApp.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        // If user is authenticated, redirect to Dashboard
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            return RedirectToPage("/Dashboard/Index");
        }

        // Else, stay on welcome page
        return Page();
    }
}
