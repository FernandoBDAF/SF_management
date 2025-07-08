using Microsoft.EntityFrameworkCore;
using SFManagement.Models;
using SFManagement.Models.AssetInfrastructure;
using SFManagement.Models.Entities;
using SFManagement.Models.Support;
using SFManagement.Models.Transactions;
using SFManagement.Services;
using Microsoft.Extensions.Logging;

namespace SFManagement.Data;

    public class DataContext(DbContextOptions<DataContext> options, IHttpContextAccessor httpContextAccessor, ILoggingService loggingService) : DbContext(options)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ILoggingService _loggingService = loggingService;

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
    public DbSet<SettlementTransaction> SettlementTransactions { get; set; }
    public DbSet<AgencyInvoice> AgencyInvoices { get; set; }
    
    public DbSet<Ofx> Ofxs { get; set; }
    public DbSet<OfxTransaction> OfxTransactions { get; set; }
    public DbSet<Excel> Excels { get; set; }
    public DbSet<ExcelTransaction> ExcelTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the one-to-one relationships between BaseAssetHolder and specific entities
        modelBuilder.Entity<Client>()
            .HasOne(c => c.BaseAssetHolder)
            .WithOne(bah => bah.Client)
            .HasForeignKey<Client>(c => c.BaseAssetHolderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Bank>()
            .HasOne(b => b.BaseAssetHolder)
            .WithOne(bah => bah.Bank)
            .HasForeignKey<Bank>(b => b.BaseAssetHolderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Member>()
            .HasOne(m => m.BaseAssetHolder)
            .WithOne(bah => bah.Member)
            .HasForeignKey<Member>(m => m.BaseAssetHolderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PokerManager>()
            .HasOne(pm => pm.BaseAssetHolder)
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
        var userId = GetCurrentUserId();
        
        foreach (var auditableEntity in ChangeTracker.Entries<BaseDomain>())
        {
            if (auditableEntity.State == EntityState.Added)
            {
                auditableEntity.Entity.CreatedAt = DateTime.UtcNow;
                auditableEntity.Entity.LastModifiedBy = userId;
                
                // Log creation event
                _loggingService.LogDataAccess("create", auditableEntity.Entity.GetType().Name, 
                    auditableEntity.Entity.Id, new { 
                        EntityType = auditableEntity.Entity.GetType().Name,
                        EntityId = auditableEntity.Entity.Id,
                        CreatedBy = userId,
                        CreatedAt = auditableEntity.Entity.CreatedAt
                    });
            }
            else if (auditableEntity.State == EntityState.Modified)
            {
                if (auditableEntity.Entity.DeletedAt.HasValue)
                {
                    // Soft delete operation
                    auditableEntity.Property(p => p.UpdatedAt).IsModified = false;
                    
                    if (auditableEntity.Entity.DeletedAt == DateTime.MinValue)
                    {
                        auditableEntity.Entity.DeletedAt = DateTime.UtcNow;
                        auditableEntity.Entity.LastModifiedBy = userId;
                        
                        // Log deletion event
                        _loggingService.LogDataAccess("delete", auditableEntity.Entity.GetType().Name, 
                            auditableEntity.Entity.Id, new { 
                                EntityType = auditableEntity.Entity.GetType().Name,
                                EntityId = auditableEntity.Entity.Id,
                                DeletedBy = userId,
                                DeletedAt = auditableEntity.Entity.DeletedAt
                            });
                    }
                }
                else
                {
                    // Regular update operation
                    auditableEntity.Entity.UpdatedAt = DateTime.UtcNow;
                    auditableEntity.Entity.LastModifiedBy = userId;
                    
                    // Log update event
                    _loggingService.LogDataAccess("update", auditableEntity.Entity.GetType().Name, 
                        auditableEntity.Entity.Id, new { 
                            EntityType = auditableEntity.Entity.GetType().Name,
                            EntityId = auditableEntity.Entity.Id,
                            UpdatedBy = userId,
                            UpdatedAt = auditableEntity.Entity.UpdatedAt
                        });
                }
            }
        }
    }

    private Guid GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        
        if (user != null && user.Identity?.IsAuthenticated == true)
        {
            var subClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(subClaim))
            {
                // Generate consistent Guid from Auth0 sub claim
                var hash = System.Security.Cryptography.SHA256.Create();
                var hashBytes = hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(subClaim));
                return new Guid(hashBytes.Take(16).ToArray());
            }
        }
        
        // Return a default system user ID if no authenticated user
        return Guid.Parse("00000000-0000-0000-0000-000000000000");
    }
}