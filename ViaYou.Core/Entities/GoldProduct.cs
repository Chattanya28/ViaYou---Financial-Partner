using System.ComponentModel.DataAnnotations;

namespace ViaYou.Core.Entities
{
    public class GoldProduct
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ProductName { get; set; } = string.Empty;

        public string ProductType { get; set; } = string.Empty; // Digital Gold, Gold ETF, SGB

        public decimal CurrentPrice { get; set; } // per gram for digital gold, per unit for ETFs

        public decimal MinimumQuantity { get; set; }

        public decimal MakingCharges { get; set; } // percentage

        public bool IsTaxEfficient { get; set; }

        public decimal InterestRate { get; set; } // for SGB only

        public string Description { get; set; } = string.Empty;
    }
}