using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace ViaYou.Core.Entities
{
    public class User : IdentityUser<int>
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? ProfilePicture { get; set; }
        
        // Security Hub Tracking
        public DateTime LastPasswordChangedDate { get; set; } = DateTime.UtcNow;
        public bool LoginAlertsEnabled { get; set; } = true;
        public bool TransactionLimitsSet { get; set; } = true;
        public string? RecoveryEmail { get; set; }

        public string KycStatus { get; set; } = "Pending";
        public string LanguagePreference { get; set; } = "English";

        // Navigation properties
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
        public ICollection<Goal> Goals { get; set; } = new List<Goal>();
        public ICollection<LoginHistory> LoginHistory { get; set; } = new List<LoginHistory>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<Bill> Bills { get; set; } = new List<Bill>();
    }
}