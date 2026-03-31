using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ViaYou.Data;
using ViaYou.Core.Entities;
using ViaYou.ViewModels;

namespace ViaYou.Controllers
{
    [Authorize]
    public class BillsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BillsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var bills = await _context.Bills
                                    .Where(b => b.UserId == userId)
                                    .OrderBy(b => b.DueDate)
                                    .ToListAsync();

            // Set dynamic overdue statuses before shipping to View
            var today = DateTime.Now.Date;
            foreach (var bill in bills)
            {
                if (bill.Status == "Pending" && bill.DueDate.Date < today)
                {
                    bill.Status = "Overdue";
                    _context.Bills.Update(bill);
                }
            }
            if (_context.ChangeTracker.HasChanges()) await _context.SaveChangesAsync();

            ViewBag.Accounts = await _context.Accounts.Where(a => a.UserId == userId).ToListAsync();
            
            return View(bills);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBill(AddBillViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Provide valid bill parameters.";
                return RedirectToAction("Index");
            }

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId)) return RedirectToAction("Login", "Auth");

            var bill = new Bill
            {
                UserId = userId,
                BillerName = model.BillerName,
                Amount = model.Amount,
                DueDate = model.DueDate,
                Category = model.Category,
                IsAutoPay = model.IsAutoPay,
                AccountNumber = model.AccountNumber,
                IsFrequent = model.IsFrequent,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Scheduled {model.BillerName} for {model.DueDate:MMM dd}.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayBill(int billId, int accountId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId)) return RedirectToAction("Login", "Auth");

            var bill = await _context.Bills.FirstOrDefaultAsync(b => b.Id == billId && b.UserId == userId);
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);

            if (bill == null || account == null || bill.Status == "Paid")
            {
                TempData["ErrorMessage"] = "Payment failed. Confirm bill identity and source account.";
                return RedirectToAction("Index");
            }

            if (account.Balance < bill.Amount)
            {
                TempData["ErrorMessage"] = $"Account '{account.BankName}' has insufficient funds to pay {bill.BillerName}.";
                return RedirectToAction("Index");
            }

            // Deduct funds
            account.Balance -= bill.Amount;
            
            // Mark bill paid
            bill.Status = "Paid";
            
            // Log as a formal transaction
            var record = new Transaction
            {
                AccountId = account.Id,
                Merchant = bill.BillerName,
                Amount = bill.Amount,
                Type = "Debit",
                Category = bill.Category,
                Date = DateTime.Now
            };

            // Log a Notification system alert
            var alert = new Notification
            {
                UserId = userId,
                Title = "Bill Payment Successful",
                Message = $"Successfully paid ₹{bill.Amount:N2} to {bill.BillerName} from {account.BankName}.",
                Type = "Success",
                CreatedAt = DateTime.Now
            };

            _context.Transactions.Add(record);
            _context.Notifications.Add(alert);
            
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"Payment processing completed for {bill.BillerName}.";
            return RedirectToAction("Index");
        }
    }
}
