using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ViaYou.Data;
using ViaYou.Core.Entities;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace ViaYou.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string search, string category, string type, DateTime? startDate, DateTime? endDate)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId)) 
                return RedirectToAction("Login", "Auth");

            var accounts = await _context.Accounts.Where(a => a.UserId == userId).ToListAsync();
            var accountIds = accounts.Select(a => a.Id).ToList();

            var query = _context.Transactions
                .Include(t => t.Account)
                .Where(t => accountIds.Contains(t.AccountId))
                .AsQueryable();

            // Filter Matrix
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Merchant.Contains(search) || t.Amount.ToString().Contains(search));
            }
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(t => t.Category == category);
            }
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(t => t.Type == type);
            }
            if (startDate.HasValue)
            {
                query = query.Where(t => t.Date >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(t => t.Date <= endDate.Value.AddDays(1).AddTicks(-1));
            }

            var transactions = await query.OrderByDescending(t => t.Date).ToListAsync();

            // Dynamic Chart.js Pipeline Extraction
            var categoriesData = transactions
                .Where(t => t.Type == "Debit")
                .GroupBy(t => t.Category ?? "Other")
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

            ViewBag.CategoriesLabels = categoriesData.Keys.ToList();
            ViewBag.CategoriesData = categoriesData.Values.ToList();
            
            ViewBag.Accounts = accounts;
            ViewBag.Search = search;
            ViewBag.CategoryFilter = category;
            ViewBag.TypeFilter = type;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(transactions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTransaction(int accountId, string merchant, decimal amount, string type, string category, DateTime date, bool isRecurring)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId)) 
                return RedirectToAction("Login", "Auth");

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);
            if (account == null)
            {
                TempData["ErrorMessage"] = "Invalid source account routing.";
                return RedirectToAction(nameof(Index));
            }

            if (type.ToLower() == "debit" && account.Balance < amount)
            {
                TempData["ErrorMessage"] = "Insufficient funds to execute this debit.";
                return RedirectToAction(nameof(Index));
            }

            // Adjust live balance
            account.Balance += (type.ToLower() == "credit" ? amount : -amount);

            var transaction = new Transaction
            {
                AccountId = account.Id,
                Merchant = merchant,
                Amount = amount,
                Type = type,
                Category = category,
                Date = date,
                IsRecurring = isRecurring
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Transaction '{merchant}' logged successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadReceipt(int transactionId, IFormFile receiptFile)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId)) 
                return RedirectToAction("Login", "Auth");

            var transaction = await _context.Transactions
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t => t.Id == transactionId && t.Account.UserId == userId);

            if (transaction == null)
            {
                TempData["ErrorMessage"] = "Transaction record unverified.";
                return RedirectToAction(nameof(Index));
            }

            if (receiptFile != null && receiptFile.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await receiptFile.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                string base64String = Convert.ToBase64String(imageBytes);
                
                transaction.ReceiptImage = $"data:{receiptFile.ContentType};base64,{base64String}";
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Receipt formally attached to ledger.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
