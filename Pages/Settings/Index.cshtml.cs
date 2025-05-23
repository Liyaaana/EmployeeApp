﻿using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using EmployeeApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EmployeeApp.Pages.Settings
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public ChangePasswordInputModel Input { get; set; }

        public class ChangePasswordInputModel
        {
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Old Password")]
            public string OldPassword { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New Password")]
            public string NewPassword { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("User not found");

            // Check if the old password is correct
            var isCorrect = await _userManager.CheckPasswordAsync(user, Input.OldPassword);
            if (!isCorrect)
            {
                ModelState.AddModelError("Input.OldPassword", "Old password is incorrect.");
                return Page();
            }

            // Change the password
            var result = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return Page();
            }

            // Force sign out the current user
            await _signInManager.SignOutAsync();

            // Now sign in the user again with the new password
            var signInResult = await _signInManager.PasswordSignInAsync(user, Input.NewPassword, isPersistent: false, lockoutOnFailure: false);
            if (signInResult.Succeeded)
            {
                // Update the user's "first login" status
                user.IsFirstLogin = false;
                await _userManager.UpdateAsync(user);

                TempData["StatusMessage"] = "Password changed successfully.";
                return RedirectToPage("/Dashboard/Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "An error occurred while signing you in after the password change.");
                return Page();
            }
        }


        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostValidateOldPasswordAsync([FromBody] string oldPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            var isCorrect = await _userManager.CheckPasswordAsync(user, oldPassword);
            return new JsonResult(new { valid = isCorrect });
        }
    }
}
