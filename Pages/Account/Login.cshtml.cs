using EmployeeApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmployeeApp.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; }

        public class LoginInputModel
        {
            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email format")]
            [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.(com|net|org|edu|gov|in|us|co\.uk|io)$",
                ErrorMessage = "Invalid email domain")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Password is required")]
            [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
            public string Password { get; set; }
        }

        public async Task<JsonResult> OnPostLoginAsync(string email, string password)
        {
            if (!ModelState.IsValid)
                return new JsonResult(new { success = false, error = "Invalid input data." });

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new JsonResult(new { success = false, error = "No account found with this email." });

            var result = await _signInManager.PasswordSignInAsync(user.UserName, password, false, false);
            if (!result.Succeeded)
                return new JsonResult(new { success = false, error = "Incorrect password." });

            //  Log login directly into the database
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("INSERT INTO EmployeeAuditLogs (EmployeeCode, PageVisited, ActionType, ActionTime) VALUES (@EmployeeCode, @PageVisited, @ActionType, @ActionTime)", connection);
                command.Parameters.AddWithValue("@EmployeeCode", user.EmployeeCode);
                command.Parameters.AddWithValue("@PageVisited", "/Account/Login");
                command.Parameters.AddWithValue("@ActionType", "Login");
                command.Parameters.AddWithValue("@ActionTime", DateTime.Now);
                await command.ExecuteNonQueryAsync();
            }

            if (user.IsFirstLogin)
                return new JsonResult(new { success = true, redirectToSettings = true });

            return new JsonResult(new { success = true });
        }

    }
}
