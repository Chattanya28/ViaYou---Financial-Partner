using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ViaYou.Data;

namespace ViaYou.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite("Data Source=viayou.db");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}