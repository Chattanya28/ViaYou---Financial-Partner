using System;
using System.ComponentModel.DataAnnotations;

namespace ViaYou.ViewModels
{
    public class AddBillViewModel
    {
        [Required(ErrorMessage = "Biller Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Biller name must be between 2 and 100 characters")]
        public string BillerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is required")]
        [Range(1, 10000000, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Due Date is required")]
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(7);

        public string Category { get; set; } = "Utility";

        public bool IsAutoPay { get; set; } = false;

        public string? AccountNumber { get; set; }

        public bool IsFrequent { get; set; } = false;
    }
}
