using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SFManagement.Models;

namespace SFManagement.Data
{
    public class DataContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }

        public DbSet<Bank> Banks { get; set; }

        public DbSet<BankTransaction> BankTransactions { get; set; }

        public DbSet<Ofx> Ofxs { get; set; }

        public DbSet<Manager> Managers { get; set; }

        public DbSet<Nickname> Nicknames { get; set; }

        public DbSet<Wallet> Wallets { get; set; }

        public DbSet<WalletTransaction> WalletTransactions { get; set; }

        public DbSet<Excel> Excels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}