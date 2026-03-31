using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViaYou.Core.Entities
{
    public class Goal
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Currency)]
        public decimal TargetAmount { get; set; }

        [DataType(DataType.Currency)]
        public decimal CurrentAmount { get; set; }

        public DateTime TargetDate { get; set; }

        [StringLength(10)]
        public string? IconEmoji { get; set; }

        public decimal MonthlyContribution { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}