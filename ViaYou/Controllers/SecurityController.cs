using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViaYou.Data;
using ViaYou.Core.Entities;
using ViaYou.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ViaYou.Controllers
{
    [Authorize]
    public class SecurityController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public SecurityController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var allHistory = await _context.LoginHistory
                .Where(h => h.UserId == user.Id)
                .OrderByDescending(h => h.LoginTime)
                .ToListAsync();

            var activeSessions = allHistory.Where(h => !h.IsRevoked).ToList();
            var trustedDevices = allHistory.Where(h => h.IsTrusted && !h.IsRevoked).ToList();
            var alerts = allHistory.Where(h => !h.IsTrusted && !h.IsRevoked).ToList();

            int score = 0;
            if (user.TwoFactorEnabled) score += 25;
            
            bool isPasswordHealthy = (DateTime.UtcNow - user.LastPasswordChangedDate).TotalDays < 90;
            if (isPasswordHealthy) score += 15;
            
            bool hasSafeSessionsCount = activeSessions.Count < 3;
            if (hasSafeSessionsCount) score += 10;
            
            bool hasNoSuspiciousDevices = alerts.Count == 0;
            if (hasNoSuspiciousDevices) score += 15;
            
            if (user.LoginAlertsEnabled) score += 10;
            if (user.TransactionLimitsSet) score += 10;
            
            bool recoveryEmailConfigured = !string.IsNullOrEmpty(user.RecoveryEmail);
            if (recoveryEmailConfigured) score += 15;

            var vm = new SecurityHubViewModel
            {
                TotalScore = score,
                Is2FAEnabled = user.TwoFactorEnabled,
                IsPasswordHealthy = isPasswordHealthy,
                HasSafeSessionsCount = hasSafeSessionsCount,
                HasNoSuspiciousDevices = hasNoSuspiciousDevices,
                LoginAlertsEnabled = user.LoginAlertsEnabled,
                TransactionLimitsSet = user.TransactionLimitsSet,
                RecoveryEmailConfigured = recoveryEmailConfigured,
                ActiveSessions = activeSessions,
                TrustedDevices = trustedDevices,
                Alerts = alerts
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleSetting([FromBody] ToggleRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, error = "User not found" });

            switch(request.Setting)
            {
                case "TwoFactor":
                    var res2FA = await _userManager.SetTwoFactorEnabledAsync(user, request.Enable);
                    if (!res2FA.Succeeded) return Json(new { success = false, error = "Failed to update 2FA" });
                    break;
                case "LoginAlerts": 
                    user.LoginAlertsEnabled = request.Enable; 
                    break;
                case "TransactionLimits": 
                    user.TransactionLimitsSet = request.Enable; 
                    break;
                case "RecoveryEmail": 
                    user.RecoveryEmail = request.Enable ? "recovery@example.com" : null; 
                    break;
            }

            var res = await _userManager.UpdateAsync(user);
            return Json(new { success = res.Succeeded });
        }

        [HttpPost]
        public async Task<IActionResult> RevokeSession([FromBody] RevokeRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false });

            var history = await _context.LoginHistory.FirstOrDefaultAsync(h => h.Id == request.HistoryId && h.UserId == user.Id);
            if (history != null)
            {
                history.IsRevoked = true;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        public class ToggleRequest 
        {
            public string Setting { get; set; } = string.Empty;
            public bool Enable { get; set; }
        }

        public class RevokeRequest
        {
            public int HistoryId { get; set; }
        }
    }
}
