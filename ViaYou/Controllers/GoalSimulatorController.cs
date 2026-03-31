using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViaYou.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ViaYou.Controllers
{
    [Authorize]
    public class GoalSimulatorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GoalSimulatorController(ApplicationDbContext context)
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

            var goals = await _context.Goals
                .Where(g => g.UserId == userId)
                .ToListAsync();

            var bankPolicies = await _context.BankPolicies
                .Where(p => p.PolicyType == "FD")
                .OrderByDescending(p => p.InterestRate)
                .Take(5)
                .ToListAsync();

            // If no policies in DB, supply some mocked ones so the feature still demonstrates properly
            if (!bankPolicies.Any())
            {
                bankPolicies = new List<ViaYou.Core.Entities.BankPolicy>
                {
                    new ViaYou.Core.Entities.BankPolicy { BankName = "AU Small Finance Bank", InterestRate = 8.5m },
                    new ViaYou.Core.Entities.BankPolicy { BankName = "Ujjivan Small Finance Bank", InterestRate = 8.25m },
                    new ViaYou.Core.Entities.BankPolicy { BankName = "HDFC Bank", InterestRate = 7.10m }
                };
            }

            ViewBag.Goals = goals;
            ViewBag.BankPolicies = bankPolicies;

            return View();
        }

        public class SimulateRequest
        {
            public int goalId { get; set; }
            public decimal amount { get; set; }
            public decimal newRate { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Simulate([FromBody] SimulateRequest request)
        {
            if (request == null) return Json(new { error = "Invalid payload" });

            var goal = await _context.Goals.FindAsync(request.goalId);
            if (goal == null) return Json(new { error = "Goal not found" });

            var currentRate = 3.5m; // Standard savings rate
            var extraPerYear = request.amount * (request.newRate - currentRate) / 100;
            var remainingAmount = goal.TargetAmount - goal.CurrentAmount;
            
            if (remainingAmount <= 0) 
            {
                return Json(new { 
                    error = "Goal already achieved", 
                    goalName = goal.Name 
                });
            }

            var monthlyExtra = extraPerYear / 12;
            var newMonthlyTotal = goal.MonthlyContribution + monthlyExtra;
            
            var monthsWithoutOptimization = goal.MonthlyContribution > 0
                ? (double)(remainingAmount / goal.MonthlyContribution)
                : 999;
                
            var monthsWithOptimization = newMonthlyTotal > 0
                ? (double)(remainingAmount / newMonthlyTotal)
                : 999;
                
            var monthsSaved = monthsWithoutOptimization - monthsWithOptimization;
            if (monthsSaved < 0) monthsSaved = 0;

            return Json(new
            {
                goalName = goal.Name,
                extraPerYear = extraPerYear,
                monthsSaved = Math.Round(monthsSaved, 1),
                originalCompletionDate = DateTime.Now.AddMonths((int)monthsWithoutOptimization).ToString("MMM yyyy"),
                newCompletionDate = DateTime.Now.AddMonths((int)monthsWithOptimization).ToString("MMM yyyy")
            });
        }
    }
}