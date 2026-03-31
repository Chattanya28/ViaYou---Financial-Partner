using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ViaYou.Core.Entities;

namespace ViaYou.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<BankPolicy> BankPolicies { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<LoginHistory> LoginHistory { get; set; }
        public DbSet<MutualFund> MutualFunds { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Bill> Bills { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<Account>()
                .HasOne(a => a.User)
                .WithMany(u => u.Accounts)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Goal>()
                .HasOne(g => g.User)
                .WithMany(u => u.Goals)
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<LoginHistory>()
                .HasOne(l => l.User)
                .WithMany(u => u.LoginHistory)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Transaction>()
                .HasOne(t => t.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Bill>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bills)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // REAL FD RATES from Indian banks (as of 2024-2025)
            var fixedDate = new DateTime(2025, 1, 1);

            builder.Entity<BankPolicy>().HasData(
                // Small Finance Banks (Highest Rates)
                new BankPolicy { Id = 1, BankName = "AU Small Finance Bank", PolicyType = "FD", InterestRate = 8.0m, MinimumAmount = 1000, TenureDays = 365, LogoUrl = "/assets/banks/au.png", UpdatedAt = fixedDate },
                new BankPolicy { Id = 2, BankName = "Equitas Small Finance Bank", PolicyType = "FD", InterestRate = 8.1m, MinimumAmount = 1000, TenureDays = 365, LogoUrl = "/assets/banks/equitas.png", UpdatedAt = fixedDate },
                new BankPolicy { Id = 3, BankName = "Ujjivan Small Finance Bank", PolicyType = "FD", InterestRate = 8.0m, MinimumAmount = 1000, TenureDays = 365, LogoUrl = "/assets/banks/ujjivan.png", UpdatedAt = fixedDate },
                new BankPolicy { Id = 4, BankName = "Jana Small Finance Bank", PolicyType = "FD", InterestRate = 8.2m, MinimumAmount = 1000, TenureDays = 365, LogoUrl = "/assets/banks/jana.png", UpdatedAt = fixedDate },

                // Private Banks
                new BankPolicy { Id = 5, BankName = "HDFC Bank", PolicyType = "FD", InterestRate = 7.2m, MinimumAmount = 10000, TenureDays = 365, LogoUrl = "/assets/banks/hdfc.png", UpdatedAt = fixedDate },
                new BankPolicy { Id = 6, BankName = "ICICI Bank", PolicyType = "FD", InterestRate = 7.1m, MinimumAmount = 10000, TenureDays = 365, LogoUrl = "/assets/banks/icici.png", UpdatedAt = fixedDate },
                new BankPolicy { Id = 7, BankName = "Axis Bank", PolicyType = "FD", InterestRate = 7.0m, MinimumAmount = 10000, TenureDays = 365, LogoUrl = "/assets/banks/axis.png", UpdatedAt = fixedDate },
                new BankPolicy { Id = 8, BankName = "Kotak Mahindra Bank", PolicyType = "FD", InterestRate = 7.0m, MinimumAmount = 5000, TenureDays = 365, LogoUrl = "/assets/banks/kotak.png", UpdatedAt = fixedDate },
                new BankPolicy { Id = 9, BankName = "Yes Bank", PolicyType = "FD", InterestRate = 7.2m, MinimumAmount = 10000, TenureDays = 365, LogoUrl = "/assets/banks/yes.png", UpdatedAt = fixedDate },
                new BankPolicy { Id = 10, BankName = "IndusInd Bank", PolicyType = "FD", InterestRate = 7.3m, MinimumAmount = 10000, TenureDays = 365, LogoUrl = "/assets/banks/indusind.png", UpdatedAt = fixedDate },

                // Public Sector Banks
                new BankPolicy { Id = 11, BankName = "State Bank of India", PolicyType = "FD", InterestRate = 6.8m, MinimumAmount = 1000, TenureDays = 365, LogoUrl = "/assets/banks/sbi.png", UpdatedAt = fixedDate },
                new BankPolicy { Id = 12, BankName = "Bank of Baroda", PolicyType = "FD", InterestRate = 6.7m, MinimumAmount = 1000, TenureDays = 365, LogoUrl = "/assets/banks/bob.png", UpdatedAt = fixedDate },
                new BankPolicy { Id = 13, BankName = "Canara Bank", PolicyType = "FD", InterestRate = 6.7m, MinimumAmount = 1000, TenureDays = 365, LogoUrl = "/assets/banks/canara.png", UpdatedAt = fixedDate },
                new BankPolicy { Id = 14, BankName = "Punjab National Bank", PolicyType = "FD", InterestRate = 6.6m, MinimumAmount = 1000, TenureDays = 365, LogoUrl = "/assets/banks/pnb.png", UpdatedAt = fixedDate },

                // New Age Banks
                new BankPolicy { Id = 15, BankName = "IDFC FIRST Bank", PolicyType = "FD", InterestRate = 7.5m, MinimumAmount = 1000, TenureDays = 365, LogoUrl = "/assets/banks/idfc.png", UpdatedAt = fixedDate },
                new BankPolicy { Id = 16, BankName = "Bandhan Bank", PolicyType = "FD", InterestRate = 7.6m, MinimumAmount = 5000, TenureDays = 365, LogoUrl = "/assets/banks/bandhan.png", UpdatedAt = fixedDate }
            );
        }
    }
}