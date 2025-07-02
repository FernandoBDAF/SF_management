using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SFManagement.Models;
using SFManagement.Models.AssetInfrastructure;
using SFManagement.Models.Entities;
using SFManagement.Models.Support;
using SFManagement.Models.Transactions;


namespace SFManagement.Data;

public class DataContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DataContext(DbContextOptions<DataContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public DbSet<BaseAssetHolder> BaseAssetHolders { get; set; }
    public DbSet<Bank> Banks { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<PokerManager> PokerManagers { get; set; }
    
    public DbSet<Address> Addresses { get; set; }
    public DbSet<InitialBalance> InitialBalances { get; set; }
    public DbSet<FinancialBehavior> FinancialBehaviors { get; set; }
    
    public DbSet<AssetWallet> AssetWallets { get; set; }
    public DbSet<WalletIdentifier> WalletIdentifiers { get; set; }
    
    public DbSet<FiatAssetTransaction> FiatAssetTransactions { get; set; }
    public DbSet<DigitalAssetTransaction> DigitalAssetTransactions { get; set; }
    
    public DbSet<Ofx> Ofxs { get; set; }
    public DbSet<OfxTransaction> OfxTransactions { get; set; }
    public DbSet<Excel> Excels { get; set; }
    public DbSet<ExcelTransaction> ExcelTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the one-to-one relationships between BaseAssetHolder and specific entities
        modelBuilder.Entity<Client>()
            .HasOne<BaseAssetHolder>()
            .WithOne(bah => bah.Client)
            .HasForeignKey<Client>(c => c.BaseAssetHolderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Bank>()
            .HasOne<BaseAssetHolder>()
            .WithOne(bah => bah.Bank)
            .HasForeignKey<Bank>(b => b.BaseAssetHolderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Member>()
            .HasOne<BaseAssetHolder>()
            .WithOne(bah => bah.Member)
            .HasForeignKey<Member>(m => m.BaseAssetHolderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PokerManager>()
            .HasOne<BaseAssetHolder>()
            .WithOne(bah => bah.PokerManager)
            .HasForeignKey<PokerManager>(pm => pm.BaseAssetHolderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure WalletIdentifier relationships
        modelBuilder.Entity<WalletIdentifier>()
            .HasOne(wi => wi.BaseAssetHolder)
            .WithMany(bah => bah.WalletIdentifiers)
            .HasForeignKey(wi => wi.BaseAssetHolderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure AssetWallet relationships
        modelBuilder.Entity<AssetWallet>()
            .HasOne(aw => aw.BaseAssetHolder)
            .WithMany(bah => bah.AssetWallets)
            .HasForeignKey(aw => aw.BaseAssetHolderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure transaction relationships
        // modelBuilder.Entity<DigitalAssetTransaction>()
        //     .HasOne(dat => dat.WalletIdentifier)
        //     .WithMany(wi => wi.DigitalAssetTransactions)
        //     .HasForeignKey(dat => dat.WalletIdentifierId)
        //     .OnDelete(DeleteBehavior.Restrict);

        // modelBuilder.Entity<DigitalAssetTransaction>()
        //     .HasOne(dat => dat.AssetWallet)
        //     .WithMany(aw => aw.DigitalAssetTransactions)
        //     .HasForeignKey(dat => dat.AssetWalletId)
        //     .OnDelete(DeleteBehavior.Restrict);

        // modelBuilder.Entity<FiatAssetTransaction>()
        //     .HasOne(fat => fat.WalletIdentifier)
        //     .WithMany(wi => wi.FiatAssetTransactions)
        //     .HasForeignKey(fat => fat.WalletIdentifierId)
        //     .OnDelete(DeleteBehavior.Restrict);

        // modelBuilder.Entity<FiatAssetTransaction>()
        //     .HasOne(fat => fat.AssetWallet)
        //     .WithMany(aw => aw.FiatAssetTransactions)
        //     .HasForeignKey(fat => fat.AssetWalletId)
        //     .OnDelete(DeleteBehavior.Restrict);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetDefaultProperties();
        return base.SaveChangesAsync(cancellationToken);
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
                if (auditableEntity.Entity.CreatedAt == new DateTime() || auditableEntity.Entity.CreatedAt == null)
                    auditableEntity.Entity.CreatedAt = DateTime.Now;
            }
            else if (auditableEntity.State == EntityState.Modified)
            {
                if (auditableEntity.Entity.DeletedAt.HasValue)
                {
                    auditableEntity.Property(p => p.UpdatedAt).IsModified = false;

                    if (auditableEntity.Entity.DeletedAt == new DateTime() || auditableEntity.Entity.DeletedAt == null)
                        auditableEntity.Entity.DeletedAt = DateTime.Now;
                }
                else
                {
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