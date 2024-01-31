using Microsoft.EntityFrameworkCore;

namespace ManagementMVC.Models
{
    public class FinanceDbContext : DbContext
    {
        public FinanceDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Category { get; set; } 
    }
}
