using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViaYou.Core.Entities
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        public int AccountId { get; set; }

        [Required]
        [StringLength(100)]
        public string Merchant { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(10)]
        public string Type { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? Category { get; set; }

        public string? ReceiptImage { get; set; }
        
        public bool IsRecurring { get; set; } = false;

        [ForeignKey("AccountId")]
        public Account? Account { get; set; }
    }
}