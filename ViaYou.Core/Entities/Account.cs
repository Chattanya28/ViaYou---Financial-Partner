using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViaYou.Core.Entities
{
    public class Account
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string AccountType { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Currency)]
        public decimal Balance { get; set; }

        public decimal InterestRate { get; set; }

        [StringLength(20)]
        public string? AccountNumberMasked { get; set; }

        // ADD THIS MISSING PROPERTY - Full account number (encrypted in production)
        [StringLength(50)]
        public string? AccountNumber { get; set; }

        [StringLength(10)]
        public string? ColorHex { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
        public ICollection<Transaction>? Transactions { get; set; }
    }
}