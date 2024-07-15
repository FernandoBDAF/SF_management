using Microsoft.EntityFrameworkCore;
using SFManagement.Models;

namespace SFManagement.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }

        public DbSet<Bank> Banks { get; set; }

        public DbSet<BankTransaction> BankTransactions { get; set; }

        public DbSet<Ofx> Ofxs { get; set; }
    }
}