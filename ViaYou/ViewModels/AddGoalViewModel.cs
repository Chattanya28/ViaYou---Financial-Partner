using System;
using System.ComponentModel.DataAnnotations;

namespace ViaYou.ViewModels
{
    public class AddGoalViewModel
    {
        [Required(ErrorMessage = "Goal Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Target Amount is required")]
        [Range(100, 100000000, ErrorMessage = "Target must be at least ₹100")]
        public decimal TargetAmount { get; set; }

        [Range(0, 100000000)]
        public decimal CurrentAmount { get; set; } = 0;

        [Required(ErrorMessage = "Target Date is required")]
        public DateTime TargetDate { get; set; } = DateTime.Now.AddYears(1);

        [StringLength(10)]
        public string IconEmoji { get; set; } = "🎯";

        [Range(0, 10000000, ErrorMessage = "Invalid monthly contribution")]
        public decimal MonthlyContribution { get; set; } = 0;
    }
}
