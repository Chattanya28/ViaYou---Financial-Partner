using System;

namespace ViaYou.Core.Entities
{
    public class LoginHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime LoginTime { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string DeviceInfo { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public bool IsTrusted { get; set; } = true;
        public bool IsRevoked { get; set; } = false;

        // Navigation property
        public User User { get; set; } = null!;
    }
}