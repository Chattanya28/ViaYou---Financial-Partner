using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViaYou.Data;
using ViaYou.Core.Entities;
using ViaYou.ViewModels;
using System.Security.Claims;

namespace ViaYou.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get current logged-in user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Get the actual user
            var currentUser = await _context.Users.FindAsync(userId);
            var firstName = currentUser?.FullName?.Split(' ')[0] ?? "User";

            // Get user-specific data
            var accounts = await _context.Accounts
                .Where(a => a.UserId == userId)
                .ToListAsync();

            var goals = await _context.Goals
                .Where(g => g.UserId == userId)
                .ToListAsync();

            var netWorth = accounts.Sum(a => a.Balance);

            // Detect idle cash opportunities
            var idleCashOpportunities = await DetectIdleCash(userId, accounts);

            // Get recent transactions
            var recentTransactions = await _context.Transactions
                .Where(t => t.Account != null && t.Account.UserId == userId)
                .OrderByDescending(t => t.Date)
                .Take(5)
                .ToListAsync();

            // Real security score calculation
            int securityScore = 0;
            if (currentUser.TwoFactorEnabled) securityScore += 25;
            if ((DateTime.UtcNow - currentUser.LastPasswordChangedDate).TotalDays < 90) securityScore += 15;
            if (currentUser.LoginAlertsEnabled) securityScore += 10;
            if (currentUser.TransactionLimitsSet) securityScore += 10;
            if (!string.IsNullOrEmpty(currentUser.RecoveryEmail)) securityScore += 15;
            
            // For chart data (Weekly spending)
            var weekAgo = DateTime.Now.AddDays(-7);
            var weeklyTransactions = await _context.Transactions
                .Where(t => t.Account!.UserId == userId && t.Type.ToLower() == "debit" && t.Date >= weekAgo)
                .ToListAsync();

            var groupedDaily = weeklyTransactions
                .GroupBy(t => t.Date.Date)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

            var chartLabels = new List<string>();
            var chartData = new List<decimal>();
            for (int i = 6; i >= 0; i--)
            {
                var d = DateTime.Now.Date.AddDays(-i);
                chartLabels.Add(d.ToString("ddd"));
                chartData.Add(groupedDaily.ContainsKey(d) ? groupedDaily[d] : 0);
            }

            // Spending Categories
            var categories = await _context.Transactions
                .Where(t => t.Account!.UserId == userId && t.Type.ToLower() == "debit")
                .GroupBy(t => t.Category)
                .Select(g => new { Category = g.Key ?? "Other", Total = g.Sum(t => t.Amount) })
                .OrderByDescending(x => x.Total)
                .Take(4)
                .ToListAsync();

            ViewBag.Accounts = accounts;
            ViewBag.Goals = goals;
            ViewBag.NetWorth = netWorth;
            ViewBag.UserName = firstName;
            ViewBag.IdleCashOpportunities = idleCashOpportunities;
            ViewBag.RecentTransactions = recentTransactions;
            ViewBag.SecurityScore = securityScore;
            ViewBag.ChartLabels = chartLabels;
            ViewBag.ChartData = chartData;
            ViewBag.SpendingCategories = categories;

            return View();
        }

        private async Task<List<IdleCashOpportunity>> DetectIdleCash(int userId, List<Account> accounts)
        {
            var opportunities = new List<IdleCashOpportunity>();

            // Get all FD policies from database
            var fdPolicies = await _context.BankPolicies
                .Where(p => p.PolicyType == "FD")
                .OrderByDescending(p => p.InterestRate)
                .ToListAsync();

            foreach (var account in accounts.Where(a => a.AccountType == "Savings" && a.Balance > 25000))
            {
                // Find better rates than current savings rate
                var betterRates = fdPolicies
                    .Where(p => p.InterestRate > account.InterestRate && p.MinimumAmount <= account.Balance)
                    .Select(p => new BetterRateOption
                    {
                        BankName = p.BankName,
                        CurrentRate = account.InterestRate,
                        NewRate = p.InterestRate,
                        ExtraEarning = account.Balance * (p.InterestRate - account.InterestRate) / 100,
                        MinAmount = p.MinimumAmount
                    })
                    .OrderByDescending(p => p.ExtraEarning)
                    .Take(3)
                    .ToList();

                if (betterRates.Any())
                {
                    opportunities.Add(new IdleCashOpportunity
                    {
                        AccountId = account.Id,
                        BankName = account.BankName,
                        CurrentBalance = account.Balance,
                        CurrentRate = account.InterestRate,
                        BetterOptions = betterRates,
                        PotentialGain = betterRates.First().ExtraEarning
                    });
                }
            }

            return opportunities;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAccount(AddAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid account details provided. Please check the form.";
                return RedirectToAction("Index");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Apply light masking logic to the account number: e.g. "XXXX-XXXX-1234"
            string? maskedNumber = null;
            if (!string.IsNullOrWhiteSpace(model.AccountNumber))
            {
                string num = model.AccountNumber.Trim();
                if (num.Length > 4)
                {
                    maskedNumber = new string('*', num.Length - 4) + num.Substring(num.Length - 4);
                }
                else
                {
                    maskedNumber = new string('*', num.Length);
                }
            }

            var account = new Account
            {
                UserId = userId,
                BankName = model.BankName,
                AccountType = model.AccountType,
                Balance = model.Balance,
                InterestRate = model.InterestRate,
                AccountNumberMasked = maskedNumber,
                LastUpdated = DateTime.Now
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{model.BankName} {model.AccountType} account was successfully connected!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTransaction(AddTransactionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid transaction parameters.";
                return RedirectToAction("Index");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId)) return RedirectToAction("Login", "Auth");

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == model.AccountId && a.UserId == userId);
            if (account == null)
            {
                TempData["ErrorMessage"] = "Select a valid account to log this transaction.";
                return RedirectToAction("Index");
            }

            var transaction = new Transaction
            {
                AccountId = account.Id,
                Merchant = model.Merchant,
                Amount = model.Amount,
                Type = model.Type,
                Category = model.Category,
                Date = model.Date
            };

            // Update Account Balance intrinsically 
            if (model.Type.Equals("Debit", StringComparison.OrdinalIgnoreCase))
            {
                if (account.Balance < model.Amount) TempData["ErrorMessage"] = "Warning: This transaction resulted in a negative balance.";
                account.Balance -= model.Amount;
            }
            else
            {
                account.Balance += model.Amount;
            }

            account.LastUpdated = DateTime.Now;
            
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"₹{model.Amount:N2} {model.Type} to {model.Merchant} saved!";
            TempData["UndoTransactionId"] = transaction.Id;

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UndoTransaction(int transactionId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId)) return RedirectToAction("Login", "Auth");

            var transaction = await _context.Transactions
                                        .Include(t => t.Account)
                                        .FirstOrDefaultAsync(t => t.Id == transactionId && t.Account!.UserId == userId);

            if (transaction != null)
            {
                // Reverse the balance impact
                if (transaction.Type.Equals("Debit", StringComparison.OrdinalIgnoreCase))
                {
                    transaction.Account!.Balance += transaction.Amount;
                }
                else
                {
                    transaction.Account!.Balance -= transaction.Amount;
                }

                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Transaction was undone successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Transaction could not be found or was already removed.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGoal(AddGoalViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid goal configuration parameters.";
                return RedirectToAction("Index");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId)) return RedirectToAction("Login", "Auth");

            if (model.CurrentAmount > model.TargetAmount)
            {
                TempData["ErrorMessage"] = "Target Amount must be larger than Current Amount.";
                return RedirectToAction("Index");
            }

            var goal = new Goal
            {
                UserId = userId,
                Name = model.Name,
                TargetAmount = model.TargetAmount,
                CurrentAmount = model.CurrentAmount,
                TargetDate = model.TargetDate,
                IconEmoji = model.IconEmoji ?? "🎯",
                MonthlyContribution = model.MonthlyContribution,
                CreatedAt = DateTime.Now
            };

            _context.Goals.Add(goal);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Your new goal '{model.Name}' is locked in!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayUpi(int accountId, string upiId, decimal amount)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId)) return RedirectToAction("Login", "Auth");

            if (amount <= 0 || string.IsNullOrEmpty(upiId))
            {
                TempData["ErrorMessage"] = "Invalid UPI transaction parameters.";
                return RedirectToAction("Index");
            }

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);
            if (account == null)
            {
                TempData["ErrorMessage"] = "Source account not found or unauthorized.";
                return RedirectToAction("Index");
            }

            if (account.Balance < amount)
            {
                TempData["ErrorMessage"] = $"Account '{account.BankName}' has insufficient funds (₹{account.Balance:N2}).";
                return RedirectToAction("Index");
            }

            account.Balance -= amount;

            var transaction = new Transaction
            {
                AccountId = account.Id,
                Amount = amount,
                Type = "Debit",
                Category = "UPI Transfer",
                Merchant = $"UPI: {upiId}",
                Date = DateTime.Now
            };

            var notification = new Notification
            {
                UserId = userId,
                Title = "UPI Payment Successful",
                Message = $"₹{amount:N2} was transferred to {upiId}.",
                Type = "Success"
            };

            _context.Transactions.Add(transaction);
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"₹{amount:N2} sent to {upiId} successfully!";
            return RedirectToAction("Index");
        }
    }
}