using System.ComponentModel.DataAnnotations;

namespace ViaYou.Core.Entities
{
    public class MutualFund
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FundName { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public decimal NAV { get; set; }

        public decimal Returns1Y { get; set; }

        public decimal Returns3Y { get; set; }

        public decimal Returns5Y { get; set; }

        public decimal ExpenseRatio { get; set; }

        public decimal MinimumInvestment { get; set; }

        public string RiskLevel { get; set; } = string.Empty;

        public bool IsDirectPlan { get; set; } = true;
    }
}