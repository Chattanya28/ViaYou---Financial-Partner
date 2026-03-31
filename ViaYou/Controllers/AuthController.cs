using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViaYou.Core.Entities;
using ViaYou.Data;
using ViaYou.ViewModels;
using System.Security.Claims;  

namespace ViaYou.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid login attempt");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // Log login history
                var loginHistory = new LoginHistory
                {
                    UserId = user.Id,
                    LoginTime = DateTime.UtcNow,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    DeviceInfo = Request.Headers.UserAgent.ToString(),
                    Location = "Unknown" // Would use IP geolocation in production
                };
                _context.LoginHistory.Add(loginHistory);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Dashboard");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "Account locked out. Try again later.");
                return View(model);
            }

            ModelState.AddModelError("", "Invalid login attempt");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Create default demo data for new user
                await CreateDefaultUserData(user.Id);

                return RedirectToAction("Index", "Dashboard");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        private async Task CreateDefaultUserData(int userId)
        {
            // Create default accounts for new user
            _context.Accounts.AddRange(
                new Account
                {
                    UserId = userId,
                    BankName = "Demo Bank",
                    AccountType = "Savings",
                    Balance = 25000,
                    InterestRate = 3.5m,
                    AccountNumberMasked = "XXXX1234",
                    ColorHex = "#2F3C7E",
                    LastUpdated = DateTime.Now
                },
                new Account
                {
                    UserId = userId,
                    BankName = "Demo Salary",
                    AccountType = "Salary",
                    Balance = 50000,
                    InterestRate = 0m,
                    AccountNumberMasked = "XXXX5678",
                    ColorHex = "#0EA5E9",
                    LastUpdated = DateTime.Now
                }
            );

            // Create default goal for new user
            _context.Goals.Add(new Goal
            {
                UserId = userId,
                Name = "Emergency Fund",
                TargetAmount = 100000,
                CurrentAmount = 25000,
                TargetDate = DateTime.Now.AddYears(1),
                IconEmoji = "🛡️",
                MonthlyContribution = 5000,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
        }
    }
}