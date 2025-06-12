using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SFManagement.Models;
using SFManagement.Models.Closing;
using SFManagement.Models.Transactions;

namespace SFManagement.Data;

public class DataContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DataContext(DbContextOptions<DataContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Client> Clients { get; set; }

    public DbSet<Bank> Banks { get; set; }

    public DbSet<BankTransaction> BankTransactions { get; set; }

    public DbSet<Ofx> Ofxs { get; set; }
    
    public DbSet<OfxTransaction> OfxTransactions { get; set; }

    public DbSet<Manager> Managers { get; set; }

    public DbSet<Nickname> Nicknames { get; set; }

    public DbSet<Wallet> Wallets { get; set; }

    public DbSet<WalletTransaction> WalletTransactions { get; set; }

    public DbSet<Excel> Excels { get; set; }

    public DbSet<ExcelTransaction> ExcelTransactions { get; set; }

    public DbSet<Tag> Tags { get; set; }

    public DbSet<ClosingManager> ClosingManagers { get; set; }

    public DbSet<ClosingWallet> ClosingWallets { get; set; }

    public DbSet<ClosingNickname> ClosingNicknames { get; set; }

    public DbSet<InternalTransaction> InternalTransactions { get; set; }

    public DbSet<AvgRate> AvgRates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetDefaultProperties();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override EntityEntry<TEntity> Update<TEntity>(TEntity entity)
    {
        SetPropertyReflection(entity, "EditedAt");
        return base.Update(entity);
    }

    public override EntityEntry<TEntity> Add<TEntity>(TEntity entity)
    {
        SetPropertyReflection(entity, "CreatedAt");
        return base.Update(entity);
    }

    public override EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
    {
        SetPropertyReflection(entity, "ExcludedAt");
        return base.Update(entity);
    }

    private void SetPropertyReflection(object entity, string propertyName)
    {
    }

    private void SetDefaultProperties()
    {
        var userId = Guid.Empty;
        var user = _httpContextAccessor.HttpContext?.User;

        if (user != null && user.Claims.Count() > 0 && user.Claims != null && user.Claims.Any(c => c.Type == "uid"))
            Guid.TryParse(user.Claims.FirstOrDefault(c => c.Type == "uid").Value, out userId);

        foreach (var auditableEntity in ChangeTracker.Entries<BaseDomain>())
            if (auditableEntity.State == EntityState.Added)
            {
                if (userId != Guid.Empty) auditableEntity.Entity.CreatorId = userId;

                if (auditableEntity.Entity.CreatedAt == new DateTime() || auditableEntity.Entity.CreatedAt == null)
                    auditableEntity.Entity.CreatedAt = DateTime.Now;
            }
            else if (auditableEntity.State == EntityState.Modified)
            {
                if (auditableEntity.Entity.DeletedAt.HasValue)
                {
                    auditableEntity.Property(p => p.UpdatedAt).IsModified = false;

                    if (userId != Guid.Empty) auditableEntity.Entity.DeleteId = userId;

                    if (auditableEntity.Entity.DeletedAt == new DateTime() || auditableEntity.Entity.DeletedAt == null)
                        auditableEntity.Entity.DeletedAt = DateTime.Now;
                }
                else
                {
                    if (userId != Guid.Empty) auditableEntity.Entity.EditorId = userId;

                    if (auditableEntity.Entity.UpdatedAt == new DateTime() || auditableEntity.Entity.UpdatedAt == null)
                        auditableEntity.Entity.UpdatedAt = DateTime.Now;
                }
            }
            else
            {
                auditableEntity.Property(p => p.CreatedAt).IsModified = false;
            }
    }
}