using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViaYou.Core.Entities
{
    public class Bill
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string BillerName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [StringLength(50)]
        public string Category { get; set; } = "Utility";

        public bool IsAutoPay { get; set; } = false;

        [StringLength(100)]
        public string? AccountNumber { get; set; }

        public bool IsFrequent { get; set; } = false;

        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Paid, Overdue

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}
