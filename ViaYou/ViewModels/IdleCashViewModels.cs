namespace ViaYou.ViewModels
{
    public class IdleCashOpportunity
    {
        public int AccountId { get; set; }
        public string BankName { get; set; } = string.Empty;
        public decimal CurrentBalance { get; set; }
        public decimal CurrentRate { get; set; }
        public List<BetterRateOption> BetterOptions { get; set; } = new List<BetterRateOption>();
        public decimal PotentialGain { get; set; }
        public string FormattedBalance => $"₹{CurrentBalance:N0}";
        public string FormattedCurrentRate => $"{CurrentRate}%";
        public string FormattedPotentialGain => $"₹{PotentialGain:N0}";
    }

    public class BetterRateOption
    {
        public string BankName { get; set; } = string.Empty;
        public decimal CurrentRate { get; set; }
        public decimal NewRate { get; set; }
        public decimal ExtraEarning { get; set; }
        public decimal MinAmount { get; set; }
        public string FormattedExtraEarning => $"₹{ExtraEarning:N0}";
        public string FormattedNewRate => $"{NewRate}%";
        public string FormattedDifference => $"+{(NewRate - CurrentRate):F1}%";
    }
}