using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ViaYou.Data;

namespace ViaYou.Controllers
{
    [Authorize]
    public class TaxController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaxController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId)) return RedirectToAction("Login", "Auth");

            var accounts = await _context.Accounts.Where(a => a.UserId == userId).Select(a => a.Id).ToListAsync();
            
            var currentYear = DateTime.Now.Year;
            
            // Current FY Transactions
            var fyTransactions = await _context.Transactions
                .Where(t => accounts.Contains(t.AccountId) && t.Date.Year == currentYear)
                .ToListAsync();

            var grossIncome = fyTransactions.Where(t => t.Type == "Credit").Sum(t => t.Amount);

            // simulated 80C deductions (Insurance, Mutual Funds, ELSS)
            string[] section80CCategories = { "Insurance", "Mutual Fund", "Investment", "Tax Saving" };
            var deduction80C = fyTransactions
                .Where(t => t.Type == "Debit" && section80CCategories.Contains(t.Category))
                .Sum(t => t.Amount);

            // cap 80C at 1.5L
            var effective80C = Math.Min(deduction80C, 150000);

            // Standard Deduction (Salary) Default in India is 50,000
            var standardDeduction = grossIncome > 0 ? 50000 : 0;

            var taxableIncome = grossIncome - effective80C - standardDeduction;
            if (taxableIncome < 0) taxableIncome = 0;

            // Simple Old Regime Slab logic logic approximation
            decimal taxLiability = 0;
            if (taxableIncome > 1000000)
            {
                taxLiability += (taxableIncome - 1000000) * 0.30m;
                taxLiability += 500000 * 0.20m;
            }
            else if (taxableIncome > 500000)
            {
                taxLiability += (taxableIncome - 500000) * 0.20m;
            }
            // Add 4% cess
            taxLiability *= 1.04m;

            ViewBag.GrossIncome = grossIncome;
            ViewBag.Sec80C = deduction80C;
            ViewBag.Effective80C = effective80C;
            ViewBag.StandardDeduction = standardDeduction;
            ViewBag.TaxableIncome = taxableIncome;
            ViewBag.TaxLiability = taxLiability;

            return View();
        }
    }
}
