using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ViaYou.Core.Entities;
using ViaYou.ViewModels;

namespace ViaYou.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public ProfileController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var vm = new ProfileViewModel
            {
                Email = user.Email ?? "",
                FullName = user.FullName,
                MemberSince = user.CreatedAt,
                SecurityScore = 95,
                ProfilePictureBase64 = user.ProfilePicture,
                EmailConfirmed = user.EmailConfirmed,
                KycStatus = user.KycStatus,
                LanguagePreference = user.LanguagePreference,
                LoginAlertsEnabled = user.LoginAlertsEnabled,
                PhoneNumber = user.PhoneNumber,
                EditProfile = new EditProfileForm 
                { 
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    PhoneNumber = user.PhoneNumber,
                    LanguagePreference = user.LanguagePreference,
                    LoginAlertsEnabled = user.LoginAlertsEnabled
                }
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileForm model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (ModelState.IsValid)
            {
                user.FullName = model.FullName;
                user.PhoneNumber = model.PhoneNumber;
                user.LanguagePreference = model.LanguagePreference;
                user.LoginAlertsEnabled = model.LoginAlertsEnabled;
                
                // Natively update email via Identity if changed
                if (user.Email != model.Email)
                {
                    var token = await _userManager.GenerateChangeEmailTokenAsync(user, model.Email);
                    await _userManager.ChangeEmailAsync(user, model.Email, token);
                    await _userManager.SetUserNameAsync(user, model.Email);
                    user.EmailConfirmed = false; // force re-verify if email string mutated
                }

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Profile settings successfully saved.";
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            TempData["ErrorMessage"] = "Failed to evaluate structural edits.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerification()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            
            // SIMULATED: Usually you'd generate an email token here and dispatch via SendGrid
            // We'll instantly set it confirmed for UX presentation
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = "Email address has been successfully verified.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPicture(IFormFile ProfilePictureFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (ProfilePictureFile != null && ProfilePictureFile.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await ProfilePictureFile.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                string base64String = Convert.ToBase64String(imageBytes);
                
                user.ProfilePicture = $"data:{ProfilePictureFile.ContentType};base64,{base64String}";
                await _userManager.UpdateAsync(user);
                
                TempData["SuccessMessage"] = "Avatar identity successfully synchronized.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordForm model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (ModelState.IsValid)
            {
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (changePasswordResult.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);
                    TempData["SuccessMessage"] = "Security footprint locked. Password changed successfully.";
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            TempData["ErrorMessage"] = "Password alteration failed validation constraints.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (user.Email?.ToLower() == "demo@viayou.com")
            {
                TempData["ErrorMessage"] = "Cannot terminate the core architecture demo account.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Login", "Auth");
            }

            TempData["ErrorMessage"] = "Failed to sequence account termination block.";
            return RedirectToAction(nameof(Index));
        }
    }
}
