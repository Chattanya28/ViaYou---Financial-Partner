using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ViaYou.Data;
using ViaYou.Core.Entities;

namespace ViaYou.Controllers
{
    [Authorize]
    public class AnalyticsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId)) return RedirectToAction("Login", "Auth");

            var accounts = await _context.Accounts.Where(a => a.UserId == userId).Select(a => a.Id).ToListAsync();
            
            var allTransactions = await _context.Transactions
                .Where(t => accounts.Contains(t.AccountId))
                .ToListAsync();

            // 1. Current Month Telemetry
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var thisMonthTx = allTransactions.Where(t => t.Date.Month == currentMonth && t.Date.Year == currentYear).ToList();

            var monthlyIncome = thisMonthTx.Where(t => t.Type == "Credit").Sum(t => t.Amount);
            var monthlyExpense = thisMonthTx.Where(t => t.Type == "Debit").Sum(t => t.Amount);

            // 2. Category Doughnut Chart (All Time / Or specific)
            var categoryGroup = allTransactions
                .Where(t => t.Type == "Debit")
                .GroupBy(t => t.Category ?? "Other")
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

            var pieLabels = categoryGroup.Keys.ToList();
            var pieData = categoryGroup.Values.ToList();

            // 3. 6-Month Trend Matrix (Bar Chart)
            var trendLabels = new List<string>();
            var trendIncomeData = new List<decimal>();
            var trendExpenseData = new List<decimal>();

            for (int i = 5; i >= 0; i--)
            {
                var targetMonth = DateTime.Now.AddMonths(-i);
                var monthLabel = targetMonth.ToString("MMM");
                trendLabels.Add(monthLabel);
                
                var monthTx = allTransactions.Where(t => t.Date.Month == targetMonth.Month && t.Date.Year == targetMonth.Year).ToList();
                trendIncomeData.Add(monthTx.Where(t => t.Type == "Credit").Sum(t => t.Amount));
                trendExpenseData.Add(monthTx.Where(t => t.Type == "Debit").Sum(t => t.Amount));
            }

            ViewBag.MonthlyIncome = monthlyIncome;
            ViewBag.MonthlyExpense = monthlyExpense;
            ViewBag.NetYield = monthlyIncome - monthlyExpense;

            ViewBag.PieLabels = pieLabels;
            ViewBag.PieData = pieData;
            
            ViewBag.TrendLabels = trendLabels;
            ViewBag.TrendIncome = trendIncomeData;
            ViewBag.TrendExpense = trendExpenseData;

            return View();
        }
    }
}
