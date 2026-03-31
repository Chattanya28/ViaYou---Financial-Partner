using System;
using System.ComponentModel.DataAnnotations;

namespace ViaYou.Core.Entities
{
    public class BankPolicy
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PolicyType { get; set; } = string.Empty;

        public decimal InterestRate { get; set; }

        public decimal MinimumAmount { get; set; }

        public int TenureDays { get; set; }

        [StringLength(100)]
        public string? LogoUrl { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}