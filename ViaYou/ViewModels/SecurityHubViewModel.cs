using System.Collections.Generic;
using ViaYou.Core.Entities;

namespace ViaYou.ViewModels
{
    public class SecurityHubViewModel
    {
        public int TotalScore { get; set; }
        
        // Settings / Flags
        public bool Is2FAEnabled { get; set; }
        public bool IsPasswordHealthy { get; set; }
        public bool HasSafeSessionsCount { get; set; }
        public bool HasNoSuspiciousDevices { get; set; }
        public bool LoginAlertsEnabled { get; set; }
        public bool TransactionLimitsSet { get; set; }
        public bool RecoveryEmailConfigured { get; set; }

        // Data arrays
        public List<LoginHistory> ActiveSessions { get; set; } = new List<LoginHistory>();
        public List<LoginHistory> TrustedDevices { get; set; } = new List<LoginHistory>();
        public List<LoginHistory> Alerts { get; set; } = new List<LoginHistory>(); // E.g., not trusted, strange location
    }
}
