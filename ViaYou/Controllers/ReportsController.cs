using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ViaYou.Data;
using ViaYou.Core.Entities;
using System.Linq;

namespace ViaYou.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId)) 
                return RedirectToAction("Login", "Auth");

            // Default bounding: 6 months backward
            var end = endDate ?? DateTime.Now;
            var start = startDate ?? DateTime.Now.AddMonths(-5).Date.AddDays(1 - DateTime.Now.Day);

            var accounts = await _context.Accounts.Where(a => a.UserId == userId).ToListAsync();
            var accountIds = accounts.Select(a => a.Id).ToList();
            
            // All transactions connected to user inside Date parameter + Future Transactions
            var allTransactions = await _context.Transactions
                .Where(t => accountIds.Contains(t.AccountId))
                .OrderBy(t => t.Date)
                .ToListAsync();

            var rangeTransactions = allTransactions
                .Where(t => t.Date >= start && t.Date <= end.AddDays(1).AddTicks(-1))
                .ToList();

            // 1. Yearly Summary Metrics (Total Income, Expenses, Savings)
            decimal totalIncome = rangeTransactions.Where(t => t.Type == "Credit").Sum(t => t.Amount);
            decimal totalExpense = rangeTransactions.Where(t => t.Type == "Debit").Sum(t => t.Amount);
            decimal netSavings = totalIncome - totalExpense;

            // 2. Net Worth Area Chart Algorithm (Backward Projection)
            var currentNetWorth = accounts.Sum(a => a.Balance);
            var netWorthLabels = new List<string>();
            var netWorthData = new List<decimal>();

            var monthsIterator = new List<DateTime>();
            var cursor = start;
            while (cursor <= end)
            {
                monthsIterator.Add(cursor);
                cursor = cursor.AddMonths(1);
            }

            foreach (var month in monthsIterator)
            {
                var endOfMonth = month.AddMonths(1).AddDays(-1);
                
                // Transactions that occurred precisely AFTER this month (up to DateTime.Now)
                var transactionsAfterMonth = allTransactions.Where(t => t.Date > endOfMonth && t.Date <= DateTime.Now);
                
                var rollbackCredits = transactionsAfterMonth.Where(t => t.Type == "Credit").Sum(t => t.Amount);
                var rollbackDebits = transactionsAfterMonth.Where(t => t.Type == "Debit").Sum(t => t.Amount);

                // If I have $1000 today, and I gained $200 (Credit) and spent $50 (Debit) AFTER last month,
                // Last month's balance was: $1000 - $200 + $50 = $850.
                var historicalNetWorth = currentNetWorth - rollbackCredits + rollbackDebits;
                
                netWorthLabels.Add(month.ToString("MMM yy"));
                netWorthData.Add(historicalNetWorth);
            }

            // 3. Income vs Expenses Bar Chart
            var incomeBarData = new List<decimal>();
            var expenseBarData = new List<decimal>();

            foreach (var month in monthsIterator)
            {
                var monthlyIncome = rangeTransactions.Where(t => t.Date.Month == month.Month && t.Date.Year == month.Year && t.Type == "Credit").Sum(t => t.Amount);
                var monthlyExpense = rangeTransactions.Where(t => t.Date.Month == month.Month && t.Date.Year == month.Year && t.Type == "Debit").Sum(t => t.Amount);
                incomeBarData.Add(monthlyIncome);
                expenseBarData.Add(monthlyExpense);
            }

            // 4. Spending Trends Line Chart (Top 3 Categories Over Time)
            var topCategories = rangeTransactions
                .Where(t => t.Type == "Debit" && t.Category != "Other")
                .GroupBy(t => t.Category ?? "Uncategorized")
                .OrderByDescending(g => g.Sum(x => x.Amount))
                .Select(g => g.Key)
                .Take(2)
                .ToList();

            var trendChartData = new Dictionary<string, List<decimal>>();
            foreach(var cat in topCategories)
            {
                var catTrend = new List<decimal>();
                foreach (var month in monthsIterator)
                {
                    var output = rangeTransactions.Where(t => t.Category == cat && t.Date.Month == month.Month && t.Date.Year == month.Year && t.Type == "Debit").Sum(t => t.Amount);
                    catTrend.Add(output);
                }
                trendChartData[cat] = catTrend;
            }

            // Package Payloads for View
            ViewBag.StartDate = start.ToString("yyyy-MM-dd");
            ViewBag.EndDate = end.ToString("yyyy-MM-dd");
            
            ViewBag.TotalIncome = totalIncome;
            ViewBag.TotalExpense = totalExpense;
            ViewBag.NetSavings = netSavings;
            ViewBag.CurrentNetWorth = currentNetWorth;
            
            ViewBag.MonthLabels = netWorthLabels; // Shared X-Axis
            
            ViewBag.NetWorthData = netWorthData;
            ViewBag.IncomeBarData = incomeBarData;
            ViewBag.ExpenseBarData = expenseBarData;

            ViewBag.TopCategoriesNames = topCategories;
            ViewBag.TopCategoriesTrends = trendChartData; // Dictionary<string, List<decimal>>

            return View(rangeTransactions); // Send explicit transaction lines for the data table
        }
    }
}
