using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ViaYou.Data;
using ViaYou.Core.Entities;

namespace ViaYou.Controllers
{
    [Authorize]
    public class CompareController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CompareController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var fdPolicies = await _context.BankPolicies
                                    .Where(p => p.PolicyType == "FD")
                                    .OrderByDescending(p => p.InterestRate)
                                    .ToListAsync();

            if (!fdPolicies.Any() || fdPolicies.Count < 15)
            {
                var mockBanks = GetMockBankPolicies();
                
                // If it's completely empty, we might seed it
                if (!await _context.BankPolicies.AnyAsync(p => p.PolicyType == "FD"))
                {
                    _context.BankPolicies.AddRange(mockBanks);
                    await _context.SaveChangesAsync();
                }
                
                // Use mocks if existing db is insufficient to show the 15+ banks logic
                fdPolicies = mockBanks; 
            }

            // Ensure the list is sorted
            fdPolicies = fdPolicies.OrderByDescending(p => p.InterestRate).ToList();
            
            return View(fdPolicies);
        }

        public class CalculateFdRequest
        {
            public decimal Amount { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Calculate([FromBody] CalculateFdRequest request)
        {
            if (request == null || request.Amount < 1000 || request.Amount > 1000000)
            {
                return Json(new { error = "Please enter an amount between ₹1,000 and ₹10,00,000" });
            }

            var fdPolicies = await _context.BankPolicies
                                    .Where(p => p.PolicyType == "FD")
                                    .OrderByDescending(p => p.InterestRate)
                                    .ToListAsync();

            if (!fdPolicies.Any())
            {
                fdPolicies = GetMockBankPolicies();
            }

            var results = fdPolicies.Select(p => 
            {
                var standardSavingsRate = 3.5m;
                var standardInterest = request.Amount * (standardSavingsRate / 100);
                
                var fdInterest = request.Amount * (p.InterestRate / 100);
                var extraEarnings = fdInterest - standardInterest;
                var maturityAmount = request.Amount + fdInterest;

                return new {
                    policyId = p.Id,
                    bankName = p.BankName,
                    interestRate = p.InterestRate,
                    returns = Math.Round(fdInterest, 2),
                    maturityAmount = Math.Round(maturityAmount, 2),
                    extraEarnings = Math.Round(extraEarnings, 2)
                };
            }).ToList();

            return Json(new { success = true, data = results });
        }

        private List<BankPolicy> GetMockBankPolicies()
        {
            return new List<BankPolicy>
            {
                new BankPolicy { BankName = "AU Small Finance Bank", InterestRate = 8.5m, MinimumAmount = 1000, PolicyType = "FD", TenureDays = 365, LogoUrl = "fas fa-bank text-amber-500" },
                new BankPolicy { BankName = "Equitas Small Finance Bank", InterestRate = 8.5m, MinimumAmount = 1000, PolicyType = "FD", TenureDays = 365, LogoUrl = "fas fa-building-columns text-green-500" },
                new BankPolicy { BankName = "Ujjivan Small Finance Bank", InterestRate = 8.25m, MinimumAmount = 1000, PolicyType = "FD", TenureDays = 365, LogoUrl = "fas fa-piggy-bank text-blue-500" },
                new BankPolicy { BankName = "Jana Small Finance Bank", InterestRate = 8.25m, MinimumAmount = 1000, PolicyType = "FD", TenureDays = 365, LogoUrl = "fas fa-landmark text-red-500" },
                new BankPolicy { BankName = "Bandhan Bank", InterestRate = 7.85m, MinimumAmount = 1000, PolicyType = "FD", TenureDays = 365, LogoUrl = "fas fa-wallet text-purple-500" },
                new BankPolicy { BankName = "IndusInd Bank", InterestRate = 7.75m, MinimumAmount = 10000, PolicyType = "FD", TenureDays = 365, LogoUrl = "fas fa-money-check-dollar text-indigo-500" },
                new BankPolicy { BankName = "IDFC FIRST Bank", InterestRate = 7.75m, MinimumAmount = 10000, PolicyType = "FD", TenureDays = 365, LogoUrl = "fas fa-vault text-teal-400" },
                new BankPolicy { BankName = "Yes Bank", InterestRate = 7.75m, MinimumAmount = 10000, PolicyType = "FD", TenureDays = 365, LogoUrl = "fas fa-coins text-yellow-400" },
                new BankPolicy { BankName = "HDFC Bank", InterestRate = 7.10m, MinimumAmount = 5000, PolicyType = "FD", TenureDays = 365, LogoUrl = "fas fa-building text-blue-700" },
                new BankPolicy { BankName = "ICICI Bank", InterestRate = 7.10m, MinimumAmount = 10000, PolicyType = "FD", TenureDays = 365, LogoUrl = "fas fa-building text-orange-500" },
                new BankPolicy { BankName = "Axis Bank", InterestRate = 7.10m, MinimumAmount = 5000, PolicyType = "FD", TenureDays = 365, LogoUrl = "fas fa-building text-red-700" },
                new BankPolicy { BankName = "Kotak Mahindra Bank", InterestRate = 7.10m, MinimumAmount = 5000, PolicyType = "FD", TenureDays = 365, LogoUrl = "fas fa-landmark text-red-600" },
                new BankPolicy { BankName = "State Bank of India", InterestRate = 6.80m, MinimumAmount = 1000, PolicyType = "FD", TenureDays = 365, LogoUrl = "fas fa-university text-blue-400" },
                new BankPolicy { BankName = "Bank of Baroda", InterestRate = 6.80m, MinimumAmount = 1000, PolicyType = "FD", TenureDays = 365, LogoUrl = "fas fa-university text-orange-400" },
                new BankPolicy { BankName = "Canara Bank", InterestRate = 6.80m, MinimumAmount = 1000, PolicyType = "FD", TenureDays = 365, LogoUrl = "fas fa-university text-blue-800" },
                new BankPolicy { BankName = "Punjab National Bank", InterestRate = 6.75m, MinimumAmount = 1000, PolicyType = "FD", TenureDays = 365, LogoUrl = "fas fa-university text-orange-600" }
            };
        }
    }
}
