using System.ComponentModel.DataAnnotations;

namespace ViaYou.ViewModels
{
    public class AddAccountViewModel
    {
        [Required(ErrorMessage = "Bank Name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Bank Name must be between 2 and 50 characters")]
        public string BankName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Account Type is required")]
        public string AccountType { get; set; } = "Savings";

        [Required(ErrorMessage = "Current Balance is required")]
        [Range(0, 100000000, ErrorMessage = "Balance must be a positive amount")]
        [DataType(DataType.Currency)]
        public decimal Balance { get; set; }

        [Range(0, 30, ErrorMessage = "Interest rate should be between 0% and 30%")]
        public decimal InterestRate { get; set; }

        [StringLength(20, ErrorMessage = "Account number length must be up to 20 characters")]
        public string? AccountNumber { get; set; }
    }
}
