using System;
using System.ComponentModel.DataAnnotations;

namespace ViaYou.ViewModels
{
    public class AddTransactionViewModel
    {
        [Required(ErrorMessage = "Please select an account")]
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Merchant Name is required")]
        [StringLength(100, ErrorMessage = "Merchant name cannot exceed 100 characters")]
        public string Merchant { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 10000000, ErrorMessage = "Amount must be strictly positive")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Credit or Debit type is required")]
        public string Type { get; set; } = "Debit";

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; } = "Other";

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
